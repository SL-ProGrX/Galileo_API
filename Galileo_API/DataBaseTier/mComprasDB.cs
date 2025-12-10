using System.Data;
using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class MComprasDB
    {
        private readonly PortalDB _portalDB;

        public MComprasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        #region Helpers privados

        private static void AddFiltroYPaginacionParameters(MComprasFiltros vfiltro, DynamicParameters parameters)
        {
            // Filtro
            var tieneFiltro = !string.IsNullOrWhiteSpace(vfiltro.filtro);
            parameters.Add("@HasFiltro", tieneFiltro ? 1 : 0, DbType.Int32);
            parameters.Add("@Filtro",
                tieneFiltro ? $"%{vfiltro.filtro}%" : (object)DBNull.Value,
                DbType.String);

            // Paginación
            var paginar = vfiltro.pagina.HasValue && vfiltro.paginacion.HasValue;
            parameters.Add("@Paginar", paginar ? 1 : 0, DbType.Int32);
            parameters.Add("@Offset", paginar ? vfiltro.pagina!.Value : 0, DbType.Int32);
            parameters.Add("@PageSize", paginar ? vfiltro.paginacion!.Value : int.MaxValue, DbType.Int32);
        }

        #endregion

        #region Cargos / Tipos Orden

        public List<CargoPeriodicoDto> sbCprCboCargosPer(int codEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<CargoPeriodicoDto>(
                _portalDB,
                codEmpresa,
                "SELECT cod_cargo, descripcion FROM cxp_cargos ORDER BY cod_cargo");

            return result.Result ?? new List<CargoPeriodicoDto>();
        }

        public List<TipoOrdenDto> sbCprCboTiposOrden(int codEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<TipoOrdenDto>(
                _portalDB,
                codEmpresa,
                "SELECT tipo_orden, descripcion FROM cpr_tipo_orden");

            return result.Result ?? new List<TipoOrdenDto>();
        }

        #endregion

        #region Cambia Fecha

        public bool fxCprCambiaFecha(int codEmpresa, string usuario)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM cpr_INVUSRFECHAS 
                WHERE usuario = @Usuario;";

            var result = DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                codEmpresa,
                sql,
                defaultValue: 0,
                parameters: new { Usuario = usuario });

            var count = result.Result;
            return result.Code == 0 && count == 1;
        }

        #endregion

        #region Ordenes Despacho

        public ErrorDto sbCprOrdenesDespacho(int codEmpresa, string codOrden)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                const string sqlExiste = @"
                    SELECT ISNULL(COUNT(*), 0) 
                    FROM cpr_ordenes_detalle 
                    WHERE cantidad - ISNULL(cantidad_despachada, 0) > 1 
                      AND cod_orden = @CodOrden;";

                using var connection = _portalDB.CreateConnection(codEmpresa);

                var existe = connection.QueryFirstOrDefault<int>(
                    sqlExiste,
                    new { CodOrden = codOrden });

                if (existe > 0)
                {
                    const string sqlUpdate = @"
                        UPDATE cpr_ordenes 
                        SET proceso = 'D' 
                        WHERE cod_orden = @CodOrden;";

                    var rows = connection.Execute(sqlUpdate, new { CodOrden = codOrden });

                    resp.Code = rows;
                    resp.Description = "Ok";
                }
                else
                {
                    resp.Code = -1;
                    resp.Description = "No hay cantidades pendientes por despachar";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        #endregion

        #region Unidades

        public ErrorDto<UnidadesDtoList> UnidadesObtener(int codEmpresa, string? filtros)
        {
            var vfiltro = filtros != null
                ? JsonConvert.DeserializeObject<MComprasFiltros>(filtros) ?? new MComprasFiltros()
                : new MComprasFiltros();

            var response = DbHelper.CreateOkResponse(new UnidadesDtoList());

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@CodConta", vfiltro.CodConta, DbType.Int32);
                AddFiltroYPaginacionParameters(vfiltro, parameters);

                const string sqlCount = @"
                    SELECT COUNT(*) 
                    FROM CntX_Unidades
                    WHERE COD_CONTABILIDAD = @CodConta
                      AND (@HasFiltro = 0 OR (COD_UNIDAD LIKE @Filtro OR descripcion LIKE @Filtro));";

                const string sqlData = @"
                    SELECT cod_unidad AS unidad, descripcion 
                    FROM CntX_Unidades
                    WHERE COD_CONTABILIDAD = @CodConta
                      AND (@HasFiltro = 0 OR (COD_UNIDAD LIKE @Filtro OR descripcion LIKE @Filtro))
                    ORDER BY COD_UNIDAD DESC
                    OFFSET CASE WHEN @Paginar = 1 THEN @Offset ELSE 0 END ROWS
                    FETCH NEXT CASE WHEN @Paginar = 1 THEN @PageSize ELSE 2147483647 END ROWS ONLY;";

                var data = response.Result ??= new UnidadesDtoList();

                data.Total = connection.QueryFirstOrDefault<int>(sqlCount, parameters);
                data.Unidades = connection.Query<UnidadesDto>(sqlData, parameters).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;

                var data = response.Result ??= new UnidadesDtoList();
                data.Unidades = new List<UnidadesDto>();
                data.Total = 0;
            }

            return response;
        }

        #endregion

        #region Centros de Costo

        public ErrorDto<CentroCostoDtoList> CentroCostosObtener(int codEmpresa, string? filtros)
        {
            var vfiltro = filtros != null
                ? JsonConvert.DeserializeObject<MComprasFiltros>(filtros) ?? new MComprasFiltros()
                : new MComprasFiltros();

            var response = DbHelper.CreateOkResponse(new CentroCostoDtoList());

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@CodConta", vfiltro.CodConta, DbType.Int32);
                AddFiltroYPaginacionParameters(vfiltro, parameters);

                const string sqlCount = @"
                    SELECT COUNT(*) 
                    FROM CNTX_CENTRO_COSTOS
                    WHERE COD_CONTABILIDAD = @CodConta
                      AND (@HasFiltro = 0 OR (cod_centro_costo LIKE @Filtro OR descripcion LIKE @Filtro));";

                const string sqlData = @"
                    SELECT cod_centro_costo AS centrocosto, descripcion 
                    FROM CNTX_CENTRO_COSTOS
                    WHERE COD_CONTABILIDAD = @CodConta
                      AND (@HasFiltro = 0 OR (cod_centro_costo LIKE @Filtro OR descripcion LIKE @Filtro))
                    ORDER BY cod_centro_costo DESC
                    OFFSET CASE WHEN @Paginar = 1 THEN @Offset ELSE 0 END ROWS
                    FETCH NEXT CASE WHEN @Paginar = 1 THEN @PageSize ELSE 2147483647 END ROWS ONLY;";

                var data = response.Result ??= new CentroCostoDtoList();

                data.Total = connection.QueryFirstOrDefault<int>(sqlCount, parameters);
                data.CentroCostos = connection
                    .Query<CentroCostoDto>(sqlData, parameters)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;

                var data = response.Result ??= new CentroCostoDtoList();
                data.CentroCostos = new List<CentroCostoDto>();
                data.Total = 0;
            }

            return response;
        }

        #endregion

        #region Catálogo Compras

        public ErrorDto<List<CatalogoDto>> CatalogoCompras_Obtener(int codEmpresa, string tipo)
        {
            var response = DbHelper.CreateOkResponse(new List<CatalogoDto>());

            try
            {
                const string sql = @"
                    SELECT CATALOGO_ID AS ITEM, DESCRIPCION 
                    FROM CPR_CATALOGOS 
                    WHERE Tipo_Id = (
                        SELECT TIPO_ID 
                        FROM CPR_CATALOGOS_TIPOS 
                        WHERE DESCRIPCION = @Tipo
                    ) 
                      AND Activo = 1;";

                using var connection = _portalDB.CreateConnection(codEmpresa);
                response.Result = connection.Query<CatalogoDto>(sql, new { Tipo = tipo }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CatalogoDto>();
            }

            return response;
        }

        #endregion

        #region Facturas / Órdenes

        public ErrorDto FacturaOrdenes_Actualizar(int codEmpresa, string codFactura, int codProveedor)
        {
            var response = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);

                const string sqlProv = @"
                    SELECT CEDJUR 
                    FROM CXP_PROVEEDORES 
                    WHERE COD_PROVEEDOR = @CodProveedor;";

                var cedJur = connection.QueryFirstOrDefault<string>(
                                 sqlProv,
                                 new { CodProveedor = codProveedor }
                             ) ?? string.Empty;

                cedJur = cedJur.Replace("-", "").Replace(" ", "");

                const string sqlUpdate = @"
                    UPDATE CPR_FACTURAS_XML 
                    SET ESTADO = 'R' 
                    WHERE COD_DOCUMENTO = @CodFactura 
                      AND CED_JUR_PROV = @CedJurProv;";

                var rows = connection.Execute(sqlUpdate, new
                {
                    CodFactura = codFactura,
                    CedJurProv = cedJur
                });

                response.Code = rows;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion
    }
}