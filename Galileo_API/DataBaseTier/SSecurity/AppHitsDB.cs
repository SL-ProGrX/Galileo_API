using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class AppHitsDB
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "BaseConnString";

        private const string ObtenerSql = """
            SELECT HIT_COD,
                   DESCRIPCION,
                   ACTIVO,
                   REGISTRO_USUARIO,
                   REGISTRO_FECHA
            FROM dbo.[APP_Estadistica]
            ORDER BY HIT_COD;
            """;

        private const string InsertarSql = """
            INSERT INTO dbo.[APP_Estadistica]
                (HIT_COD, DESCRIPCION, ACTIVO, REGISTRO_USUARIO, REGISTRO_FECHA)
            VALUES
                (@Hit_Cod, @Descripcion, @Activo, @Registro_Usuario, GETDATE());
            """;

        private const string EliminarSql = """
            DELETE FROM dbo.[APP_Estadistica]
            WHERE HIT_COD = @Hit_Cod;
            """;

        private const string ActualizarSql = """
            UPDATE dbo.[APP_Estadistica]
            SET DESCRIPCION = @Descripcion,
                ACTIVO = @Activo
            WHERE HIT_COD = @Hit_Cod;
            """;

        public AppHitsDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<AppHits>> AppHits_ObtenerTodos()
        {
            ErrorDto<List<AppHits>> resp = new ErrorDto<List<AppHits>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AppHits>(),
            };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));

                var types = connection.Query<AppHits>(ObtenerSql).ToList();
                resp.Result = types;
                foreach (AppHits dt in types)
                {
                    dt.Estado = dt.Activo == 1 ? "ACTIVO" : "INACTIVO";
                }

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto AppHits_Insertar(AppHits request)
        {

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));

                var values = new
                {
                    request.Hit_Cod,
                    request.Descripcion,
                    request.Activo,
                    request.Registro_Usuario,

                };

                connection.Execute(InsertarSql, values);
                resp.Code = 0;
                resp.Description = "Ok";

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto AppHits_Eliminar(AppHits request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));

                var values = new
                {
                    request.Hit_Cod,
                };

                connection.Execute(EliminarSql, values);
                resp.Code = 0;
                resp.Description = "Ok";

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto AppHits_Actualizar(AppHits request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));

                var values = new
                {
                    request.Hit_Cod,
                    request.Descripcion,
                    request.Activo,

                };

                connection.Execute(ActualizarSql, values);
                resp.Code = 0;
                resp.Description = "Ok";

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}
