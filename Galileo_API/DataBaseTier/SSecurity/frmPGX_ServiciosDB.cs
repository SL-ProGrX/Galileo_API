using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPgxServiciosDB
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";
        private const int modulo = 13;
        private readonly MProGrXSecurityMainDb DBBitacora;

        public FrmPgxServiciosDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MProGrXSecurityMainDb(config);
        }

        public ErrorDto<List<ServicioSuscripcion>> Servicio_ObtenerTodos()
        {
            var response = new ErrorDto<List<ServicioSuscripcion>> { Result = new List<ServicioSuscripcion>(), Code = 0 };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Servicios_Obtener]";

                    response.Result = connection.Query<ServicioSuscripcion>(procedure, commandType: CommandType.StoredProcedure).ToList();
                    foreach (ServicioSuscripcion dt in response.Result)
                    {
                        dt.Estado = dt.Activo == 1 ? "ACTIVO" : "INACTIVO";
                        dt.PorUsuario = dt.Aplica_Por_Usuario == 1 ? "APLICA" : "NO_APLICA";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            response.Description = response.Code == 0 ? "Ok" : response.Description;
            return response;
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
                    RegistrarBitacora(request.CodEmpresa, request.Registro_Usuario, "REGISTRA", $"Servicio: {request.Cod_Servicio} - {request.Descripcion}");
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
                    RegistrarBitacora(request.CodEmpresa, request.Registro_Usuario, "ELIMINA", $"Servicio: {request.Cod_Servicio} - {request.Descripcion}");
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
                    RegistrarBitacora(request.CodEmpresa, request.Registro_Usuario, "MODIFICA", $"Servicio: {request.Cod_Servicio} - {request.Descripcion}");
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            if (codEmpresa <= 0 || string.IsNullOrWhiteSpace(usuario)) return;

            _ = DBBitacora.Bitacora(new MProGrXSecurityMainBitacora
            {
                CodEmpresa = codEmpresa,
                usuario = usuario,
                vModulo = modulo,
                strTipoMovimiento = $"{movimiento} - WEB",
                strDetalleMovimiento = detalle
            });
        }



    }
}
