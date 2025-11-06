using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class FrmUsModulosDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsModulosDb(IConfiguration config)
        {
            _config = config;
        }

        public List<ModuloDto> Modulo_ObtenerTodos()
        {
            List<ModuloDto> data = new List<ModuloDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Opciones_Modulos_Obtener]";
                    data = connection.Query<ModuloDto>(procedure, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        private ErrorDto Modulo_Insertar(ModuloDto request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Modulo_Insertar]";
                    var values = new
                    {
                        Modulo = request.Modulo,
                        Nombre = request.Nombre,
                        Descripcion = request.Descripcion,
                        Activo = request.Activo,

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

        public ErrorDto Modulo_Eliminar(int request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Modulo_Eliminar]";
                    var values = new
                    {
                        Modulo = request,
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

        private ErrorDto Modulo_Actualizar(ModuloDto request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Modulo_Editar]";
                    var values = new
                    {
                        Modulo = request.Modulo,
                        Nombre = request.Nombre,
                        Descripcion = request.Descripcion,
                        Activo = request.Activo,

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

        public ErrorDto Modulo_Guardar(ModuloDto request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
            {
                var query = "SELECT COUNT(*) FROM US_MODULOS WHERE Modulo = @Modulo";
                var count = connection.Query<int>(query, new { Modulo = request.Modulo }).FirstOrDefault();
                if (count > 0)
                {
                    resp = Modulo_Actualizar(request);
                }
                else
                {
                    resp = Modulo_Insertar(request);
                }

            }
            return resp;
        }
    }
}
