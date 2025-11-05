using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmUS_ModulosDB
    {
        private readonly IConfiguration _config;

        public frmUS_ModulosDB(IConfiguration config)
        {
            _config = config;
        }

        public List<ModuloDTO> Modulo_ObtenerTodos()
        {
            List<ModuloDTO> data = new List<ModuloDTO>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Opciones_Modulos_Obtener]";
                    data = connection.Query<ModuloDTO>(procedure, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        private ErrorDTO Modulo_Insertar(ModuloDTO request)
        {
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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

        public ErrorDTO Modulo_Eliminar(int request)
        {
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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

        private ErrorDTO Modulo_Actualizar(ModuloDTO request)
        {
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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

        public ErrorDTO Modulo_Guardar(ModuloDTO request)
        {
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
