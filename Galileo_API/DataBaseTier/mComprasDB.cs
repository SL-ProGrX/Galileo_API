using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using System.Data;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class MComprasDB
    {
        private readonly IConfiguration _config;
        public MComprasDB(IConfiguration config)
        {
            _config = config;
        }


        public List<CargoPeriodicoDto> sbCprCboCargosPer(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<CargoPeriodicoDto> info = new List<CargoPeriodicoDto>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT cod_cargo,descripcion  FROM cxp_cargos order by cod_cargo";

                    info = connection.Query<CargoPeriodicoDto>(query).ToList();

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

                    vCambia = vNum == 1;

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return vCambia;
        }

        public ErrorDto sbCprOrdenesDespacho(int CodEmpresa, string vOrden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var query = string.Empty;
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    query = "select isnull(count(*),0) as Existe from cpr_ordenes_detalle WHERE cantidad - isnull(cantidad_despachada,0) > 1 AND cod_orden = @cod_orden";

                    var parameters = new DynamicParameters();
                    parameters.Add("cod_orden", vOrden, DbType.String);

                    var dapperinfo = connection.Query(query, parameters).FirstOrDefault() as dynamic;

                    if ((dapperinfo?.Existe ?? 0) == 0)
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

        public List<TipoOrdenDto> sbCprCboTiposOrden(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<TipoOrdenDto> info = new List<TipoOrdenDto>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT tipo_orden,descripcion FROM cpr_tipo_orden";

                    info = connection.Query<TipoOrdenDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public ErrorDto<UnidadesDtoList> UnidadesObtener(int CodEmpresa, string? filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            MComprasFiltros vfiltro = filtros != null
                ? JsonConvert.DeserializeObject<MComprasFiltros>(filtros) ?? new MComprasFiltros()
                : new MComprasFiltros();
            var response = new ErrorDto<UnidadesDtoList>();
            response.Result = new UnidadesDtoList();
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
                    response.Result.Unidades = connection.Query<UnidadesDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Unidades = new List<UnidadesDto>();
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto<CentroCostoDtoList> CentroCostosObtener(int CodEmpresa, string? filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            MComprasFiltros vfiltro = filtros != null
                ? JsonConvert.DeserializeObject<MComprasFiltros>(filtros) ?? new MComprasFiltros()
                : new MComprasFiltros();
            var response = new ErrorDto<CentroCostoDtoList>();
            response.Result = new CentroCostoDtoList();
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
                    response.Result.CentroCostos = connection.Query<CentroCostoDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.CentroCostos = new List<CentroCostoDto>();
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto<List<CatalogoDto>> CatalogoCompras_Obtener(int CodEmpresa, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CatalogoDto>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select CATALOGO_ID AS ITEM, DESCRIPCION from CPR_CATALOGOS 
                        where Tipo_Id = (select TIPO_ID from CPR_CATALOGOS_TIPOS where DESCRIPCION = '{tipo}') and Activo = 1";
                    response.Result = connection.Query<CatalogoDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CatalogoDto>();
            }

            return response;
        }

        public ErrorDto FacturaOrdenes_Actualizar(int CodEmpresa, string cod_factura, int cod_proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //busco cedula juridica del proveedor
                    string qProv = $@"SELECT CEDJUR FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = '{cod_proveedor}' ";
                    string cedJur = connection.Query<string>(qProv).FirstOrDefault() ?? string.Empty;


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
