using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPTiposProvDB
    {

        private readonly IConfiguration _config;

        public frmCxPTiposProvDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<List<TiposProveedorDto>> ObtenerClasificacionProveedores(int CodCliente)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<List<TiposProveedorDto>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "SELECT cod_clasificacion as 'CodClasificacion',descripcion as 'Descripcion',NIT_Codigo as 'NitCodigo', Activo FROM cxp_prov_clas ORDER BY cod_clasificacion";

                    response.Result = connection.Query<TiposProveedorDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDTO<List<Proveedor>> ObtenerProveedores(int CodCliente)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<List<Proveedor>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "SELECT COD_PROVEEDOR, DESCRIPCION FROM CXP_PROVEEDORES WHERE ESTADO = 'A' ORDER BY COD_PROVEEDOR";

                    response.Result = connection.Query<Proveedor>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        public ErrorDTO TipoProveedor_Actualizar(TiposProveedorDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(request.CodEmpresa);

            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "UPDATE cxp_prov_clas SET descripcion = @Descripcion, nit_codigo = @NitCodigo, Activo = @Activo WHERE cod_clasificacion = @CodClasificacion";

                    var parameters = new DynamicParameters();
                    parameters.Add("CodClasificacion", request.CodClasificacion, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("NitCodigo", request.NitCodigo, DbType.String);
                    parameters.Add("Activo", request.Activo, DbType.Int32);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Tipo proveedor actualizado correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO TipoProveedor_Eliminar(TiposProveedorDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(request.CodEmpresa);

            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "DELETE cxp_prov_clas WHERE cod_clasificacion = @CodClasificacion";

                    var parameters = new DynamicParameters();
                    parameters.Add("CodClasificacion", request.CodClasificacion, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Tipo proveedor eliminado correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        public ErrorDTO TipoProveedor_Insertar(TiposProveedorDto request)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(request.CodEmpresa);

            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = "[spCxP_W_TipoProveeedor_Agregar]";
                    var values = new
                    {
                        Cod_Clasificacion = request.CodClasificacion,
                        Descripcion = request.Descripcion,
                        Nit_Codigo = request.NitCodigo,
                        Activo = request.Activo,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Tipo Proveedor agregado correctamente";
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
