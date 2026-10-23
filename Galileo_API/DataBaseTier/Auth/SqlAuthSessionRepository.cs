using System.Data;
using Galileo.BusinessLogic.Auth;
using Galileo.Models.Auth;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.Auth;

/// <summary>Persistence for authenticated sessions; callers generate opaque tokens and pass only SHA-256 hashes.</summary>
public sealed class SqlAuthSessionRepository
{
    private const string UnidentifiedValue = "No identificado";
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
            BrowserName = reader.IsDBNull(2) ? UnidentifiedValue : reader.GetString(2),
            BrowserVersion = reader.IsDBNull(3) ? null : reader.GetString(3),
            OperatingSystem = reader.IsDBNull(4) ? UnidentifiedValue : reader.GetString(4),
            OperatingSystemVersion = reader.IsDBNull(5) ? null : reader.GetString(5),
            DeviceType = reader.IsDBNull(6) ? UnidentifiedValue : reader.GetString(6),
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

    public AdminSessionPageDto ListAllSessions(AdminSessionQueryDto query, string currentSessionId)
    {
        const string sql = """
            DECLARE @now datetime2(7) = @nowUtc;

            SELECT
                COALESCE(SUM(CASE WHEN RevokedAtUtc IS NULL
                    AND RefreshExpiresAtUtc > @now
                    AND (IdleExpiresAtUtc IS NULL OR IdleExpiresAtUtc > @now)
                    AND (AbsoluteExpiresAtUtc IS NULL OR AbsoluteExpiresAtUtc > @now)
                    THEN 1 ELSE 0 END), 0) AS ActiveCount,
                COALESCE(SUM(CASE WHEN CreatedAtUtc >= DATEADD(hour, -24, @now)
                    THEN 1 ELSE 0 END), 0) AS StartedLast24Hours,
                COALESCE(SUM(CASE WHEN RevokedAtUtc >= DATEADD(hour, -24, @now)
                    THEN 1 ELSE 0 END), 0) AS RevokedLast24Hours
              FROM dbo.PGX_AuthSessions;

            SELECT session.SessionId, session.UserId, session.UserName, session.DisplayName,
                   session.Application, session.AuthenticationMethod, session.BrowserName,
                   session.BrowserVersion, session.OperatingSystem, session.OperatingSystemVersion,
                   session.DeviceType, session.ClientIpAddress, session.CreatedAtUtc,
                   session.LastActivityAtUtc, session.RefreshExpiresAtUtc, session.IdleExpiresAtUtc,
                   session.AbsoluteExpiresAtUtc, session.RevokedAtUtc, session.RevocationReason,
                   audit.Reason AS AdminRevocationReason, audit.AdminUserName AS RevokedByUserName
              INTO #FilteredSessions
              FROM dbo.PGX_AuthSessions AS session
              OUTER APPLY
              (
                  SELECT TOP (1) Reason, AdminUserName
                    FROM dbo.PGX_AuthSessionAdminAudit
                   WHERE SessionId = session.SessionId AND Outcome = 'revoked'
                   ORDER BY OccurredAtUtc DESC, AuditId DESC
              ) AS audit
             WHERE (@application = 'all' OR session.Application = @application)
               AND (@fromUtc IS NULL OR session.CreatedAtUtc >= @fromUtc)
               AND (@toUtc IS NULL OR session.CreatedAtUtc <= @toUtc)
               AND (@search IS NULL OR UserName LIKE @search ESCAPE '~'
                    OR DisplayName LIKE @search ESCAPE '~'
                    OR ClientIpAddress LIKE @search ESCAPE '~'
                    OR BrowserName LIKE @search ESCAPE '~'
                    OR DeviceType LIKE @search ESCAPE '~')
               AND (@status = 'all'
                    OR (@status = 'active' AND session.RevokedAtUtc IS NULL
                        AND session.RefreshExpiresAtUtc > @now
                        AND (session.IdleExpiresAtUtc IS NULL OR session.IdleExpiresAtUtc > @now)
                        AND (session.AbsoluteExpiresAtUtc IS NULL OR session.AbsoluteExpiresAtUtc > @now))
                    OR (@status = 'revoked' AND session.RevokedAtUtc IS NOT NULL)
                    OR (@status = 'expired' AND session.RevokedAtUtc IS NULL
                        AND (session.RefreshExpiresAtUtc <= @now
                             OR (session.IdleExpiresAtUtc IS NOT NULL AND session.IdleExpiresAtUtc <= @now)
                             OR (session.AbsoluteExpiresAtUtc IS NOT NULL AND session.AbsoluteExpiresAtUtc <= @now))));

            SELECT COUNT(*) AS TotalCount FROM #FilteredSessions;

            SELECT SessionId, UserId, UserName, DisplayName, Application,
                   AuthenticationMethod, BrowserName, BrowserVersion, OperatingSystem,
                   OperatingSystemVersion, DeviceType, ClientIpAddress, CreatedAtUtc,
                   LastActivityAtUtc, RefreshExpiresAtUtc, IdleExpiresAtUtc,
                   AbsoluteExpiresAtUtc, RevokedAtUtc, RevocationReason,
                   AdminRevocationReason, RevokedByUserName,
                   CASE WHEN RevokedAtUtc IS NOT NULL THEN 'revoked'
                        WHEN RefreshExpiresAtUtc <= @now
                          OR (IdleExpiresAtUtc IS NOT NULL AND IdleExpiresAtUtc <= @now)
                          OR (AbsoluteExpiresAtUtc IS NOT NULL AND AbsoluteExpiresAtUtc <= @now)
                            THEN 'expired'
                        ELSE 'active' END AS Status
              FROM #FilteredSessions
             ORDER BY LastActivityAtUtc DESC, CreatedAtUtc DESC, SessionId DESC
             OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;

            DROP TABLE #FilteredSessions;
            """;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        var now = DateTime.UtcNow;
        object search = string.IsNullOrWhiteSpace(query.Search)
            ? DBNull.Value
            : $"%{EscapeLikeFilter(query.Search.Trim())}%";
        command.Parameters.Add("@nowUtc", SqlDbType.DateTime2).Value = now;
        command.Parameters.Add("@application", SqlDbType.VarChar, 20).Value = query.Application;
        command.Parameters.Add("@status", SqlDbType.VarChar, 20).Value = query.Status;
        command.Parameters.Add("@search", SqlDbType.NVarChar, 320).Value = search;
        command.Parameters.Add("@fromUtc", SqlDbType.DateTime2).Value = query.FromUtc is DateTime fromUtc
            ? fromUtc
            : DBNull.Value;
        command.Parameters.Add("@toUtc", SqlDbType.DateTime2).Value = query.ToUtc is DateTime toUtc
            ? toUtc
            : DBNull.Value;
        command.Parameters.Add("@offset", SqlDbType.Int).Value = (query.Page - 1) * query.PageSize;
        command.Parameters.Add("@pageSize", SqlDbType.Int).Value = query.PageSize;

        connection.Open();
        using var reader = command.ExecuteReader();
        reader.Read();
        var activeCount = reader.GetInt32(0);
        var startedLast24Hours = reader.GetInt32(1);
        var revokedLast24Hours = reader.GetInt32(2);

        reader.NextResult();
        reader.Read();
        var totalCount = reader.GetInt32(0);

        reader.NextResult();
        var sessions = new List<AdminAuthSessionDto>();
        while (reader.Read())
        {
            sessions.Add(MapAdminSession(reader, currentSessionId));
        }

        return new AdminSessionPageDto
        {
            Items = sessions,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            ActiveCount = activeCount,
            StartedLast24Hours = startedLast24Hours,
            RevokedLast24Hours = revokedLast24Hours,
        };
    }

