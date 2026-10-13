using System.Security.Cryptography;
using System.Text;
using Galileo.Models.Auth;
using Galileo.DataBaseTier.Auth;

namespace Galileo.BusinessLogic.Auth;

/// <summary>
/// Stores pre-authentication challenges in process memory. Authenticated sessions use
/// process memory by default or SQL when AuthSessions:Store is explicitly set to Sql.
/// </summary>
public sealed class AuthSessionStore
{
    private readonly SqlAuthSessionRepository? _sql;
    private readonly object _sync = new();
    private readonly Dictionary<string, SessionState> _sessionsByRefreshHash = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ChallengeState> _challengesByHash = new(StringComparer.Ordinal);
    private readonly HashSet<string> _revokedSessionIds = new(StringComparer.Ordinal);

    public AuthSessionStore() { }

    public AuthSessionStore(IConfiguration configuration)
    {
        var provider = configuration["AuthSessions:Store"];
        if (string.IsNullOrWhiteSpace(provider) ||
            string.Equals(provider, "Memory", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.Equals(provider, "Sql", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("AuthSessions:Store debe ser Memory o Sql.");
        }

        _sql = new SqlAuthSessionRepository(configuration);
    }

    public SessionState CreateSession(AuthUserDto user, string application, TimeSpan lifetime, string authenticationMethod)
    {
        var now = DateTime.UtcNow;
        var session = new SessionState(
            Guid.NewGuid().ToString("N"),
            user,
            application,
            now.Add(lifetime),
            CreateOpaqueToken(),
            authenticationMethod);

        if (_sql is not null)
        {
            _sql.Create(session, Hash(session.RefreshToken));
            return session;
        }

        lock (_sync)
        {
            _sessionsByRefreshHash[Hash(session.RefreshToken)] = session;
        }

        return session;
    }

    public bool TryRotateRefreshToken(
        string rawRefreshToken,
        string application,
        out SessionState session,
        out string replacementRefreshToken)
    {
        session = default!;
        replacementRefreshToken = string.Empty;
        var hash = Hash(rawRefreshToken);

        if (_sql is not null)
        {
            replacementRefreshToken = CreateOpaqueToken();
            if (!_sql.TryRotate(hash, Hash(replacementRefreshToken), application, out var persisted))
            {
                replacementRefreshToken = string.Empty;
                return false;
            }

            session = persisted with { RefreshToken = replacementRefreshToken };
            return true;
        }

        lock (_sync)
        {
            if (!_sessionsByRefreshHash.TryGetValue(hash, out var current))
            {
                return false;
            }

            if (current.Used || current.Revoked || current.ExpiresAtUtc <= DateTime.UtcNow ||
                !string.Equals(current.Application, application, StringComparison.OrdinalIgnoreCase))
            {
                current.Revoked = true;
                _revokedSessionIds.Add(current.SessionId);
                return false;
            }

            current.Used = true;
            replacementRefreshToken = CreateOpaqueToken();
            var replacement = current with { RefreshToken = replacementRefreshToken, Used = false };
            _sessionsByRefreshHash[Hash(replacementRefreshToken)] = replacement;
            session = replacement;
            return true;
        }
    }

    public void Revoke(string rawRefreshToken)
    {
        if (_sql is not null)
        {
            _sql.Revoke(Hash(rawRefreshToken));
            return;
        }

        lock (_sync)
        {
            if (_sessionsByRefreshHash.TryGetValue(Hash(rawRefreshToken), out var session))
            {
                session.Revoked = true;
                _revokedSessionIds.Add(session.SessionId);
            }
        }
    }

    public bool RecordActivity(string sessionId, string application)
    {
        if (_sql is not null)
        {
            return _sql.RecordActivity(sessionId, application);
        }

        return IsSessionActive(sessionId);
    }

    public bool IsSessionActive(string sessionId)
    {
        if (_sql is not null)
        {
            return _sql.IsActive(sessionId);
        }

        lock (_sync)
        {
            return !_revokedSessionIds.Contains(sessionId) &&
                _sessionsByRefreshHash.Values.Any(session =>
                    session.SessionId == sessionId &&
                    !session.Revoked &&
                    session.ExpiresAtUtc > DateTime.UtcNow);
        }
    }

    public ChallengeState CreateChallenge(
        AuthUserDto user,
        string application,
        IReadOnlyCollection<string> methods,
        string purpose = "mfa")
    {
        var challenge = new ChallengeState(
            CreateOpaqueToken(),
            user,
            application,
            methods,
            DateTime.UtcNow.AddMinutes(5),
            purpose);

        lock (_sync)
        {
            _challengesByHash[Hash(challenge.Token)] = challenge;
        }

        return challenge;
    }

    public bool TryGetChallenge(string token, string application, out ChallengeState challenge)
    {
        lock (_sync)
        {
            if (_challengesByHash.TryGetValue(Hash(token), out challenge!) &&
                !challenge.Consumed &&
                challenge.ExpiresAtUtc > DateTime.UtcNow &&
                string.Equals(challenge.Application, application, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            challenge = default!;
            return false;
        }
    }

    public bool TryConsumeChallenge(string token, string application, out ChallengeState challenge)
    {
        lock (_sync)
        {
            if (!TryGetChallenge(token, application, out challenge))
            {
                return false;
            }

            challenge.Consumed = true;
            return true;
        }
    }

    public bool RegisterChallengeFailure(string token, string application)
    {
        lock (_sync)
        {
            if (!TryGetChallenge(token, application, out var challenge))
            {
                return false;
            }

            challenge.Attempts++;
            if (challenge.Attempts >= 5)
            {
                challenge.Consumed = true;
            }

            return true;
        }
    }

    private static string CreateOpaqueToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string Hash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)));
    }

    public sealed record SessionState(
        string SessionId,
        AuthUserDto User,
        string Application,
        DateTime ExpiresAtUtc,
        string RefreshToken,
        string AuthenticationMethod)
    {
        public bool Used { get; set; }
        public bool Revoked { get; set; }
    }

    public sealed record ChallengeState(
        string Token,
        AuthUserDto User,
        string Application,
        IReadOnlyCollection<string> Methods,
        DateTime ExpiresAtUtc,
        string Purpose)
    {
        public bool Consumed { get; set; }
        public int Attempts { get; set; }
    }
}
