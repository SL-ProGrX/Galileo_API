using Galileo.BusinessLogic.Auth;
using Galileo.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Galileo.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public sealed class AuthController : ControllerBase
{
    private const string GalileoRefreshCookie = "pgx_galileo_refresh";
    private const string SSecurityRefreshCookie = "pgx_ssecurity_refresh";
    private const string GalileoDeviceCookie = "pgx_galileo_device";
    private const string AuthenticationUnavailableStatus = "authenticationUnavailable";
    private const string InvalidApplicationStatus = "invalidApplication";
    private const string EstadoCambioContrasenaRequerido = "passwordChangeRequired";
    private const string AccountBlockedStatus = "accountBlocked";
    private const string SessionsUnavailableStatus = "sessionsUnavailable";
    private readonly AuthBL _auth;
    private readonly ILogger<AuthController>? _logger;
    private readonly int _refreshTokenDays;

    public AuthController(
        AuthBL auth,
        IConfiguration configuration,
        ILogger<AuthController>? logger = null)
    {
        _auth = auth;
        _logger = logger;
        _refreshTokenDays = Math.Clamp(configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 7, 1, 30);
    }

    [AllowAnonymous]
    [HttpPost("Galileo/Login")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AuthResponseDto>> GalileoLogin(
        [FromBody] AuthLoginRequest request,
        [FromHeader(Name = "User-Agent")] string? userAgent,
        [FromHeader(Name = "X-Client-Platform")] string? clientPlatform)
    {
        return await Login(request, AuthApplications.Galileo, userAgent, clientPlatform);
    }

    [AllowAnonymous]
    [HttpPost("SSecurity/Login")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AuthResponseDto>> SSecurityLogin(
        [FromBody] AuthLoginRequest request,
        [FromHeader(Name = "User-Agent")] string? userAgent,
        [FromHeader(Name = "X-Client-Platform")] string? clientPlatform)
    {
        return await Login(request, AuthApplications.SSecurity, userAgent, clientPlatform);
    }

    [AllowAnonymous]
    [HttpPost("Galileo/SwitchPreview")]
    [HttpPost("SSecurity/SwitchPreview")]
    [ValidateAntiForgeryToken]
    public ActionResult<AuthResponseDto> SwitchPreview([FromBody] AuthLoginRequest request)
    {
        var response = _auth.PreviewAccountSwitch(request);
        return response.Status switch
        {
            "switchPreview" => Ok(response),
            AuthenticationUnavailableStatus => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => Unauthorized(response),
        };
    }

    [Authorize]
    [HttpPost("Galileo/PreInitialize")]
    [ValidateAntiForgeryToken]
    public ActionResult<AuthResponseDto> PreInitializeGalileo([FromBody] GalileoSecurityPreInitializeRequest request)
    {
        if (!string.Equals(User.FindFirst("app")?.Value, AuthApplications.Galileo, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var username = User.FindFirst("UserName")?.Value;
        if (string.IsNullOrWhiteSpace(username) ||
            request is null ||
            string.IsNullOrWhiteSpace(request.AppVersion) ||
            request.AppVersion.Length > 50)
        {
            return BadRequest(new AuthResponseDto
            {
                Status = AuthenticationUnavailableStatus,
                Detail = "La solicitud de preinicialización de Galileo no es válida.",
            });
        }

        var response = _auth.PreInicializarGalileo(username, request.AppVersion);
        return response.Status switch
        {
            "ready" or EstadoCambioContrasenaRequerido or AccountBlockedStatus or "applicationBlocked" => Ok(response),
            AuthenticationUnavailableStatus => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => BadRequest(response),
        };
    }

    [Authorize]
    [HttpPost("Galileo/Initialize")]
    [ValidateAntiForgeryToken]
    public ActionResult<AuthResponseDto> InitializeGalileo([FromBody] GalileoSecurityInitializeRequest request)
    {
        if (!string.Equals(User.FindFirst("app")?.Value, AuthApplications.Galileo, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var username = User.FindFirst("UserName")?.Value;
        if (string.IsNullOrWhiteSpace(username) ||
            request is null ||
            request.EmpresaId <= 0 ||
            string.IsNullOrWhiteSpace(request.AppVersion) ||
            request.AppVersion.Length > 50)
        {
            return BadRequest(new AuthResponseDto
            {
                Status = AuthenticationUnavailableStatus,
                Detail = "La solicitud de inicialización de Galileo no es válida.",
            });
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new AuthResponseDto
            {
                Status = AuthenticationUnavailableStatus,
                Detail = "No fue posible identificar la dirección IP de origen.",
            });
        }

        var deviceCookie = Request.Cookies[GalileoDeviceCookie];
        var deviceId = Guid.TryParse(deviceCookie, out var registeredDeviceId)
            ? registeredDeviceId
            : Guid.NewGuid();
        var response = _auth.InicializarGalileo(
            username,
            request,
            deviceId,
            remoteIp.ToString());

        if (response.Status is "ready" or EstadoCambioContrasenaRequerido)
        {
            Response.Cookies.Append(
                GalileoDeviceCookie,
                deviceId.ToString("D"),
                DeviceCookieOptions());
        }

        return response.Status switch
        {
            "ready" or EstadoCambioContrasenaRequerido or AccountBlockedStatus or "applicationBlocked" or "accessDenied" => Ok(response),
            AuthenticationUnavailableStatus => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => BadRequest(response),
        };
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("{application}/Mfa/Verify")]
    public ActionResult<AuthResponseDto> VerifyMfa(
        [FromRoute] string application,
        [FromBody] MfaVerifyRequest request,
        [FromHeader(Name = "User-Agent")] string? userAgent,
        [FromHeader(Name = "X-Client-Platform")] string? clientPlatform)
    {
        if (!IsSupportedApplication(application)) return BadRequest(new { status = InvalidApplicationStatus });

        var response = _auth.VerifyMfa(request, application, GetSessionClientInfo(userAgent, clientPlatform));
        if (response.Status == "authenticated")
        {
            RevokePreviousBrowserSession(application);
            SetRefreshCookie(application, response.RefreshToken);
            return Ok(response);
        }

        return response.Status switch
        {
            "invalidCode" or "invalidChallenge" or AccountBlockedStatus or EstadoCambioContrasenaRequerido => Ok(response),
            "accessDenied" => StatusCode(StatusCodes.Status403Forbidden, response),
            AuthenticationUnavailableStatus => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => Unauthorized(response),
        };
    }

    [AllowAnonymous]
    [HttpPost("SSecurity/Password/Change")]
    [ValidateAntiForgeryToken]
    public ActionResult<AuthResponseDto> ChangeExpiredPassword([FromBody] PasswordChangeRequest request)
    {
        var response = _auth.CambiarContrasenaVencida(request);
        return response.Status switch
        {
            "passwordChanged" or "passwordChangeFailed" or "invalidChallenge" => Ok(response),
            AuthenticationUnavailableStatus => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => BadRequest(response),
        };
    }

    [AllowAnonymous]
    [HttpPost("Galileo/Password/Recovery/Policy")]
    [ValidateAntiForgeryToken]
    public ActionResult<AuthResponseDto> GalileoRecoveryPolicy([FromBody] GalileoPasswordRecoveryTokenRequest request)
    {
        var response = _auth.ObtenerPoliticaRecuperacionGalileo(request);
        return response.Status == AuthenticationUnavailableStatus
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, response)
            : Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("Galileo/Password/Recovery/Change")]
    [ValidateAntiForgeryToken]
    public ActionResult<AuthResponseDto> GalileoRecoveryChange([FromBody] GalileoPasswordRecoveryRequest request)
    {
        var response = _auth.RecuperarContrasenaGalileo(request);
        return response.Status == AuthenticationUnavailableStatus
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, response)
            : Ok(response);
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("{application}/Mfa/Resend")]
    public async Task<IActionResult> ResendMfa([FromRoute] string application, [FromBody] MfaResendRequest request)

    {
        if (!IsSupportedApplication(application)) return BadRequest(new { status = InvalidApplicationStatus });

        var response = await _auth.ResendMfaAsync(request, application);
        return response.Status switch
        {
            "mfaResent" => NoContent(),
            "mfaResendCooldown" => StatusCode(StatusCodes.Status429TooManyRequests, response),
            "invalidChallenge" => BadRequest(response),
            _ => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
        };
    }

    [AllowAnonymous]
    [HttpPost("Refresh")]
    [ValidateAntiForgeryToken]
    public ActionResult<AuthSessionResponse> Refresh(
        [FromQuery] string application,
        [FromHeader(Name = "X-Auth-Refresh")] string? csrfHeader)
    {
        if (!IsSupportedApplication(application)) return BadRequest(new { status = InvalidApplicationStatus });

        if (!string.Equals(csrfHeader, "1", StringComparison.Ordinal))
        {
            return BadRequest(new { status = "invalidRefreshRequest" });
        }

        var cookieName = GetCookieName(application);
        if (!Request.Cookies.TryGetValue(cookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new { status = "invalidRefreshToken" });
        }

        if (!_auth.TryRefresh(refreshToken, application, out var response, out var replacementRefreshToken))
        {
            // Otra pestaña puede haber rotado la cookie mientras esta solicitud estaba en curso.
            // Borrarla aquí eliminaría también la cookie nueva compartida por el navegador.
            return Unauthorized(new { status = "invalidRefreshToken" });
        }

        SetRefreshCookie(application, replacementRefreshToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth-activity")]
    [HttpGet("SessionStatus")]
    public ActionResult<AuthSessionStatusDto> SessionStatus([FromQuery] string application)
    {
        if (!IsSupportedApplication(application)) return BadRequest(new { status = InvalidApplicationStatus });

        var cookieName = GetCookieName(application);
        if (!Request.Cookies.TryGetValue(cookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Ok(new AuthSessionStatusDto
            {
                Status = "ended",
                Reason = AuthSessionEndReasons.SessionNotFound,
            });
        }

        try
        {
            return Ok(_auth.GetSessionStatus(refreshToken, application));
        }
        catch (Exception ex) when (ex is InvalidOperationException or Microsoft.Data.SqlClient.SqlException)
        {
            _logger?.LogError(ex, "No fue posible verificar la sesión del usuario autenticado.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { status = "sessionStatusUnavailable" });
        }
    }

    [Authorize]
    [EnableRateLimiting("auth-activity")]
    [HttpPost("Activity")]
    [ValidateAntiForgeryToken]
    public IActionResult Activity([FromQuery] string application)
    {
        if (!IsSupportedApplication(application)) return BadRequest(new { status = InvalidApplicationStatus });

        var sessionId = User.FindFirst("sid")?.Value;
        if (string.IsNullOrWhiteSpace(sessionId) ||
            !string.Equals(User.FindFirst("app")?.Value, application, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        return _auth.RecordActivity(sessionId, application)
            ? NoContent()
            : Unauthorized(new { status = "sessionExpired" });
    }

    [Authorize]
    [HttpGet("Sessions")]
    public ActionResult<IReadOnlyList<UserSessionDto>> GetSessions()
    {
        if (!TryGetSessionIdentity(out var userId, out var currentSessionId, out var application)) return Forbid();
        try
        {
            return Ok(_auth.ListActiveSessions(userId, currentSessionId, application));
        }
        catch (Exception ex) when (ex is InvalidOperationException or Microsoft.Data.SqlClient.SqlException)
        {
            _logger?.LogError(ex, "No fue posible consultar las sesiones del usuario autenticado.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { status = SessionsUnavailableStatus, detail = "No fue posible consultar las sesiones." });
        }
    }

    [Authorize]
    [HttpDelete("Sessions/{sessionId}")]
    [ValidateAntiForgeryToken]
    public IActionResult RevokeSession(string sessionId)
    {
        if (!TryGetSessionIdentity(out var userId, out var currentSessionId, out var application)) return Forbid();
        if (sessionId.Length != 32 || !sessionId.All(Uri.IsHexDigit)) return BadRequest();
        if (string.Equals(sessionId, currentSessionId, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { status = "useLogoutForCurrentSession" });
        }
        try
        {
            return _auth.RevokeSession(userId, sessionId, application) ? NoContent() : NotFound();
        }
        catch (Exception ex) when (ex is InvalidOperationException or Microsoft.Data.SqlClient.SqlException)
        {
            _logger?.LogError(ex, "No fue posible cerrar una sesión del usuario autenticado.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { status = SessionsUnavailableStatus, detail = "No fue posible cerrar la sesión." });
        }
    }

    [Authorize]
    [HttpGet("Admin/Sessions")]
    public ActionResult<AdminSessionPageDto> GetAllSessions([FromQuery] AdminSessionQueryDto query)
    {
        try
        {
            if (!TryGetPortalAdminIdentity(out _, out _, out var currentSessionId))
            {
                return Forbid();
            }

            var validationMessage = ValidateAdminSessionQuery(query);
            if (validationMessage is not null)
            {
                return BadRequest(new { status = "invalidSessionQuery", detail = validationMessage });
            }

            return Ok(_auth.ListAllSessions(query, currentSessionId));
        }
        catch (Exception ex) when (ex is InvalidOperationException or Microsoft.Data.SqlClient.SqlException)
        {
            _logger?.LogError(ex, "No fue posible consultar el control global de sesiones.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { status = SessionsUnavailableStatus, detail = ex.GetBaseException().Message });
        }
    }

    [Authorize]
    [HttpDelete("Admin/Sessions/{sessionId}")]
    [ValidateAntiForgeryToken]
    public IActionResult RevokeAnySession(string sessionId, [FromBody] AdminSessionRevokeRequestDto request)
    {
        try
        {
            if (!TryGetPortalAdminIdentity(out var adminUserId, out var adminUserName, out var currentSessionId))
            {
                return Forbid();
            }

            if (sessionId.Length != 32 || !sessionId.All(Uri.IsHexDigit)) return BadRequest();
            if (string.Equals(sessionId, currentSessionId, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { status = "useLogoutForCurrentSession" });
            }

            var application = request.Application?.Trim().ToLowerInvariant() ?? string.Empty;
            if (!IsSupportedApplication(application))
            {
                return BadRequest(new { status = "invalidApplication" });
            }

            var reason = request.Reason?.Trim() ?? string.Empty;
            if (reason.Length is < 5 or > 300)
            {
                return BadRequest(new { status = "invalidReason", detail = "Indique un motivo entre 5 y 300 caracteres." });
            }

            var result = _auth.RevokeAllSession(
                sessionId,
                application,
                adminUserId,
                adminUserName,
                reason);

            return result.Status switch
            {
                "revoked" => NoContent(),
                "already-ended" => Conflict(new { status = result.Status }),
                _ => NotFound(new { status = "sessionNotFound" }),
            };
        }
        catch (Exception ex) when (ex is InvalidOperationException or Microsoft.Data.SqlClient.SqlException)
        {
            _logger?.LogError(ex, "No fue posible revocar una sesión desde el control global.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { status = SessionsUnavailableStatus, detail = ex.GetBaseException().Message });
        }
    }

    private bool TryGetPortalAdminIdentity(out int userId, out string userName, out string sessionId)
    {
        var rawUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        userName = User.FindFirst("UserName")?.Value ?? string.Empty;
        sessionId = User.FindFirst("sid")?.Value ?? string.Empty;
        var application = User.FindFirst("app")?.Value ?? string.Empty;
        if (!int.TryParse(rawUserId, out userId) || userId <= 0 ||
            string.IsNullOrWhiteSpace(userName) || sessionId.Length != 32 ||
            !string.Equals(application, AuthApplications.SSecurity, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return _auth.IsPortalAdministrator(userName);
    }

    private static string? ValidateAdminSessionQuery(AdminSessionQueryDto query)
    {
        query.Application = query.Application?.Trim().ToLowerInvariant() ?? string.Empty;
        query.Status = query.Status?.Trim().ToLowerInvariant() ?? string.Empty;
        query.Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        if (query.Page is < 1 or > 100_000 || query.PageSize is not (10 or 25 or 30 or 50 or 100))
        {
            return "La página o la cantidad de resultados no es válida.";
        }

        if (query.Application is not ("all" or AuthApplications.Galileo or AuthApplications.SSecurity))
        {
            return "La aplicación indicada no es válida.";
        }

        if (query.Status is not ("all" or "active" or "revoked" or "expired"))
        {
            return "El estado indicado no es válido.";
        }

        if (query.Search?.Length > 150)
        {
            return "El texto de búsqueda no puede superar 150 caracteres.";
        }

        query.FromUtc = NormalizeUtc(query.FromUtc);
        query.ToUtc = NormalizeUtc(query.ToUtc);
        if (query.FromUtc is DateTime fromUtc && query.ToUtc is DateTime toUtc && fromUtc > toUtc)
        {
            return "La fecha inicial no puede ser posterior a la fecha final.";
        }

        return null;
    }

    private static DateTime? NormalizeUtc(DateTime? value) => value?.Kind switch
    {
        DateTimeKind.Local => value.Value.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc),
        _ => value,
    };

    private bool TryGetSessionIdentity(out int userId, out string sessionId, out string application)
    {
        var rawUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        sessionId = User.FindFirst("sid")?.Value ?? string.Empty;
        application = User.FindFirst("app")?.Value ?? string.Empty;
        return int.TryParse(rawUserId, out userId) && userId > 0 && sessionId.Length == 32
            && IsSupportedApplication(application);
    }

    [AllowAnonymous]
    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout([FromQuery] string application)
    {
        if (!IsSupportedApplication(application)) return BadRequest(new { status = InvalidApplicationStatus });

        // AutoValidateAntiforgeryToken valida globalmente las operaciones POST.
        // El marcador X-Auth-Refresh es necesario para Refresh, pero duplicaría
        // la validación CSRF y podía rechazar el logout aunque el token fuera válido.
        var cookieName = GetCookieName(application);
        if (Request.Cookies.TryGetValue(cookieName, out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
        {
            _auth.RevokeRefreshToken(refreshToken);
        }

        Response.Cookies.Delete(cookieName, CookieOptions());
        return NoContent();
    }

    private async Task<ActionResult<AuthResponseDto>> Login(
        AuthLoginRequest request,
        string application,
        string? userAgent,
        string? clientPlatform)
    {
        var response = await _auth.LoginAsync(
            request,
            application,
            GetSessionClientInfo(userAgent, clientPlatform));
        if (response.Status == "authenticated" && !string.IsNullOrWhiteSpace(response.AccessToken))
        {
            RevokePreviousBrowserSession(application);
            SetRefreshCookie(application, response.RefreshToken);
            return Ok(response);
        }

        return response.Status switch
        {
            "mfaRequired" or EstadoCambioContrasenaRequerido or AccountBlockedStatus => Ok(response),
            "accessDenied" => StatusCode(StatusCodes.Status403Forbidden, response),
            AuthenticationUnavailableStatus => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => Unauthorized(response),
        };
    }

    private AuthSessionClientInfo GetSessionClientInfo(string? userAgent, string? clientPlatform)
    {
        return AuthSessionClientInfoParser.Parse(
            userAgent,
            HttpContext.Connection.RemoteIpAddress,
            clientPlatform);
    }

    private void SetRefreshCookie(string application, string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        Response.Cookies.Append(GetCookieName(application), refreshToken, CookieOptions());
    }

    private void RevokePreviousBrowserSession(string application)
    {
        if (Request.Cookies.TryGetValue(GetCookieName(application), out var previousToken) &&
            !string.IsNullOrWhiteSpace(previousToken))
        {
            _auth.RevokeRefreshToken(previousToken);
        }
    }

    private CookieOptions CookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = GetAuthCookiePath(),
            MaxAge = TimeSpan.FromDays(_refreshTokenDays),
        };
    }

    private CookieOptions DeviceCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = GetAuthCookiePath(),
            MaxAge = TimeSpan.FromDays(365),
        };
    }

    private string GetAuthCookiePath()
    {
        return Request.PathBase.Add(new PathString("/api/Auth")).Value ?? "/api/Auth";
    }

    private static string GetCookieName(string application)
    {
        return application.Trim().ToLowerInvariant() switch
        {
            AuthApplications.Galileo => GalileoRefreshCookie,
            AuthApplications.SSecurity => SSecurityRefreshCookie,
            _ => throw new ArgumentException("Aplicación no válida.", nameof(application)),
        };
    }

    private static bool IsSupportedApplication(string application)
    {
        return string.Equals(application, AuthApplications.Galileo, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(application, AuthApplications.SSecurity, StringComparison.OrdinalIgnoreCase);
    }
}
