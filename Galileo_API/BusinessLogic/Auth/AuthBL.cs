using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.Auth;
using Microsoft.Data.SqlClient;

namespace Galileo.BusinessLogic.Auth;

public sealed class AuthBL
{
    private readonly LogonDB _logonDb;
    private readonly PerfilUsuarioDB _perfilDb;
    private readonly SeguridadPortalDb _seguridadPortalDb;
    private readonly CambiarContrasenaDB _cambiarContrasenaDb;
    private readonly AccessTokenService _accessTokenService;
    private readonly AuthSessionStore _sessionStore;
    private readonly JwtDto _jwtSettings;

    public AuthBL(
        IConfiguration configuration,
        AccessTokenService accessTokenService,
        AuthSessionStore sessionStore)
    {
        _logonDb = new LogonDB(configuration);
        _perfilDb = new PerfilUsuarioDB(configuration);
        _seguridadPortalDb = new SeguridadPortalDb(configuration);
        _cambiarContrasenaDb = new CambiarContrasenaDB(configuration);
        _accessTokenService = accessTokenService;
        _sessionStore = sessionStore;
        _jwtSettings = configuration.GetSection("Jwt").Get<JwtDto>() ?? new JwtDto();
    }

    public async Task<AuthResponseDto> LoginAsync(AuthLoginRequest request, string application)
    {
        if (!HasCredentials(request))
        {
            return InvalidCredentials();
        }

        var username = request.Usuario.Trim();
        var credentialsError = ValidateCredentials(username, request.Clave);
        if (credentialsError is not null)
        {
            return credentialsError;
        }

        try
        {
            var user = LoadUser(username);
            return await CompleteLoginAsync(user, application);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return AuthenticationUnavailable(ex.Message);
        }
    }

    private AuthResponseDto? ValidateCredentials(string username, string password)
    {
        var login = _logonDb.LoginObtener(new LoginObtenerDto
        {
            Usuario = username,
            Clave = password,
        });

        return login.Code switch
        {
            0 => null,
            2 => InvalidCredentials(),
            _ => AuthenticationUnavailable(login.Description),
        };
    }

    private AuthUserDto LoadUser(string username)
    {
        var result = _perfilDb.UsuarioPerfilConsultar(username, propagarError: true);
        var profile = result.Result;
        if (profile is null || profile.UserId is null || profile.UserId <= 0)
        {
            throw new InvalidOperationException(result.Description ?? "No fue posible consultar el perfil del usuario autenticado.");
        }

        return new AuthUserDto
        {
            UserId = profile.UserId.Value,
            Usuario = string.IsNullOrWhiteSpace(profile.Usuario) ? username : profile.Usuario,
            Nombre = profile.Nombre,
        };
    }

    private async Task<AuthResponseDto> CompleteLoginAsync(AuthUserDto user, string application)
    {
        if (!IsSSecurity(application))
        {
            return CreateAuthenticatedResponse(user, application);
        }

        var tfa = _logonDb.TFA_Data_Load(user.Usuario);
        if (!tfa.tfa_ind)
        {
            return CompleteSSecurityAuthentication(user, "pwd");
        }

        return await CreateMfaChallengeAsync(user, application, tfa);
    }

    private async Task<AuthResponseDto> CreateMfaChallengeAsync(AuthUserDto user, string application, TfaData tfa)
    {
        var method = string.IsNullOrWhiteSpace(tfa.tfa_metodo) ? "UNKNOWN" : tfa.tfa_metodo.Trim().ToUpperInvariant();
        if (method != "MAIL")
        {
            return AuthenticationUnavailable($"El método de verificación {method} no está disponible.");
        }

        var sent = await _logonDb.TFA_Codigo_EnviarMAIL(user.Usuario, tfa.email);
        if (sent.Code != 0)
        {
            return AuthenticationUnavailable(sent.Description);
        }

        var challenge = _sessionStore.CreateChallenge(user, application, new[] { method });
        return CreateMfaRequiredResponse(challenge);
    }

