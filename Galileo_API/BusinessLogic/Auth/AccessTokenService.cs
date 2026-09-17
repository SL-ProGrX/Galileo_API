using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Galileo.Models;
using Galileo.Models.Auth;
using Microsoft.IdentityModel.Tokens;

namespace Galileo.BusinessLogic.Auth;

public sealed class AccessTokenService
{
    private readonly JwtDto _settings;
    private readonly SymmetricSecurityKey _signingKey;

    public AccessTokenService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Jwt");
        _settings = section.Get<JwtDto>() ?? new JwtDto();

        var secret = Environment.GetEnvironmentVariable("Jwt__Secret");

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Jwt:Secret no está configurada.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Issuer) ||
            string.IsNullOrWhiteSpace(_settings.Audience))
        {
            throw new InvalidOperationException("Configuración JWT incompleta.");
        }

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public AuthSessionResponse CreateAccessToken(AuthUserDto user, string application, string sessionId, string authenticationMethod = "pwd")
    {
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(Math.Max(_settings.AccessTokenMinutes, 1));
        var token = CreateToken(user, application, sessionId, authenticationMethod, issuedAt, expiresAt, "access");

        return new AuthSessionResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAt,
            User = user,
        };
    }

    public string CreateRefreshToken(
        AuthUserDto user,
        string application,
        string sessionId,
        string authenticationMethod = "pwd")
    {
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddDays(Math.Clamp(_settings.RefreshTokenDays, 1, 30));
        return new JwtSecurityTokenHandler().WriteToken(
            CreateToken(user, application, sessionId, authenticationMethod, issuedAt, expiresAt, "refresh"));
    }

    public bool TryValidateRefreshToken(
        string rawToken,
        string application,
        out AuthUserDto user,
        out string sessionId,
        out string authenticationMethod)
    {
        user = default!;
        sessionId = string.Empty;
        authenticationMethod = string.Empty;

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(
                rawToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                },
                out _);

            if (!string.Equals(principal.FindFirstValue("token_use"), "refresh", StringComparison.Ordinal) ||
                !string.Equals(principal.FindFirstValue("app"), application, StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(principal.FindFirstValue("UserId"), out var userId) ||
                userId <= 0)
            {
                return false;
            }

            sessionId = principal.FindFirstValue("sid") ?? string.Empty;
            authenticationMethod = principal.FindFirstValue("amr") ?? "pwd";
            var username = principal.FindFirstValue("UserName") ?? principal.Identity?.Name ?? string.Empty;
            var name = principal.FindFirstValue("display_name") ?? username;

            if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            user = new AuthUserDto
            {
                UserId = userId,
                Usuario = username,
                Nombre = name,
            };
            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private JwtSecurityToken CreateToken(
        AuthUserDto user,
        string application,
        string sessionId,
        string authenticationMethod,
        DateTime issuedAt,
        DateTime expiresAt,
        string tokenUse)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Usuario),
            new("UserId", user.UserId.ToString()),
            new("UserName", user.Usuario),
            new("display_name", user.Nombre ?? string.Empty),
            new("app", application),
            new("sid", sessionId),
            new("amr", authenticationMethod),
            new("token_use", tokenUse),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

        return new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));
    }
}