    public AdminSessionRevokeResult RevokeAllSession(
        string sessionId,
        string application,
        int adminUserId,
        string adminUserName,
        string reason)
    {
        const string sql = """
            SET XACT_ABORT ON;
            DECLARE @now datetime2(7) = SYSUTCDATETIME();
            DECLARE @targetUserId int = NULL;
            DECLARE @targetUserName nvarchar(150) = NULL;
            DECLARE @targetDisplayName nvarchar(250) = NULL;
            DECLARE @targetRevokedAtUtc datetime2(7) = NULL;
            DECLARE @targetRefreshExpiresAtUtc datetime2(7) = NULL;
            DECLARE @targetIdleExpiresAtUtc datetime2(7) = NULL;
            DECLARE @targetAbsoluteExpiresAtUtc datetime2(7) = NULL;
            DECLARE @result varchar(20);

            SELECT @targetUserId = UserId,
                   @targetUserName = UserName,
                   @targetDisplayName = DisplayName,
                   @targetRevokedAtUtc = RevokedAtUtc,
                   @targetRefreshExpiresAtUtc = RefreshExpiresAtUtc,
                   @targetIdleExpiresAtUtc = IdleExpiresAtUtc,
                   @targetAbsoluteExpiresAtUtc = AbsoluteExpiresAtUtc
              FROM dbo.PGX_AuthSessions WITH (UPDLOCK, HOLDLOCK)
             WHERE SessionId = @sessionId AND Application = @application;

            IF @targetUserId IS NULL
                SET @result = 'not-found';
            ELSE IF @targetRevokedAtUtc IS NOT NULL
                 OR @targetRefreshExpiresAtUtc <= @now
                 OR (@targetIdleExpiresAtUtc IS NOT NULL AND @targetIdleExpiresAtUtc <= @now)
                 OR (@targetAbsoluteExpiresAtUtc IS NOT NULL AND @targetAbsoluteExpiresAtUtc <= @now)
                SET @result = 'already-ended';
            ELSE
            BEGIN
                UPDATE dbo.PGX_AuthSessions
                   SET RevokedAtUtc = @now, RevocationReason = @revocationReason
                 WHERE SessionId = @sessionId AND Application = @application
                   AND RevokedAtUtc IS NULL;
                SET @result = CASE WHEN @@ROWCOUNT = 1 THEN 'revoked' ELSE 'already-ended' END;
            END;

            INSERT INTO dbo.PGX_AuthSessionAdminAudit
                (AdminUserId, AdminUserName, SessionId, TargetUserId, TargetUserName,
                 TargetDisplayName, Application, Reason, Outcome, OccurredAtUtc)
            VALUES
                (@adminUserId, @adminUserName, @sessionId, @targetUserId, @targetUserName,
                 @targetDisplayName, @application, @reason, @result, @now);

            SELECT @result;
            """;

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@sessionId", SqlDbType.Char, 32).Value = sessionId;
        command.Parameters.Add("@application", SqlDbType.VarChar, 20).Value = application;
        command.Parameters.Add("@adminUserId", SqlDbType.Int).Value = adminUserId;
        command.Parameters.Add("@adminUserName", SqlDbType.NVarChar, 150).Value = adminUserName;
        command.Parameters.Add("@reason", SqlDbType.NVarChar, 300).Value = reason;
        command.Parameters.Add("@revocationReason", SqlDbType.VarChar, 50).Value = AuthSessionEndReasons.AdminRevoked;
        connection.Open();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        command.Transaction = transaction;
        var outcome = (string?)command.ExecuteScalar() ?? "not-found";
        transaction.Commit();
        return new AdminSessionRevokeResult(outcome);
    }

