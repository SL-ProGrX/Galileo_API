using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class ServicioDB
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public ServicioDB(IConfiguration config)
        {
            _config = config;
        }

        public List<ServicioSuscripcion> Servicio_ObtenerTodos()
        {
            List<ServicioSuscripcion> servs = new List<ServicioSuscripcion>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Servicios_Obtener]";

                    servs = connection.Query<ServicioSuscripcion>(procedure, commandType: CommandType.StoredProcedure).ToList();
                    foreach (ServicioSuscripcion dt in servs)
                    {
                        dt.Estado = dt.Activo == 1 ? "ACTIVO" : "INACTIVO";
                        dt.PorUsuario = dt.Aplica_Por_Usuario == 1 ? "APLICA" : "NO_APLICA";
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return servs;
        }

        public ErrorDto Servicio_Insertar(ServicioSuscripcion request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Servicios_Insertar]";
                    var values = new
                    {
                        Cod_Servicio = request.Cod_Servicio,
                        Descripcion = request.Descripcion,
                        Activo = request.Activo,
                        Costo = request.Costo,
                        Aplica_Por_Usuario = request.Aplica_Por_Usuario,
                        Registro_Usuario = request.Registro_Usuario,

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

        public ErrorDto Servicio_Eliminar(ServicioSuscripcion request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Servicios_Eliminar]";
                    var values = new
                    {
                        Cod_Servicio = request.Cod_Servicio,
                        //ModificaUsuario = request.ModificaUsuario,

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

        public ErrorDto Servicio_Actualizar(ServicioSuscripcion request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Servicios_Editar]";
                    var values = new
                    {
                        Cod_Servicio = request.Cod_Servicio,
                        Descripcion = request.Descripcion,
                        Activo = request.Activo,
                        Costo = request.Costo,
                        Aplica_Por_Usuario = request.Aplica_Por_Usuario,

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
