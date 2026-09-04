using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPgxClientesClasificaDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";
        private const int moduloBitacora = 13;
        private readonly MProGrXSecurityMainDb DBBitacora;

        public FrmPgxClientesClasificaDb(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MProGrXSecurityMainDb(config);
        }

        public ErrorDto<List<ClienteClasifica>> Cliente_Clasifica_ObtenerTodos()
        {
            var response = new ErrorDto<List<ClienteClasifica>> { Result = new List<ClienteClasifica>(), Code = 0 };
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Clientes_Clasifica_Obtener]";

                    response.Result = connection.Query<ClienteClasifica>(procedure, commandType: CommandType.StoredProcedure).ToList();
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

        private ErrorDto Cliente_Clasifica_Insertar(ClienteClasifica request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Clientes_Clasifica_Insertar]";
                    var values = new
                    {
                        Cod_Clasificacion = request.Cod_Clasificacion,
                        Descripcion = request.Descripcion,
                        Activa = request.Activa,
                        Registro_Usuario = request.Registro_Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                    if (resp.Code == 0)
                    {
                        RegistrarBitacora(request, "REGISTRA", $"Cliente Clasifica: {request.Cod_Clasificacion}");
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto Cliente_Clasifica_Eliminar(string request, int codEmpresa, string usuario)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Clientes_Clasifica_Eliminar]";
                    var values = new
                    {
                        Cod_Clasificacion = request,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                    if (resp.Code == 0)
                    {
                        RegistrarBitacora(new ClienteClasifica
                        {
                            CodEmpresa = codEmpresa,
                            Cod_Clasificacion = request,
                            Registro_Usuario = usuario
                        }, "ELIMINA", $"Cliente Clasifica: {request}");
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        
        private ErrorDto Cliente_Clasifica_Actualizar(ClienteClasifica request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_W_Clientes_Clasifica_Editar]";
                    var values = new
                    {
                        Cod_Clasificacion = request.Cod_Clasificacion,
                        Descripcion = request.Descripcion,
                        Activa = request.Activa,
                        //   Registro_Usuario = request.Registro_Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                    if (resp.Code == 0)
                    {
                        RegistrarBitacora(request, "MODIFICA", $"Cliente Clasifica: {request.Cod_Clasificacion}");
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private void RegistrarBitacora(ClienteClasifica request, string movimiento, string detalle)
        {
            if (request.CodEmpresa <= 0 || string.IsNullOrWhiteSpace(request.Registro_Usuario))
            {
                return;
            }

            _ = DBBitacora.Bitacora(new MProGrXSecurityMainBitacora
            {
                CodEmpresa = request.CodEmpresa,
                usuario = request.Registro_Usuario,
                vModulo = moduloBitacora,
                strTipoMovimiento = $"{movimiento} - WEB",
                strDetalleMovimiento = detalle
            });
        }

        public List<ClienteSelecciona> Cliente_Selecciona_ObtenerTodos(string usuario)
        {
            List<ClienteSelecciona> data = new List<ClienteSelecciona>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spSEG_Admin_Client_Access_List]";
                    var values = new
                    {
                        Usuario = usuario,
                        Filtro = "",
                        Top = 30,
                    };

                    data = connection.Query<ClienteSelecciona>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public ErrorDto Cliente_Clasifica_Guardar(ClienteClasifica request)
        {
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    //valido si existe codigo
                    var query = "SELECT COUNT(*) FROM PGX_CLIENTES_CLASIFICACION WHERE Cod_Clasificacion = @Cod_Clasificacion";
                    var count = connection.Query<int>(query, new { Cod_Clasificacion = request.Cod_Clasificacion }).FirstOrDefault();
                    if (count > 0)
                    {
                        resp = Cliente_Clasifica_Actualizar(request);
                    }
                    else
                    {
                        resp = Cliente_Clasifica_Insertar(request);
                    }
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
