using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCprTiposOrdenDB
    {
        private readonly IConfiguration _config;

        public frmCprTiposOrdenDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<TiposOrdenLista> ObtenerTiposOrdenes(int CodEmpresa, string jFiltros)
        {
            TipoOrdenFiltro filtro = JsonConvert.DeserializeObject<TipoOrdenFiltro>(jFiltros) ?? new TipoOrdenFiltro(); ;
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<TiposOrdenLista>
            {
                Result = new TiposOrdenLista()
            };
            response.Result.total = 0;

            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    var query = $@"Select COUNT(Tipo_Orden) from cpr_Tipo_Orden";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    string vFiltro = " ";

                    if (filtro.filtro != null)
                    {
                        vFiltro = " WHERE Tipo_Orden LIKE '%" + filtro.filtro + "%' OR descripcion LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.pagina > 0)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"Select Tipo_Orden,descripcion, activo  
                                from cpr_Tipo_Orden {vFiltro} order by Tipo_Orden 
                                {paginaActual} {paginacionActual}";
                    response.Result.lista = connection.Query<TiposOrdenDTO>(query).ToList();

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

        public ErrorDto TipoOrden_Actualizar(int CodEmpresa, TiposOrdenDTO tiposOrden)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto result = new ErrorDto();
            result.Code = 0;
            int activo = 0;
            try
            {
                activo = (tiposOrden.activo == true) ? 1 : 0;
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Update cpr_Tipo_Orden set 
                                         activo = {activo} , 
                                         descripcion = '{tiposOrden.descripcion}'
                                            where       
                                         Tipo_Orden = '{tiposOrden.tipo_orden}'";

                    connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        public ErrorDto TipoOrden_Eliminar(int CodEmpresa, string tiposOrden)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto result = new ErrorDto();
            result.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Delete from cpr_Tipo_Orden where Tipo_Orden = '{tiposOrden}'";
                    connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        public ErrorDto TipoOrden_Insertar(int CodEmpresa, TiposOrdenDTO tiposOrden)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto errorDTO = new ErrorDto();
            errorDTO.Code = 0;
            int activo = 0;
            try
            {
                tiposOrden.tipo_orden = ObtenerSequencia(CodEmpresa).ToString();
                activo = (tiposOrden.activo == true) ? 1 : 0;
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Insert into cpr_Tipo_Orden (Tipo_Orden,descripcion,activo) 
                                    values ('{tiposOrden.tipo_orden}','{tiposOrden.descripcion}',{activo})";

                    connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                errorDTO.Code = -1;
                errorDTO.Description = ex.Message;
            }
            return errorDTO;
        }

        public string ObtenerSequencia(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            string result = "00";

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT FORMAT(ISNULL(MAX(CAST(Tipo_Orden AS INT)), 0) + 1, '000') 
                                   FROM cpr_Tipo_Orden";

                    result = connection.Query<string>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        public ErrorDto<List<RangosMontos>> rangosMontos_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<RangosMontos>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT 
                                    r.cod_rango as item, 
                                    CONCAT(r.descripcion, ' - Mínimo: ', r.monto_minimo, ' Máximo: ', r.monto_maximo) AS descripcion,
                                    r.MONTO_MAXIMO, r.MONTO_MINIMO
                                FROM cpr_orden_rangos r
                                INNER JOIN CPR_RANGO_USUARIO u ON r.cod_rango = u.cod_rango
                                            WHERE r.REGISTRO_USUARIO = '{usuario}'";
                    response.Result = connection.Query<RangosMontos>(query).ToList();
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


    }
}
