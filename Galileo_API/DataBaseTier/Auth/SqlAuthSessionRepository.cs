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
                 IdleExpiresAtUtc, AbsoluteExpiresAtUtc)
            VALUES
                (@id, @app, @userId, @userName, @displayName, @method,
                 @hash, @now, @expires, @now, @idleExpires, @absoluteExpires);
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

    public void Revoke(string refreshHash)
    {
        const string sql = """
            UPDATE dbo.PGX_AuthSessions
               SET RevokedAtUtc = SYSUTCDATETIME(), RevocationReason = 'logout'
             WHERE RefreshTokenHash = @hash AND RevokedAtUtc IS NULL;
            """;
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@hash", SqlDbType.Char, 64).Value = refreshHash;
        connection.Open();
        command.ExecuteNonQuery();
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
