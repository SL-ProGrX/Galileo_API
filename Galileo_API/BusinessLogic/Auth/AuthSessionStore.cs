using System.Security.Cryptography;
using System.Text;
using Galileo.Models.Auth;

namespace Galileo.BusinessLogic.Auth;

/// <summary>
/// Stores only short-lived MFA challenges in process memory. Authentication
/// sessions and refresh tokens are stateless signed JWTs.
/// </summary>
public sealed class AuthSessionStore
{
    private readonly object _sync = new();
    private readonly Dictionary<string, ChallengeState> _challengesByHash = new(StringComparer.Ordinal);

    public ChallengeState CreateChallenge(AuthUserDto user, string application, IReadOnlyCollection<string> methods)
    {
        var challenge = new ChallengeState(
            CreateOpaqueToken(),
            user,
            application,
            methods,
            DateTime.UtcNow.AddMinutes(5));

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

    public sealed record ChallengeState(
        string Token,
        AuthUserDto User,
        string Application,
        IReadOnlyCollection<string> Methods,
        DateTime ExpiresAtUtc)
    {
        public bool Consumed { get; set; }
        public int Attempts { get; set; }
    }
}
