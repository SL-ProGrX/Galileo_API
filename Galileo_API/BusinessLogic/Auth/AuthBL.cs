using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.Auth;
using Microsoft.Data.SqlClient;
using System.Net;

namespace Galileo.BusinessLogic.Auth;

public sealed class AuthBL
{
    private const string InvalidChallengeStatus = "invalidChallenge";
    private const string InvalidCodeStatus = "invalidCode";
    private const string AuthenticationUnavailableStatus = "authenticationUnavailable";
    private const string PasswordChangeFailedStatus = "passwordChangeFailed";
    private const string PolicyLookupFailureMessage = "No fue posible consultar las políticas de contraseña.";

    private readonly LogonDB _logonDb;
    private readonly PerfilUsuarioDB _perfilDb;
    private readonly SeguridadPortalDb _seguridadPortalDb;
    private readonly CambiarContrasenaDB _cambiarContrasenaDb;
    private readonly AccessTokenService _accessTokenService;
    private readonly AuthSessionStore _sessionStore;
    private readonly JwtDto _jwtSettings;
    private readonly string _galileoAppName;

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
        _galileoAppName = configuration["AppSettings:GalileoAppName"] ?? "Galileo";
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

    public AuthResponseDto PreInicializarGalileo(string username, string appVersion)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(appVersion) ||
            appVersion.Length > 50)
        {
            return new AuthResponseDto
            {
                Status = AuthenticationUnavailableStatus,
                Detail = "No fue posible validar los datos de inicialización de Galileo.",
            };
        }

        username = username.Trim();
        try
        {
            return ValidarUsuarioYAplicacionGalileo(username, appVersion);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException or ArgumentException)
        {
            return AuthenticationUnavailable(ex.Message);
        }
    }

    public AuthResponseDto InicializarGalileo(
        string username,
        GalileoSecurityInitializeRequest request,
        Guid deviceId,
        string remoteIp)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            request is null ||
            request.EmpresaId <= 0 ||
            string.IsNullOrWhiteSpace(request.AppVersion) ||
            request.AppVersion.Length > 50 ||
            deviceId == Guid.Empty ||
            !IPAddress.TryParse(remoteIp, out var parsedIp))
        {
            return new AuthResponseDto
            {
                Status = AuthenticationUnavailableStatus,
                Detail = "No fue posible validar los datos de inicialización de Galileo.",
            };
        }

        username = username.Trim();
        var ipAddress = parsedIp.IsIPv4MappedToIPv6
            ? parsedIp.MapToIPv4().ToString()
            : parsedIp.ToString();

        try
        {
            var empresas = _logonDb.ClientesObtener(username);
            if (empresas.Code < 0 || empresas.Result is null)
            {
                throw new InvalidOperationException(
                    empresas.Description ?? "No fue posible consultar las empresas del usuario.");
            }

            var tieneAccesoEmpresa = empresas.Result.Any(empresa =>
                (int.TryParse(empresa.Cod_Empresa, out var codEmpresa) && codEmpresa == request.EmpresaId) ||
                (int.TryParse(empresa.CodEmpresa, out codEmpresa) && codEmpresa == request.EmpresaId));
            if (!tieneAccesoEmpresa)
            {
                return new AuthResponseDto
                {
                    Status = "accessDenied",
                    Detail = "El usuario no tiene acceso a la empresa seleccionada.",
                };
            }

            var seguridad = ValidarUsuarioYAplicacionGalileo(username, request.AppVersion);
            if (seguridad.Status != "ready")
            {
                return seguridad;
            }

            var accessLimit = _seguridadPortalDb.RegistrarDispositivoWeb(
                request.EmpresaId,
                username,
                deviceId,
                ipAddress,
                request.AppVersion);
            if (accessLimit.Indicador != 1)
            {
                return new AuthResponseDto
                {
                    Status = "accessDenied",
                    Detail = $"Limitación de acceso: {accessLimit.Notas}",
                    PasswordExpiryNotice = seguridad.PasswordExpiryNotice,
                };
            }

            return new AuthResponseDto
            {
                Status = "ready",
                AppStatus = seguridad.AppStatus,
                PasswordExpiryNotice = seguridad.PasswordExpiryNotice,
            };
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException or ArgumentException)
        {
            return AuthenticationUnavailable(ex.Message);
        }
    }

    private AuthResponseDto ValidarUsuarioYAplicacionGalileo(string username, string appVersion)
    {
        var bloqueo = _seguridadPortalDb.UsuarioBloqueoObtener(username);
        if (bloqueo.Bloqueo == 1)
        {
            return new AuthResponseDto
            {
                Status = "accountBlocked",
                Detail = "Su contraseña se encuentra bloqueada. Espere el desbloqueo automático o comuníquese con su administrador de sistemas.",
            };
        }

        var condicion = _seguridadPortalDb.UsuarioCondicionObtener(username);
        if (condicion.KEY_RENEW_SESION == 1)
        {
            return new AuthResponseDto
            {
                Status = "passwordChangeRequired",
                Detail = "Debe renovar su contraseña para continuar.",
            };
        }

        var vencimiento = _seguridadPortalDb.UsuarioVencimientoObtener(username);
        if (vencimiento.Vencida == 1)
        {
            return new AuthResponseDto
            {
                Status = "passwordChangeRequired",
                Detail = "Su contraseña ya se encuentra vencida. Debe cambiarla para continuar.",
            };
        }

        var passwordExpiryNotice = vencimiento.Renovacion == 1
            ? CrearAvisoVencimiento(vencimiento.Dias)
            : null;
        var appStatus = _seguridadPortalDb.AppStatusCompletoObtener(_galileoAppName, appVersion);
        if (appStatus.Pasa != 1)
        {
            return new AuthResponseDto
            {
                Status = "applicationBlocked",
                Detail = $"El sistema: {_galileoAppName} versión [{appVersion}] no tiene acceso. Nota: {appStatus.Notas}",
                PasswordExpiryNotice = passwordExpiryNotice,
            };
        }

        return new AuthResponseDto
        {
            Status = "ready",
            AppStatus = appStatus.Version_Status,
            PasswordExpiryNotice = passwordExpiryNotice,
        };
    }

    private static string CrearAvisoVencimiento(int dias)
    {
        var periodo = dias switch
        {
            0 => "hoy (0 días)",
            1 => "en 1 día",
            _ => $"en {dias} días",
        };

        return $"Su contraseña vence {periodo}. Cámbiela antes de que caduque.";
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
            return new AuthResponseDto { Status = InvalidChallengeStatus };
        }

        var validation = _logonDb.TFA_Codigo_Validar(challenge.User.Usuario, request.Codigo.Trim());
        if (validation.Code is null or -1)
        {
            return AuthenticationUnavailable(validation.Description);
        }

        if (validation.Code == 3)
        {
            return new AuthResponseDto { Status = InvalidChallengeStatus };
        }

        if (validation.Code != 1)
        {
            _sessionStore.RegisterChallengeFailure(request.ChallengeToken, AuthApplications.SSecurity);
            return new AuthResponseDto { Status = InvalidCodeStatus };
        }

        if (!_sessionStore.TryConsumeChallenge(request.ChallengeToken, AuthApplications.SSecurity, out _))
        {
            return new AuthResponseDto { Status = InvalidChallengeStatus };
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
            return new AuthResponseDto { Status = InvalidChallengeStatus };
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
            return new AuthResponseDto { Status = InvalidChallengeStatus };
        }

        try
        {
            var passwordParameters = _cambiarContrasenaDb.ParametrosObtener();
            if (passwordParameters.Code < 0 || passwordParameters.Result is null)
            {
                throw new InvalidOperationException(passwordParameters.Description ?? PolicyLookupFailureMessage);
            }

            var policy = passwordParameters.Result;
            if (!CumplePoliticasContrasena(request.NuevaContrasena, policy))
            {
                return new AuthResponseDto
                {
                    Status = PasswordChangeFailedStatus,
                    Detail = $"La nueva contraseña debe tener entre {policy.key_lenmin} y {policy.key_lenmax} caracteres y cumplir los requisitos de complejidad configurados.",
                };
            }

            var encodedNewPassword = EncodeLegacyPassword(request.NuevaContrasena);
            var encodedCurrentPassword = EncodeLegacyPassword(request.PassViejo);
            var credentialsError = ValidateCredentials(challenge.User.Usuario, encodedCurrentPassword);
            if (credentialsError is not null)
            {
                if (credentialsError.Status == AuthenticationUnavailableStatus)
                {
                    return credentialsError;
                }

                _sessionStore.RegisterChallengeFailure(request.ChallengeToken, AuthApplications.SSecurity);
                return new AuthResponseDto
                {
                    Status = PasswordChangeFailedStatus,
                    Detail = "La contraseña actual no corresponde a la cuenta.",
                };
            }

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
                    Status = PasswordChangeFailedStatus,
                    Detail = "No puede reutilizar una de sus contraseñas recientes.",
                };
            }

            var passwordChanged = _cambiarContrasenaDb.CambiarClaveParaAutenticacion(new ClaveCambiarDto
            {
                Cliente = 1,
                Usuario = challenge.User.Usuario,
                PassViejo = encodedCurrentPassword,
                PassNuevo = encodedNewPassword,
                Renueva = 0,
            });

            if (!passwordChanged)
            {
                _sessionStore.RegisterChallengeFailure(request.ChallengeToken, AuthApplications.SSecurity);
                return new AuthResponseDto
                {
                    Status = PasswordChangeFailedStatus,
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

    public AuthResponseDto ObtenerPoliticaRecuperacionGalileo(GalileoPasswordRecoveryTokenRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Usuario) ||
            string.IsNullOrWhiteSpace(request.Token) || request.Usuario.Length > 50 || request.Token.Length > 256)
        {
            return new AuthResponseDto { Status = InvalidCodeStatus };
        }

        try
        {
            var usuario = request.Usuario.Trim();
            var tokenStatus = _cambiarContrasenaDb.ValidarTokenParaRecuperacion(usuario, request.Token.Trim());
            if (tokenStatus != 1)
            {
                return new AuthResponseDto { Status = tokenStatus == -1 ? "expiredCode" : InvalidCodeStatus };
            }

            var parameters = _cambiarContrasenaDb.ParametrosObtener();
            if (parameters.Code < 0 || parameters.Result is null)
            {
                throw new InvalidOperationException(parameters.Description ?? PolicyLookupFailureMessage);
            }

            return new AuthResponseDto { Status = "recoveryReady", PasswordPolicy = parameters.Result };
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return AuthenticationUnavailable(ex.Message);
        }
    }

    public AuthResponseDto RecuperarContrasenaGalileo(GalileoPasswordRecoveryRequest request)
    {
        if (request is null || TieneDatosRecuperacionInvalidos(request))
        {
            return new AuthResponseDto { Status = PasswordChangeFailedStatus, Detail = "Verifique el usuario, el token y la nueva contraseña." };
        }

        var usuario = request.Usuario.Trim();
        var token = request.Token.Trim();
        try
        {
            var tokenStatus = _cambiarContrasenaDb.ValidarTokenParaRecuperacion(usuario, token);
            if (tokenStatus != 1)
            {
                return new AuthResponseDto { Status = tokenStatus == -1 ? "expiredCode" : InvalidCodeStatus };
            }

            var parameters = _cambiarContrasenaDb.ParametrosObtener();
            if (parameters.Code < 0 || parameters.Result is null)
            {
                throw new InvalidOperationException(parameters.Description ?? PolicyLookupFailureMessage);
            }

            var policy = parameters.Result;
            if (!CumplePoliticasContrasena(request.NuevaContrasena, policy))
            {
                return new AuthResponseDto
                {
                    Status = PasswordChangeFailedStatus,
                    Detail = $"La nueva contraseña debe tener entre {policy.key_lenmin} y {policy.key_lenmax} caracteres y cumplir los requisitos de complejidad configurados.",
                };
            }

            var encodedNewPassword = EncodeLegacyPassword(request.NuevaContrasena);
            var history = _cambiarContrasenaDb.KeyHistoryObtener(usuario, Math.Max(0, policy.key_history));
            if (history.Code < 0 || history.Result is null)
            {
                throw new InvalidOperationException(history.Description ?? "No fue posible consultar el historial de contraseñas.");
            }

            if (history.Result.Contains(encodedNewPassword, StringComparer.OrdinalIgnoreCase))
            {
                return new AuthResponseDto
                {
                    Status = PasswordChangeFailedStatus,
                    Detail = "No puede reutilizar una de sus contraseñas recientes.",
                };
            }

            var changeStatus = _cambiarContrasenaDb.CambiarClaveParaRecuperacion(new ClaveCambiarDto
            {
                Cliente = 1,
                Usuario = usuario,
                PassViejo = string.Empty,
                PassNuevo = encodedNewPassword,
                Renueva = 0,
            }, token);
            return new AuthResponseDto
            {
                Status = changeStatus switch
                {
                    1 => "passwordChanged",
                    -1 => "expiredCode",
                    _ => InvalidCodeStatus,
                },
            };
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            return AuthenticationUnavailable(ex.Message);
        }
    }

    private static bool TieneDatosRecuperacionInvalidos(GalileoPasswordRecoveryRequest request)
    {
        return string.IsNullOrWhiteSpace(request.Usuario) ||
            string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.NuevaContrasena) ||
            string.IsNullOrWhiteSpace(request.Confirmacion) ||
            request.Usuario.Length > 50 || request.Token.Length > 256 ||
            request.NuevaContrasena.Length > 256 || request.Confirmacion.Length > 256 ||
            !string.Equals(request.NuevaContrasena, request.Confirmacion, StringComparison.Ordinal);
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
        // Mantiene compatibilidad con UtilitiesService.generateFixedHash del login de Galileo.
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
        for (var index = 0; index < transformed.Length; index++)
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
                throw new InvalidOperationException(passwordParameters.Description ?? PolicyLookupFailureMessage);
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
            response = new AuthResponseDto
            {
                Status = response.Status,
                AccessToken = response.AccessToken,
                ExpiresAtUtc = response.ExpiresAtUtc,
                User = response.User,
                RefreshToken = response.RefreshToken,
                PasswordExpiryNotice = CrearAvisoVencimiento(vencimiento.Dias),
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
            Status = AuthenticationUnavailableStatus,
            Detail = string.IsNullOrWhiteSpace(detail)
                ? "No fue posible completar la autenticación."
                : detail,
        };
    }
}
