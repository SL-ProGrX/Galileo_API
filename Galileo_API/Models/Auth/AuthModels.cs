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

public sealed record AuthSessionClientInfo(
    string BrowserName,
    string? BrowserVersion,
    string OperatingSystem,
    string? OperatingSystemVersion,
    string DeviceType,
    string? ClientIpAddress);

public sealed class AuthResponseDto
{
    public string Status { get; init; } = string.Empty;
    public string? Detail { get; init; }
    public string? AppStatus { get; init; }
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

public class GalileoPasswordRecoveryTokenRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public sealed class GalileoPasswordRecoveryRequest : GalileoPasswordRecoveryTokenRequest
{
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

public static class AuthSessionEndReasons
{
    public const string Logout = "logout";
    public const string UserRevoked = "user-revoked";
    public const string AdminRevoked = "admin-revoked";
    public const string IdleTimeout = "idle-timeout";
    public const string AbsoluteTimeout = "absolute-timeout";
    public const string RefreshExpired = "refresh-expired";
    public const string SessionNotFound = "session-not-found";
}

public sealed class AuthSessionStatusDto
{
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
}

public class UserSessionDto
{
    public string SessionId { get; init; } = string.Empty;
    public string AuthenticationMethod { get; init; } = string.Empty;
    public string BrowserName { get; init; } = string.Empty;
    public string? BrowserVersion { get; init; }
    public string OperatingSystem { get; init; } = string.Empty;
    public string? OperatingSystemVersion { get; init; }
    public string DeviceType { get; init; } = string.Empty;
    public string? ClientIpAddress { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime LastActivityAtUtc { get; init; }
    public DateTime RefreshExpiresAtUtc { get; init; }
    public DateTime? IdleExpiresAtUtc { get; init; }
    public DateTime? AbsoluteExpiresAtUtc { get; init; }
    public bool IsCurrent { get; init; }
}

public sealed class AdminSessionQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string Application { get; set; } = "all";
    public string Status { get; set; } = "all";
    public string? Search { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}

public sealed class AdminSessionPageDto
{
    public IReadOnlyList<AdminAuthSessionDto> Items { get; init; } = Array.Empty<AdminAuthSessionDto>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int ActiveCount { get; init; }
    public int StartedLast24Hours { get; init; }
    public int RevokedLast24Hours { get; init; }
}

public sealed class AdminAuthSessionDto : UserSessionDto
{
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Application { get; init; } = string.Empty;
    public DateTime? RevokedAtUtc { get; init; }
    public string? RevocationReason { get; init; }
    public string? AdminRevocationReason { get; init; }
    public string? RevokedByUserName { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class AdminSessionRevokeRequestDto
{
    public string Application { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public sealed record AdminSessionRevokeResult(string Status);

public sealed class GalileoSecurityInitializeRequest
{
    public required int EmpresaId { get; set; }
    public string AppVersion { get; set; } = string.Empty;
}

public sealed class GalileoSecurityPreInitializeRequest
{
    public string AppVersion { get; set; } = string.Empty;
}

public sealed class AppStatusDto
{
    public int Pasa { get; set; }
    public string Notas { get; set; } = string.Empty;
    public string Version_Status { get; set; } = string.Empty;
}

public sealed class WebAccessResultDto
{
    public int Indicador { get; set; }
    public string Notas { get; set; } = string.Empty;
}
