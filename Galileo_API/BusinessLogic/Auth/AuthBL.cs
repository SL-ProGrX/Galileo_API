using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.Auth;

namespace Galileo.BusinessLogic.Auth;

public sealed class AuthBL
{
    private readonly LogonDB _logonDb;
    private readonly PerfilUsuarioDB _perfilDb;
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
        if (!CredentialsAreValid(username, request.Clave))
        {
            return InvalidCredentials();
        }

        var user = LoadUser(username);
        if (user is null)
        {
            return InvalidCredentials();
        }

        return await CompleteLoginAsync(user, application);
    }

    private bool CredentialsAreValid(string username, string password)
    {
        var login = _logonDb.LoginObtener(new LoginObtenerDto
        {
            Usuario = username,
            Clave = password,
        });

        return login.Code == 0;
    }

    private AuthUserDto? LoadUser(string username)
    {
        var profile = _perfilDb.UsuarioPerfilConsultar(username).Result;
        if (profile is null || profile.UserId is null || profile.UserId <= 0)
        {
            return null;
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
            return CreateAuthenticatedResponse(user, application);
        }

        return await CreateMfaChallengeAsync(user, application, tfa);
    }

    private async Task<AuthResponseDto> CreateMfaChallengeAsync(AuthUserDto user, string application, TfaData tfa)
    {
        var method = string.IsNullOrWhiteSpace(tfa.tfa_metodo) ? "UNKNOWN" : tfa.tfa_metodo.Trim().ToUpperInvariant();
        var challenge = _sessionStore.CreateChallenge(user, application, new[] { method });

        if (method != "MAIL")
        {
            return CreateMfaRequiredResponse(challenge);
        }

        var sent = await _logonDb.TFA_Codigo_EnviarMAIL(user.Usuario, tfa.email);
        return sent.Code == 0
            ? CreateMfaRequiredResponse(challenge)
            : new AuthResponseDto { Status = "authenticationUnavailable" };
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
            !_sessionStore.TryGetChallenge(request.ChallengeToken, AuthApplications.SSecurity, out var challenge))
        {
            return new AuthResponseDto { Status = "invalidChallenge" };
        }

        var validation = _logonDb.TFA_Codigo_Validar(challenge.User.Usuario, request.Codigo.Trim());
        if (validation.Code != 1)
        {
            _sessionStore.RegisterChallengeFailure(request.ChallengeToken, AuthApplications.SSecurity);
            return new AuthResponseDto { Status = "invalidCode" };
        }

        if (!_sessionStore.TryConsumeChallenge(request.ChallengeToken, AuthApplications.SSecurity, out _))
        {
            return new AuthResponseDto { Status = "invalidChallenge" };
        }

        return CreateAuthenticatedResponse(challenge.User, AuthApplications.SSecurity, "pwd,mfa");
    }

    public async Task<bool> ResendMfaAsync(MfaResendRequest request)
    {
        if (request is null || !_sessionStore.TryGetChallenge(request.ChallengeToken, AuthApplications.SSecurity, out var challenge) ||
            !challenge.Methods.Contains("MAIL", StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        var tfa = _logonDb.TFA_Data_Load(challenge.User.Usuario);
        var response = await _logonDb.TFA_Codigo_EnviarMAIL(challenge.User.Usuario, tfa.email);
        return response.Code == 0;
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
}
