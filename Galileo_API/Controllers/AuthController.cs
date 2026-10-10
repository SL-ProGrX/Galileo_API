using Galileo.BusinessLogic.Auth;
using Galileo.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    private const string PasswordChangeRequiredStatus = "passwordChangeRequired";
    private const string AccountBlockedStatus = "accountBlocked";
    private readonly AuthBL _auth;
    private readonly int _refreshTokenDays;

    public AuthController(AuthBL auth, IConfiguration configuration)
    {
        _auth = auth;
        _refreshTokenDays = Math.Clamp(configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 7, 1, 30);
    }

    [AllowAnonymous]
    [HttpPost("Galileo/Login")]
    public async Task<ActionResult<AuthResponseDto>> GalileoLogin([FromBody] AuthLoginRequest request)
    {
        return await Login(request, AuthApplications.Galileo);
    }

    [AllowAnonymous]
    [HttpPost("SSecurity/Login")]
    public async Task<ActionResult<AuthResponseDto>> SSecurityLogin([FromBody] AuthLoginRequest request)
    {
        return await Login(request, AuthApplications.SSecurity);
    }

    [Authorize]
    [HttpPost("Galileo/PreInitialize")]
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
            "ready" or PasswordChangeRequiredStatus or AccountBlockedStatus or "applicationBlocked" => Ok(response),
            AuthenticationUnavailableStatus => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => BadRequest(response),
        };
    }

    [Authorize]
    [HttpPost("Galileo/Initialize")]
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

        if (response.Status is "ready" or PasswordChangeRequiredStatus)
        {
            Response.Cookies.Append(
                GalileoDeviceCookie,
                deviceId.ToString("D"),
                DeviceCookieOptions());
        }

        return response.Status switch
        {
            "ready" or PasswordChangeRequiredStatus or AccountBlockedStatus or "applicationBlocked" or "accessDenied" => Ok(response),
            AuthenticationUnavailableStatus => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => BadRequest(response),
        };
    }

    [AllowAnonymous]
    [HttpPost("SSecurity/Mfa/Verify")]
    public ActionResult<AuthResponseDto> VerifyMfa([FromBody] MfaVerifyRequest request)
    {
        var response = _auth.VerifyMfa(request);
        if (response.Status == "authenticated")
        {
            SetRefreshCookie(AuthApplications.SSecurity, response.RefreshToken);
            return Ok(response);
        }

        return response.Status switch
        {
            "invalidCode" or "invalidChallenge" or AccountBlockedStatus or PasswordChangeRequiredStatus => Ok(response),
            AuthenticationUnavailableStatus => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => Unauthorized(response),
        };
    }

    [AllowAnonymous]
    [HttpPost("SSecurity/Password/Change")]
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
    public ActionResult<AuthResponseDto> GalileoRecoveryPolicy([FromBody] GalileoPasswordRecoveryTokenRequest request)
    {
        var response = _auth.ObtenerPoliticaRecuperacionGalileo(request);
        return response.Status == AuthenticationUnavailableStatus
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, response)
            : Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("Galileo/Password/Recovery/Change")]
    public ActionResult<AuthResponseDto> GalileoRecoveryChange([FromBody] GalileoPasswordRecoveryRequest request)
    {
        var response = _auth.RecuperarContrasenaGalileo(request);
        return response.Status == AuthenticationUnavailableStatus
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, response)
            : Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("SSecurity/Mfa/Resend")]
    public async Task<IActionResult> ResendMfa([FromBody] MfaResendRequest request)
    {
        var response = await _auth.ResendMfaAsync(request);
        return response.Status switch
        {
            "mfaResent" => NoContent(),
            "invalidChallenge" => BadRequest(response),
            _ => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
        };
    }

    [AllowAnonymous]
    [HttpPost("Refresh")]
    public ActionResult<AuthSessionResponse> Refresh(
        [FromQuery] string application,
        [FromHeader(Name = "X-Auth-Refresh")] string? csrfHeader)
    {
        if (!IsSupportedApplication(application)) return BadRequest(new { status = "invalidApplication" });

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
            Response.Cookies.Delete(cookieName, CookieOptions());
            return Unauthorized(new { status = "invalidRefreshToken" });
        }

        SetRefreshCookie(application, replacementRefreshToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("Logout")]
    public IActionResult Logout([FromQuery] string application)
    {
        if (!IsSupportedApplication(application)) return BadRequest(new { status = "invalidApplication" });

        var cookieName = GetCookieName(application);
        if (Request.Cookies.TryGetValue(cookieName, out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
        {
            _auth.RevokeRefreshToken(refreshToken);
        }

        Response.Cookies.Delete(cookieName, CookieOptions());
        return NoContent();
    }

    private async Task<ActionResult<AuthResponseDto>> Login(AuthLoginRequest request, string application)
    {
        var response = await _auth.LoginAsync(request, application);
        if (response.Status == "authenticated" && !string.IsNullOrWhiteSpace(response.AccessToken))
        {
            SetRefreshCookie(application, response.RefreshToken);
            return Ok(response);
        }

        return response.Status switch
        {
            "mfaRequired" or PasswordChangeRequiredStatus or AccountBlockedStatus => Ok(response),
            AuthenticationUnavailableStatus => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => Unauthorized(response),
        };
    }

    private void SetRefreshCookie(string application, string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        Response.Cookies.Append(GetCookieName(application), refreshToken, CookieOptions());
    }

    private CookieOptions CookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = "/api/Auth",
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
            Path = "/api/Auth",
            MaxAge = TimeSpan.FromDays(365),
        };
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
