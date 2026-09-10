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

        var secret = Environment.GetEnvironmentVariable("Jwt__Secret")
            ?? configuration["Jwt:Secret"];

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
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Usuario),
            new("UserId", user.UserId.ToString()),
            new("UserName", user.Usuario),
            new("app", application),
            new("sid", sessionId),
            new("amr", authenticationMethod),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

        return new AuthSessionResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAt,
            User = user,
        };
    }
}