    private static AdminAuthSessionDto MapAdminSession(SqlDataReader reader, string currentSessionId)
    {
        var sessionId = reader.GetString(0).Trim();
        var userName = reader.IsDBNull(2) ? UnidentifiedValue : reader.GetString(2);
        return new AdminAuthSessionDto
        {
            SessionId = sessionId,
            UserId = reader.GetInt32(1),
            UserName = userName,
            DisplayName = reader.IsDBNull(3) ? userName : reader.GetString(3),
            Application = reader.GetString(4),
            AuthenticationMethod = reader.GetString(5),
            BrowserName = reader.IsDBNull(6) ? UnidentifiedValue : reader.GetString(6),
            BrowserVersion = reader.IsDBNull(7) ? null : reader.GetString(7),
            OperatingSystem = reader.IsDBNull(8) ? UnidentifiedValue : reader.GetString(8),
            OperatingSystemVersion = reader.IsDBNull(9) ? null : reader.GetString(9),
            DeviceType = reader.IsDBNull(10) ? UnidentifiedValue : reader.GetString(10),
            ClientIpAddress = reader.IsDBNull(11) ? null : reader.GetString(11),
            CreatedAtUtc = AsUtc(reader.GetDateTime(12)),
            LastActivityAtUtc = AsUtc(reader.GetDateTime(13)),
            RefreshExpiresAtUtc = AsUtc(reader.GetDateTime(14)),
            IdleExpiresAtUtc = reader.IsDBNull(15) ? null : AsUtc(reader.GetDateTime(15)),
            AbsoluteExpiresAtUtc = reader.IsDBNull(16) ? null : AsUtc(reader.GetDateTime(16)),
            RevokedAtUtc = reader.IsDBNull(17) ? null : AsUtc(reader.GetDateTime(17)),
            RevocationReason = reader.IsDBNull(18) ? null : reader.GetString(18),
            AdminRevocationReason = reader.IsDBNull(19) ? null : reader.GetString(19),
            RevokedByUserName = reader.IsDBNull(20) ? null : reader.GetString(20),
            Status = reader.GetString(21),
            IsCurrent = string.Equals(sessionId, currentSessionId, StringComparison.Ordinal),
        };
    }

    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string EscapeLikeFilter(string value) => value
        .Replace("~", "~~", StringComparison.Ordinal)
        .Replace("%", "~%", StringComparison.Ordinal)
        .Replace("_", "~_", StringComparison.Ordinal)
        .Replace("[", "~[", StringComparison.Ordinal);

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
