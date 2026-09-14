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
            "invalidCode" => BadRequest(response),
            _ => Unauthorized(response),
        };
    }

    [AllowAnonymous]
    [HttpPost("SSecurity/Mfa/Resend")]
    public async Task<IActionResult> ResendMfa([FromBody] MfaResendRequest request)
    {
        return await _auth.ResendMfaAsync(request) ? NoContent() : BadRequest();
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
            "mfaRequired" => Ok(response),
            "authenticationUnavailable" => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
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
