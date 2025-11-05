using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using System.Data;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class mComprasDB
    {
        private readonly IConfiguration _config;
        public mComprasDB(IConfiguration config)
        {
            _config = config;
        }


        public List<CargoPeriodicoDTO> sbCprCboCargosPer(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<CargoPeriodicoDTO> info = new List<CargoPeriodicoDTO>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT cod_cargo,descripcion  FROM cxp_cargos order by cod_cargo";

                    info = connection.Query<CargoPeriodicoDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public bool fxCprCambiaFecha(int CodEmpresa, string vUsuario)
        {
            bool vCambia = false;
            int vNum = 0;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT ISNULL(COUNT(*),0) AS Existe FROM cpr_INVUSRFECHAS WHERE usuario = @usuario";
                    var parameters = new DynamicParameters();
                    parameters.Add("usuario", vUsuario, DbType.String);

                    vNum = connection.ExecuteAsync(query, parameters).Result;

                    _ = vNum == 1 ? vCambia = true : vCambia = false;

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return vCambia;
        }

        public ErrorDTO sbCprOrdenesDespacho(int CodEmpresa, string vOrden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var query = string.Empty;
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    query = "select isnull(count(*),0) as Existe from cpr_ordenes_detalle WHERE cantidad - isnull(cantidad_despachada,0) > 1 AND cod_orden = @cod_orden";

                    var parameters = new DynamicParameters();
                    parameters.Add("cod_orden", vOrden, DbType.String);

                    var dapperinfo = connection.Query(query, parameters).FirstOrDefault();

                    if (dapperinfo.Existe == 0)
                    {
                        query = "update cpr_ordenes set proceso = 'D' where cod_orden = cod_orden = @cod_orden";
                    }
                    else
                    {
                        query = "update cpr_ordenes set proceso = 'D' where cod_orden = cod_orden = @cod_orden";
                    }
                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
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

        public List<TipoOrdenDTO> sbCprCboTiposOrden(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<TipoOrdenDTO> info = new List<TipoOrdenDTO>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT tipo_orden,descripcion FROM cpr_tipo_orden";

                    info = connection.Query<TipoOrdenDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public ErrorDTO<UnidadesDTOList> UnidadesObtener(int CodEmpresa, string? filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            mComprasFiltros vfiltro = JsonConvert.DeserializeObject<mComprasFiltros>(filtros);
            var response = new ErrorDTO<UnidadesDTOList>();
            response.Result = new UnidadesDTOList();
            response.Code = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                string where = $"where COD_CONTABILIDAD = {vfiltro.CodConta}";
                using var connection = new SqlConnection(clienteConnString);
                {
                    if (vfiltro.filtro != null && vfiltro.filtro != "")
                    {
                        where += " and COD_UNIDAD LIKE '%" + vfiltro.filtro + "%' OR descripcion LIKE '%" + vfiltro.filtro + "%' ";
                    }

                    if (vfiltro.pagina != null)
                    {
                        paginaActual = " OFFSET " + vfiltro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + vfiltro.paginacion + " ROWS ONLY ";
                    }

                    query = $"select COUNT(*) from CntX_Unidades {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = @$"select cod_unidad as unidad, descripcion from CntX_Unidades 
                        {where} order by COD_UNIDAD desc {paginaActual} {paginacionActual}";
                    response.Result.unidades = connection.Query<UnidadesDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.unidades = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDTO<CentroCostoDTOList> CentroCostosObtener(int CodEmpresa, string? filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            mComprasFiltros vfiltro = JsonConvert.DeserializeObject<mComprasFiltros>(filtros);
            var response = new ErrorDTO<CentroCostoDTOList>();
            response.Result = new CentroCostoDTOList();
            response.Code = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                string where = $"where COD_CONTABILIDAD = {vfiltro.CodConta}";
                using var connection = new SqlConnection(clienteConnString);
                {
                    if (vfiltro.filtro != null && vfiltro.filtro != "")
                    {
                        where += " and cod_centro_costo LIKE '%" + vfiltro.filtro + "%' OR descripcion LIKE '%" + vfiltro.filtro + "%' ";
                    }

                    if (vfiltro.pagina != null)
                    {
                        paginaActual = " OFFSET " + vfiltro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + vfiltro.paginacion + " ROWS ONLY ";
                    }

                    query = $"select COUNT(*) from CNTX_CENTRO_COSTOS {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = @$"select cod_centro_costo as centrocosto, descripcion from CNTX_CENTRO_COSTOS
                        {where} order by cod_centro_costo desc {paginaActual} {paginacionActual}";
                    response.Result.centrocostos = connection.Query<CentroCostoDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.centrocostos = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDTO<List<CatalogoDTO>> CatalogoCompras_Obtener(int CodEmpresa, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CatalogoDTO>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select CATALOGO_ID AS ITEM, DESCRIPCION from CPR_CATALOGOS 
                        where Tipo_Id = (select TIPO_ID from CPR_CATALOGOS_TIPOS where DESCRIPCION = '{tipo}') and Activo = 1";
                    response.Result = connection.Query<CatalogoDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CatalogoDTO>();
            }

            return response;
        }

        public ErrorDTO FacturaOrdenes_Actualizar(int CodEmpresa, string cod_factura, int cod_proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //busco cedula juridica del proveedor
                    string qProv = $@"SELECT CEDJUR FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = '{cod_proveedor}' ";
                    string cedJur = connection.Query<string>(qProv).FirstOrDefault();


                    string query = $@"UPDATE CPR_FACTURAS_XML SET ESTADO = 'R' 
                                        WHERE COD_DOCUMENTO = '{cod_factura}' AND CED_JUR_PROV = '{cedJur.Replace("-","").Replace(" ","")}'";
                    response.Code = connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

    }
}
