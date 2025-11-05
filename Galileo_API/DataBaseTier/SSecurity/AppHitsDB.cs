using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class AppHitsDB
    {
        private readonly IConfiguration _config;

        public AppHitsDB(IConfiguration config)
        {
            _config = config;
        }

        public List<AppHits> AppHits_ObtenerTodos()
        {


            List<AppHits> types = new List<AppHits>();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                {
                    var procedure = "[spPGX_W_AppHits_Obtener]";

                    types = connection.Query<AppHits>(procedure, commandType: CommandType.StoredProcedure).ToList();
                    foreach (AppHits dt in types)
                    {
                        dt.Estado = dt.Activo == 1 ? "ACTIVO" : "INACTIVO";
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return types;
        }

        public ErrorAppHitsDTO AppHits_Insertar(AppHits request)
        {

            ErrorAppHitsDTO resp = new ErrorAppHitsDTO();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                {
                    var procedure = "[spPGX_W_AppHits_Insertar]";
                    var values = new
                    {
                        request.Hit_Cod,
                        request.Descripcion,
                        request.Activo,
                        request.Registro_Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorAppHitsDTO AppHits_Eliminar(AppHits request)
        {
            ErrorAppHitsDTO resp = new ErrorAppHitsDTO();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                {
                    var procedure = "[spPGX_W_AppHits_Eliminar]";
                    var values = new
                    {
                        request.Hit_Cod,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorAppHitsDTO AppHits_Actualizar(AppHits request)
        {
            ErrorAppHitsDTO resp = new ErrorAppHitsDTO();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                {
                    var procedure = "[spPGX_W_AppHits_Editar]";
                    var values = new
                    {
                        request.Hit_Cod,
                        request.Descripcion,
                        request.Activo,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
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
