using System.Text.Json.Serialization;
using Galileo.Models;

namespace Galileo.Models.Auth;

public static class AuthApplications
{
    public const string Galileo = "galileo";
    public const string SSecurity = "ssecurity";
}

public sealed class AuthLoginRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
}

public sealed class AuthUserDto
{
    public int UserId { get; init; }
    public string Usuario { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
}

public sealed class AuthResponseDto
{
    public string Status { get; init; } = string.Empty;
    public string? Detail { get; init; }
    public string? AccessToken { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public AuthUserDto? User { get; init; }
    public string? ChallengeToken { get; init; }
    public DateTime? ChallengeExpiresAtUtc { get; init; }
    public IReadOnlyCollection<string> Methods { get; init; } = Array.Empty<string>();
    public string? PasswordExpiryNotice { get; init; }
    public ParametrosObtenerDto? PasswordPolicy { get; init; }
    [JsonIgnore]
    public string? RefreshToken { get; init; }
}

public sealed class PasswordChangeRequest
{
    public string ChallengeToken { get; set; } = string.Empty;
    public string PassViejo { get; set; } = string.Empty;
    public string NuevaContrasena { get; set; } = string.Empty;
    public string Confirmacion { get; set; } = string.Empty;
}

public sealed class MfaVerifyRequest
{
    public string ChallengeToken { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
}

public sealed class MfaResendRequest
{
    public string ChallengeToken { get; set; } = string.Empty;
}

public sealed class AuthSessionResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public AuthUserDto User { get; init; } = new();
}
