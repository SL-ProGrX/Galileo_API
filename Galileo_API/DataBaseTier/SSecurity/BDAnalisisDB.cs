using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;

using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class BDAnalisisDB
    {
        private readonly IConfiguration _config;

        public BDAnalisisDB(IConfiguration config)
        {
            _config = config;
        }

        // =======================
        // API pública
        // =======================

        public ErrorDto<List<string>> TablasCargar()
        {
            try
            {
                using var connection =
                    new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                const string sql = @"
                    SELECT name
                    FROM sys.objects
                    WHERE type = 'U'
                    ORDER BY name";

                return DbHelper.CreateOkResponse(connection.Query<string>(sql).ToList());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<string>>(ex.Message);
            }
        }

        public ErrorDto<List<Dictionary<string, object?>>> ResultadosObtener(string objeto)
        {
            try
            {
                using var connection = CrearConexion();
                var objetoSeguro = ObtenerObjeto(connection, objeto);
                if (objetoSeguro is null)
                {
                    return DbHelper.CreateErrorResponse<List<Dictionary<string, object?>>>(
                        "El objeto solicitado no existe o no es una tabla válida.");
                }

                const string sql = @"
                    DECLARE @Statement nvarchar(max) =
                        N'SELECT TOP 50 * FROM ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@ObjectName);
                    EXEC sys.sp_executesql @Statement;";
                var resultado = connection.Query(sql, new
                    {
                        objetoSeguro.SchemaName,
                        objetoSeguro.ObjectName,
                    })
                    .Select(row => ((IDictionary<string, object>)row)
                        .ToDictionary(item => item.Key, item => item.Value is DBNull ? null : item.Value))
                    .ToList();

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<Dictionary<string, object?>>>(ex.Message);
            }
        }

        public ErrorDto<List<BDAnalisisEstructuraDto>> EstructuraObtener(string objeto)
        {
            try
            {
                using var connection = CrearConexion();
                var objetoSeguro = ObtenerObjeto(connection, objeto);
                if (objetoSeguro is null)
                {
                    return DbHelper.CreateErrorResponse<List<BDAnalisisEstructuraDto>>(
                        "El objeto solicitado no existe o no es una tabla válida.");
                }

                const string sql = @"
                    SELECT COLUMN_NAME AS Columna,
                           DATA_TYPE AS TipoDato,
                           IS_NULLABLE AS Nulos,
                           ISNULL(CONVERT(varchar(20), CHARACTER_MAXIMUM_LENGTH), '') AS Tamano
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = @SchemaName
                      AND TABLE_NAME = @ObjectName
                    ORDER BY ORDINAL_POSITION";

                var resultado = connection.Query<BDAnalisisEstructuraDto>(sql, new
                {
                    objetoSeguro.SchemaName,
                    objetoSeguro.ObjectName,
                }).ToList();

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<BDAnalisisEstructuraDto>>(ex.Message);
            }
        }

        private SqlConnection CrearConexion()
            => new(_config.GetConnectionString("DefaultConnString"));

        private static ObjetoSqlDto? ObtenerObjeto(SqlConnection connection, string objeto)
        {
            if (string.IsNullOrWhiteSpace(objeto)) return null;

            const string sql = @"
                SELECT TOP 1
                       SCHEMA_NAME(schema_id) AS SchemaName,
                       name AS ObjectName
                FROM sys.objects
                WHERE type = 'U'
                  AND name = @Objeto";

            return connection.QueryFirstOrDefault<ObjetoSqlDto>(sql, new { Objeto = objeto.Trim() });
        }

        private sealed class ObjetoSqlDto
        {
            public string SchemaName { get; set; } = string.Empty;
            public string ObjectName { get; set; } = string.Empty;
        }

    }
}
