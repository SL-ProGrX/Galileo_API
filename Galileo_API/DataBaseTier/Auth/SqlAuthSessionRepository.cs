using System.Data;
using Galileo.BusinessLogic.Auth;
using Galileo.Models.Auth;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.Auth;

/// <summary>Persistence for authenticated sessions; callers generate opaque tokens and pass only SHA-256 hashes.</summary>
public sealed class SqlAuthSessionRepository
{
    private readonly string _connectionString;
    private readonly int? _idleMinutes;
    private readonly int? _absoluteHours;

    public SqlAuthSessionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnString")
            ?? throw new InvalidOperationException("DefaultConnString no está configurada.");
        _idleMinutes = ReadPositiveLimit(configuration, "AuthSessions:IdleMinutes");
        _absoluteHours = ReadPositiveLimit(configuration, "AuthSessions:AbsoluteHours");
    }

    public void Create(AuthSessionStore.SessionState session, string refreshHash)
    {
        const string sql = """
            INSERT INTO dbo.PGX_AuthSessions
                (SessionId, Application, UserId, UserName, DisplayName, AuthenticationMethod,
                 RefreshTokenHash, CreatedAtUtc, RefreshExpiresAtUtc, LastActivityAtUtc,
                 IdleExpiresAtUtc, AbsoluteExpiresAtUtc, BrowserName, BrowserVersion,
                 OperatingSystem, OperatingSystemVersion, DeviceType, ClientIpAddress)
            VALUES
                (@id, @app, @userId, @userName, @displayName, @method,
                 @hash, @now, @expires, @now, @idleExpires, @absoluteExpires,
                 @browserName, @browserVersion, @operatingSystem, @operatingSystemVersion,
                 @deviceType, @clientIpAddress);
            """;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        var now = DateTime.UtcNow;
        command.Parameters.Add("@id", SqlDbType.Char, 32).Value = session.SessionId;
        command.Parameters.Add("@app", SqlDbType.VarChar, 20).Value = session.Application;
        command.Parameters.Add("@userId", SqlDbType.Int).Value = session.User.UserId;
        command.Parameters.Add("@userName", SqlDbType.NVarChar, 150).Value = session.User.Usuario;
        command.Parameters.Add("@displayName", SqlDbType.NVarChar, 250).Value = session.User.Nombre;
        command.Parameters.Add("@method", SqlDbType.VarChar, 30).Value = session.AuthenticationMethod;
        command.Parameters.Add("@hash", SqlDbType.Char, 64).Value = refreshHash;
        command.Parameters.Add("@now", SqlDbType.DateTime2).Value = now;
        command.Parameters.Add("@expires", SqlDbType.DateTime2).Value = session.ExpiresAtUtc;
        command.Parameters.Add("@idleExpires", SqlDbType.DateTime2).Value =
            _idleMinutes is int idle ? now.AddMinutes(idle) : DBNull.Value;
        command.Parameters.Add("@absoluteExpires", SqlDbType.DateTime2).Value =
            _absoluteHours is int absolute ? now.AddHours(absolute) : DBNull.Value;
        command.Parameters.Add("@browserName", SqlDbType.NVarChar, 60).Value = session.BrowserName;
        command.Parameters.Add("@browserVersion", SqlDbType.NVarChar, 20).Value =
            (object?)session.BrowserVersion ?? DBNull.Value;
        command.Parameters.Add("@operatingSystem", SqlDbType.NVarChar, 40).Value = session.OperatingSystem;
        command.Parameters.Add("@operatingSystemVersion", SqlDbType.NVarChar, 20).Value =
            (object?)session.OperatingSystemVersion ?? DBNull.Value;
        command.Parameters.Add("@deviceType", SqlDbType.NVarChar, 20).Value = session.DeviceType;
        command.Parameters.Add("@clientIpAddress", SqlDbType.VarChar, 45).Value =
            (object?)session.ClientIpAddress ?? DBNull.Value;
        connection.Open();
        command.ExecuteNonQuery();
    }

    public bool TryRotate(string oldHash, string newHash, string application, out AuthSessionStore.SessionState session)
    {
        // A single conditional UPDATE is the compare-and-swap: two API instances cannot both
        // replace the same refresh hash. An obsolete hash never matches the current row.
        const string sql = """
            UPDATE dbo.PGX_AuthSessions
               SET RefreshTokenHash = @newHash
            OUTPUT inserted.SessionId, inserted.UserId, inserted.UserName, inserted.DisplayName,
                   inserted.Application, inserted.RefreshExpiresAtUtc, inserted.AuthenticationMethod
             WHERE RefreshTokenHash = @oldHash AND Application = @app
               AND RevokedAtUtc IS NULL AND RefreshExpiresAtUtc > SYSUTCDATETIME()
               AND (IdleExpiresAtUtc IS NULL OR IdleExpiresAtUtc > SYSUTCDATETIME())
               AND (AbsoluteExpiresAtUtc IS NULL OR AbsoluteExpiresAtUtc > SYSUTCDATETIME());
            """;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@newHash", SqlDbType.Char, 64).Value = newHash;
        command.Parameters.Add("@oldHash", SqlDbType.Char, 64).Value = oldHash;
        command.Parameters.Add("@app", SqlDbType.VarChar, 20).Value = application;
        connection.Open();
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            session = default!;
            return false;
        }

        session = new AuthSessionStore.SessionState(
            reader.GetString(0),
            new AuthUserDto { UserId = reader.GetInt32(1), Usuario = reader.GetString(2), Nombre = reader.GetString(3) },
            reader.GetString(4), reader.GetDateTime(5), string.Empty, reader.GetString(6));
        return true;
    }

    public void Revoke(string refreshHash, string reason)
    {
        const string sql = """
            UPDATE dbo.PGX_AuthSessions
               SET RevokedAtUtc = SYSUTCDATETIME(), RevocationReason = @reason
             WHERE RefreshTokenHash = @hash AND RevokedAtUtc IS NULL;
            """;
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@hash", SqlDbType.Char, 64).Value = refreshHash;
        command.Parameters.Add("@reason", SqlDbType.VarChar, 50).Value = reason;
        connection.Open();
        command.ExecuteNonQuery();
    }

    public AuthSessionStatusDto GetStatusByRefreshHash(string refreshHash, string application)
    {
        const string sql = """
            SELECT CASE
                WHEN RevokedAtUtc IS NOT NULL THEN COALESCE(NULLIF(RevocationReason, ''), 'revoked')
                WHEN AbsoluteExpiresAtUtc IS NOT NULL
                     AND AbsoluteExpiresAtUtc <= SYSUTCDATETIME()
                     AND (IdleExpiresAtUtc IS NULL OR AbsoluteExpiresAtUtc <= IdleExpiresAtUtc)
                    THEN 'absolute-timeout'
                WHEN IdleExpiresAtUtc IS NOT NULL AND IdleExpiresAtUtc <= SYSUTCDATETIME()
                    THEN 'idle-timeout'
                WHEN AbsoluteExpiresAtUtc IS NOT NULL AND AbsoluteExpiresAtUtc <= SYSUTCDATETIME()
                    THEN 'absolute-timeout'
                WHEN RefreshExpiresAtUtc <= SYSUTCDATETIME() THEN 'refresh-expired'
                ELSE ''
            END
              FROM dbo.PGX_AuthSessions
             WHERE RefreshTokenHash = @hash AND Application = @application;
            """;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@hash", SqlDbType.Char, 64).Value = refreshHash;
        command.Parameters.Add("@application", SqlDbType.VarChar, 20).Value = application;
        connection.Open();
        var result = command.ExecuteScalar();
        var reason = result is null ? AuthSessionEndReasons.SessionNotFound : result as string;
        if (string.IsNullOrEmpty(reason))
        {
            reason = null;
        }

        return new AuthSessionStatusDto
        {
            Status = reason is null ? "active" : "ended",
            Reason = reason,
        };
    }

    public bool RecordActivity(string sessionId, string application)
    {
        const string sql = """
            DECLARE @updated TABLE (SessionId char(32));

            UPDATE dbo.PGX_AuthSessions
               SET LastActivityAtUtc = SYSUTCDATETIME(),
                   IdleExpiresAtUtc = CASE WHEN @idleMinutes IS NULL THEN IdleExpiresAtUtc
                       ELSE DATEADD(minute, @idleMinutes, SYSUTCDATETIME()) END
            OUTPUT inserted.SessionId INTO @updated
             WHERE SessionId = @id AND Application = @app AND RevokedAtUtc IS NULL
               AND RefreshExpiresAtUtc > SYSUTCDATETIME()
               AND (IdleExpiresAtUtc IS NULL OR IdleExpiresAtUtc > SYSUTCDATETIME())
               AND (AbsoluteExpiresAtUtc IS NULL OR AbsoluteExpiresAtUtc > SYSUTCDATETIME());

            SELECT COUNT(*) FROM @updated;
            """;
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@id", SqlDbType.Char, 32).Value = sessionId;
        command.Parameters.Add("@app", SqlDbType.VarChar, 20).Value = application;
        command.Parameters.Add("@idleMinutes", SqlDbType.Int).Value = _idleMinutes is int idle ? idle : DBNull.Value;
        connection.Open();
        return (int)command.ExecuteScalar()! == 1;
    }

    public bool IsActive(string sessionId)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.PGX_AuthSessions
                 WHERE SessionId = @id AND RevokedAtUtc IS NULL
                   AND RefreshExpiresAtUtc > SYSUTCDATETIME()
                   AND (IdleExpiresAtUtc IS NULL OR IdleExpiresAtUtc > SYSUTCDATETIME())
                   AND (AbsoluteExpiresAtUtc IS NULL OR AbsoluteExpiresAtUtc > SYSUTCDATETIME())
            ) THEN 1 ELSE 0 END;
            """;
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@id", SqlDbType.Char, 32).Value = sessionId;
        connection.Open();
        return (int)command.ExecuteScalar()! == 1;
    }

    public IReadOnlyList<UserSessionDto> ListActiveForUser(int userId, string currentSessionId, string application)
    {
        const string sql = """
            SELECT SessionId, AuthenticationMethod, BrowserName, BrowserVersion,
                   OperatingSystem, OperatingSystemVersion, DeviceType, ClientIpAddress, CreatedAtUtc,
                   LastActivityAtUtc, RefreshExpiresAtUtc, IdleExpiresAtUtc, AbsoluteExpiresAtUtc
              FROM dbo.PGX_AuthSessions
             WHERE UserId = @userId AND Application = @application AND RevokedAtUtc IS NULL
               AND RefreshExpiresAtUtc > SYSUTCDATETIME()
               AND (IdleExpiresAtUtc IS NULL OR IdleExpiresAtUtc > SYSUTCDATETIME())
               AND (AbsoluteExpiresAtUtc IS NULL OR AbsoluteExpiresAtUtc > SYSUTCDATETIME())
             ORDER BY LastActivityAtUtc DESC;
            """;
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@application", SqlDbType.VarChar, 20).Value = application;
        connection.Open();
        using var reader = command.ExecuteReader();
        var sessions = new List<UserSessionDto>();
        while (reader.Read())
        {
            sessions.Add(MapUserSession(reader, currentSessionId));
        }
        return sessions;
    }

    private static UserSessionDto MapUserSession(SqlDataReader reader, string currentSessionId)
    {
        var sessionId = reader.GetString(0).Trim();
        return new UserSessionDto
        {
            SessionId = sessionId,
            AuthenticationMethod = reader.GetString(1),
            BrowserName = reader.IsDBNull(2) ? "No identificado" : reader.GetString(2),
            BrowserVersion = reader.IsDBNull(3) ? null : reader.GetString(3),
            OperatingSystem = reader.IsDBNull(4) ? "No identificado" : reader.GetString(4),
            OperatingSystemVersion = reader.IsDBNull(5) ? null : reader.GetString(5),
            DeviceType = reader.IsDBNull(6) ? "No identificado" : reader.GetString(6),
            ClientIpAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
            CreatedAtUtc = DateTime.SpecifyKind(reader.GetDateTime(8), DateTimeKind.Utc),
            LastActivityAtUtc = DateTime.SpecifyKind(reader.GetDateTime(9), DateTimeKind.Utc),
            RefreshExpiresAtUtc = DateTime.SpecifyKind(reader.GetDateTime(10), DateTimeKind.Utc),
            IdleExpiresAtUtc = reader.IsDBNull(11) ? null : DateTime.SpecifyKind(reader.GetDateTime(11), DateTimeKind.Utc),
            AbsoluteExpiresAtUtc = reader.IsDBNull(12) ? null : DateTime.SpecifyKind(reader.GetDateTime(12), DateTimeKind.Utc),
            IsCurrent = string.Equals(sessionId, currentSessionId, StringComparison.Ordinal),
        };
    }

    public bool RevokeForUser(int userId, string sessionId, string application)
    {
        const string sql = """
            DECLARE @revoked TABLE (SessionId char(32));

            UPDATE dbo.PGX_AuthSessions
               SET RevokedAtUtc = SYSUTCDATETIME(), RevocationReason = @reason
            OUTPUT inserted.SessionId INTO @revoked
             WHERE UserId = @userId AND Application = @application
               AND SessionId = @sessionId AND RevokedAtUtc IS NULL;

            SELECT COUNT(*) FROM @revoked;
            """;
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@application", SqlDbType.VarChar, 20).Value = application;
        command.Parameters.Add("@sessionId", SqlDbType.Char, 32).Value = sessionId;
        command.Parameters.Add("@reason", SqlDbType.VarChar, 50).Value = AuthSessionEndReasons.UserRevoked;
        connection.Open();
        return (int)command.ExecuteScalar()! == 1;
    }

    private static int? ReadPositiveLimit(IConfiguration configuration, string key)
    {
        var value = configuration.GetValue<int?>(key);
        if (value is <= 0)
        {
            throw new InvalidOperationException($"{key} debe ser mayor que cero.");
        }

        return value;
    }
}