    private static AuthResponseDto CreateMfaRequiredResponse(AuthSessionStore.ChallengeState challenge)
    {
        return new AuthResponseDto
        {
            Status = "mfaRequired",
            ChallengeToken = challenge.Token,
            ChallengeExpiresAtUtc = challenge.ExpiresAtUtc,
            Methods = challenge.Methods,
        };
    }

    private static bool HasCredentials(AuthLoginRequest request)
    {
        return request is not null &&
            !string.IsNullOrWhiteSpace(request.Usuario) &&
            !string.IsNullOrWhiteSpace(request.Clave);
    }

    private static bool IsSSecurity(string application)
    {
        return string.Equals(application, AuthApplications.SSecurity, StringComparison.OrdinalIgnoreCase);
    }

    public AuthResponseDto VerifyMfa(MfaVerifyRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.ChallengeToken) || string.IsNullOrWhiteSpace(request.Codigo) ||
            !_sessionStore.TryGetChallenge(request.ChallengeToken, AuthApplications.SSecurity, out var challenge) ||
            !string.Equals(challenge.Purpose, "mfa", StringComparison.Ordinal))
        {
            return new AuthResponseDto { Status = "invalidChallenge" };
        }

        var validation = _logonDb.TFA_Codigo_Validar(challenge.User.Usuario, request.Codigo.Trim());
        if (validation.Code is null or -1)
        {
            return AuthenticationUnavailable(validation.Description);
        }

        if (validation.Code == 3)
        {
            return new AuthResponseDto { Status = "invalidChallenge" };
        }

        if (validation.Code != 1)
        {
            _sessionStore.RegisterChallengeFailure(request.ChallengeToken, AuthApplications.SSecurity);
            return new AuthResponseDto { Status = "invalidCode" };
        }

        if (!_sessionStore.TryConsumeChallenge(request.ChallengeToken, AuthApplications.SSecurity, out _))
        {
            return new AuthResponseDto { Status = "invalidChallenge" };
        }

        try
        {
            return CompleteSSecurityAuthentication(challenge.User, "pwd,mfa");
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return AuthenticationUnavailable(ex.Message);
        }
    }

    public async Task<AuthResponseDto> ResendMfaAsync(MfaResendRequest request)
    {
        if (request is null || !_sessionStore.TryGetChallenge(request.ChallengeToken, AuthApplications.SSecurity, out var challenge) ||
            !string.Equals(challenge.Purpose, "mfa", StringComparison.Ordinal) ||
            !challenge.Methods.Contains("MAIL", StringComparer.OrdinalIgnoreCase))
        {
            return new AuthResponseDto { Status = "invalidChallenge" };
        }

        try
        {
            var tfa = _logonDb.TFA_Data_Load(challenge.User.Usuario);
            if (!tfa.tfa_ind || !string.Equals(tfa.tfa_metodo, "MAIL", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticationUnavailable("La configuración de correo para 2FA ya no está disponible.");
            }

            var response = await _logonDb.TFA_Codigo_EnviarMAIL(challenge.User.Usuario, tfa.email);
            return response.Code == 0
                ? new AuthResponseDto { Status = "mfaResent" }
                : AuthenticationUnavailable(response.Description);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return AuthenticationUnavailable(ex.Message);
        }
    }

    public AuthResponseDto CambiarContrasenaVencida(PasswordChangeRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.ChallengeToken) ||
            string.IsNullOrWhiteSpace(request.PassViejo) || string.IsNullOrWhiteSpace(request.NuevaContrasena) ||
            request.PassViejo.Length > 256 || request.NuevaContrasena.Length > 256 || request.Confirmacion.Length > 256 ||
            !string.Equals(request.NuevaContrasena, request.Confirmacion, StringComparison.Ordinal) ||
            !_sessionStore.TryGetChallenge(request.ChallengeToken, AuthApplications.SSecurity, out var challenge) ||
            !string.Equals(challenge.Purpose, "passwordChange", StringComparison.Ordinal))
        {
            return new AuthResponseDto { Status = "invalidChallenge" };
        }

        try
        {
            var passwordParameters = _cambiarContrasenaDb.ParametrosObtener();
            if (passwordParameters.Code < 0 || passwordParameters.Result is null)
            {
                throw new InvalidOperationException(passwordParameters.Description ?? "No fue posible consultar las políticas de contraseña.");
            }

            var policy = passwordParameters.Result;
            if (!CumplePoliticasContrasena(request.NuevaContrasena, policy))
            {
                return new AuthResponseDto
                {
                    Status = "passwordChangeFailed",
                    Detail = $"La nueva contraseña debe tener entre {policy.key_lenmin} y {policy.key_lenmax} caracteres y cumplir los requisitos de complejidad configurados.",
                };
            }

            var encodedNewPassword = EncodeLegacyPassword(request.NuevaContrasena);

            var history = _cambiarContrasenaDb.KeyHistoryObtener(
                challenge.User.Usuario,
                Math.Max(0, policy.key_history));
            if (history.Code < 0 || history.Result is null)
            {
                throw new InvalidOperationException(history.Description ?? "No fue posible consultar el historial de contraseñas.");
            }

            if (history.Result.Contains(encodedNewPassword, StringComparer.OrdinalIgnoreCase))
            {
                _sessionStore.RegisterChallengeFailure(request.ChallengeToken, AuthApplications.SSecurity);
                return new AuthResponseDto
                {
                    Status = "passwordChangeFailed",
                    Detail = "No puede reutilizar una de sus contraseñas recientes.",
                };
            }

            var rowsAffected = _cambiarContrasenaDb.CambiarClaveParaAutenticacion(new ClaveCambiarDto
            {
                Cliente = 1,
                Usuario = challenge.User.Usuario,
                PassViejo = request.PassViejo,
                PassNuevo = encodedNewPassword,
                Renueva = 1,
            });

            if (rowsAffected <= 0)
            {
                _sessionStore.RegisterChallengeFailure(request.ChallengeToken, AuthApplications.SSecurity);
                return new AuthResponseDto
                {
                    Status = "passwordChangeFailed",
                    Detail = "No fue posible cambiar la contraseña. Verifique la contraseña actual y las políticas de seguridad.",
                };
            }

            _sessionStore.TryConsumeChallenge(request.ChallengeToken, AuthApplications.SSecurity, out _);
            return new AuthResponseDto { Status = "passwordChanged" };
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return AuthenticationUnavailable(ex.Message);
        }
    }

    private static bool CumplePoliticasContrasena(string password, ParametrosObtenerDto policy)
    {
        if (password.Length < Math.Max(1, policy.key_lenmin) || password.Length > policy.key_lenmax)
        {
            return false;
        }

        return (!policy.key_capchar || password.Any(char.IsUpper)) &&
            (!policy.key_numchar || password.Any(char.IsDigit)) &&
            (!policy.key_simchar || password.Any(character => "#$@^&*()".Contains(character)));
    }

    private static string EncodeLegacyPassword(string password)
    {
        // Mantiene compatibilidad con UtilitiesService.generateFixedHash, usado por VB6.
        var digits = new System.Text.StringBuilder();
        for (var index = password.Length - 1; index >= 0; index--)
        {
            var codePoint = (int)password[index];
            if (char.IsHighSurrogate(password[index]) && index + 1 < password.Length && char.IsLowSurrogate(password[index + 1]))
            {
                codePoint = char.ConvertToUtf32(password[index], password[index + 1]);
            }

            digits.Append(codePoint.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        var transformed = new System.Text.StringBuilder();
        var offsets = new[] { 1, -5, 7, -13, -2, 3 };
        for (var index = 0; index < digits.Length; index += 3)
        {
            var length = Math.Min(3, digits.Length - index);
            if (!int.TryParse(digits.ToString(index, length), out var number))
            {
                continue;
            }

            transformed.Append((number + offsets[(index / 3) % offsets.Length]).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        var result = new System.Text.StringBuilder();
        for (var index = 0; index < transformed.Length; index += 2)
        {
            var length = Math.Min(2, transformed.Length - index);
            if (int.TryParse(transformed.ToString(index, length), out var characterCode) &&
                characterCode > 31 && characterCode != 39 && characterCode != 34)
            {
                result.Insert(0, char.ConvertFromUtf32(characterCode));
            }
        }

        return result.ToString();
    }

    private AuthResponseDto CompleteSSecurityAuthentication(AuthUserDto user, string authenticationMethod)
    {
        var bloqueo = _seguridadPortalDb.UsuarioBloqueoObtener(user.Usuario);
        if (bloqueo.Bloqueo == 1)
        {
            return new AuthResponseDto
            {
                Status = "accountBlocked",
                Detail = "Su cuenta se encuentra bloqueada. Espere el desbloqueo automático o comuníquese con el administrador de sistemas.",
            };
        }

        var condicion = _seguridadPortalDb.UsuarioCondicionObtener(user.Usuario);
        var vencimiento = _seguridadPortalDb.UsuarioVencimientoObtener(user.Usuario);
        if (condicion.KEY_RENEW_SESION == 1 || vencimiento.Vencida == 1)
        {
            var passwordParameters = _cambiarContrasenaDb.ParametrosObtener();
            if (passwordParameters.Code < 0 || passwordParameters.Result is null)
            {
                throw new InvalidOperationException(passwordParameters.Description ?? "No fue posible consultar las políticas de contraseña.");
            }

            var challenge = _sessionStore.CreateChallenge(
                user,
                AuthApplications.SSecurity,
                Array.Empty<string>(),
                "passwordChange");

            return new AuthResponseDto
            {
                Status = "passwordChangeRequired",
                Detail = condicion.KEY_RENEW_SESION == 1
                    ? "Debe renovar su contraseña para continuar."
                    : "Su contraseña venció. Debe cambiarla para continuar.",
                ChallengeToken = challenge.Token,
                ChallengeExpiresAtUtc = challenge.ExpiresAtUtc,
                PasswordPolicy = passwordParameters.Result,
            };
        }

        var response = CreateAuthenticatedResponse(user, AuthApplications.SSecurity, authenticationMethod);
        if (vencimiento.Renovacion == 1)
        {
            var periodo = vencimiento.Dias == 0 ? "hasta hoy" : $"en {vencimiento.Dias} día(s)";
            response = new AuthResponseDto
            {
                Status = response.Status,
                AccessToken = response.AccessToken,
                ExpiresAtUtc = response.ExpiresAtUtc,
                User = response.User,
                RefreshToken = response.RefreshToken,
                PasswordExpiryNotice = $"Su contraseña está próxima a vencer ({periodo}). Cámbiela antes de que caduque.",
            };
        }

        return response;
    }

    public bool TryRefresh(string refreshToken, string application, out AuthSessionResponse response, out string replacementRefreshToken)
    {
        response = default!;
        replacementRefreshToken = string.Empty;

        if (!_sessionStore.TryRotateRefreshToken(refreshToken, application, out var session, out replacementRefreshToken))
        {
            return false;
        }

        response = _accessTokenService.CreateAccessToken(session.User, application, session.SessionId, session.AuthenticationMethod);
        return true;
    }

    public void RevokeRefreshToken(string refreshToken)
    {
        _sessionStore.Revoke(refreshToken);
    }

    private AuthResponseDto CreateAuthenticatedResponse(AuthUserDto user, string application, string authenticationMethod = "pwd")
    {
        var refreshDays = Math.Clamp(_jwtSettings.RefreshTokenDays, 1, 30);
        var session = _sessionStore.CreateSession(user, application, TimeSpan.FromDays(refreshDays), authenticationMethod);
        var token = _accessTokenService.CreateAccessToken(user, application, session.SessionId, authenticationMethod);

        return new AuthResponseDto
        {
            Status = "authenticated",
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = token.User,
            RefreshToken = session.RefreshToken,
        };
    }

    private static AuthResponseDto InvalidCredentials()
    {
        return new AuthResponseDto { Status = "invalidCredentials" };
    }

    private static AuthResponseDto AuthenticationUnavailable(string? detail)
    {
        return new AuthResponseDto
        {
            Status = "authenticationUnavailable",
            Detail = string.IsNullOrWhiteSpace(detail)
                ? "No fue posible completar la autenticación."
                : detail,
        };
    }
}
