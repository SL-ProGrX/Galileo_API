using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPgxVendedoresDB
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";
        private const int modulo = 13;
        private readonly MProGrXSecurityMainDb DBBitacora;

        public FrmPgxVendedoresDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MProGrXSecurityMainDb(config);
        }

        public ErrorDto<List<Vendedor>> Vendedor_ObtenerTodos()
        {
            var response = new ErrorDto<List<Vendedor>>
            {
                Result = new List<Vendedor>(),
                Code = 0,
                Description = string.Empty,
            };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Vendedor_Obtener]";

                    response.Result = connection.Query<Vendedor>(procedure, commandType: CommandType.StoredProcedure).ToList();
                    foreach (Vendedor dt in response.Result)
                    {
                        dt.Estado = dt.Activo == 1 ? "ACTIVO" : "INACTIVO";

                    }
                    response.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto Vendedor_Insertar(Vendedor request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Vendedor_Insertar]";
                    var values = new
                    {
                        Cod_Vendedor = request.Cod_Vendedor,
                        Identificacion = request.Identificacion,
                        Nombre = request.Nombre,
                        Activo = request.Activo,
                        Comision_Tipo = request.Comision_Tipo,
                        Comision_Cliente = request.Comision_Cliente,
                        Cuenta_Cliente = request.Cuenta_Cliente,
                        Registro_Usuario = request.Registro_Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                    RegistrarBitacora(request.CodEmpresa ?? 0, request.Registro_Usuario, "REGISTRA", $"Vendedor: {request.Cod_Vendedor}");
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto Vendedor_Eliminar(Vendedor request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Vendedor_Eliminar]";
                    var values = new
                    {
                        Cod_Vendedor = request.Cod_Vendedor,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                    RegistrarBitacora(request.CodEmpresa ?? 0, request.Registro_Usuario, "ELIMINA", $"Vendedor: {request.Cod_Vendedor}");
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto Vendedor_Actualizar(Vendedor request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Vendedor_Editar]";
                    var values = new
                    {
                        Cod_Vendedor = request.Cod_Vendedor,
                        Identificacion = request.Identificacion,
                        Nombre = request.Nombre,
                        Activo = request.Activo,
                        Comision_Tipo = request.Comision_Tipo,
                        Comision_Cliente = request.Comision_Cliente,
                        Cuenta_Cliente = request.Cuenta_Cliente,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                    RegistrarBitacora(request.CodEmpresa ?? 0, request.Registro_Usuario, "MODIFICA", $"Vendedor: {request.Cod_Vendedor}");
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
