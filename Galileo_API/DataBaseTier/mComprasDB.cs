using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using System.Data;
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

        // --------------------------------------------------------------------
        // Cargos
        // --------------------------------------------------------------------
        public List<CargoPeriodicoDto> sbCprCboCargosPer(int codEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<CargoPeriodicoDto>(
                _portalDB,
                codEmpresa,
                "SELECT cod_cargo, descripcion FROM cxp_cargos ORDER BY cod_cargo");

            return result.Result ?? new List<CargoPeriodicoDto>();
        }

        // --------------------------------------------------------------------
        // Cambia Fecha
        // --------------------------------------------------------------------
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

            // Si hubo error, devolvemos false
            var count = result.Result; // int, no referencia, no CS8602 aquí
            return result.Code == 0 && count == 1;
        }

        // --------------------------------------------------------------------
        // Ordenes Despacho
        // --------------------------------------------------------------------
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

        // --------------------------------------------------------------------
        // Tipos Orden
        // --------------------------------------------------------------------
        public List<TipoOrdenDto> sbCprCboTiposOrden(int codEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<TipoOrdenDto>(
                _portalDB,
                codEmpresa,
                "SELECT tipo_orden, descripcion FROM cpr_tipo_orden");

            return result.Result ?? new List<TipoOrdenDto>();
        }

        // --------------------------------------------------------------------
        // Unidades
        // --------------------------------------------------------------------
        public ErrorDto<UnidadesDtoList> UnidadesObtener(int codEmpresa, string? filtros)
        {
            var vfiltro = filtros != null
                ? JsonConvert.DeserializeObject<MComprasFiltros>(filtros) ?? new MComprasFiltros()
                : new MComprasFiltros();

            var response = DbHelper.CreateOkResponse(new UnidadesDtoList());

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);

                var where = "WHERE COD_CONTABILIDAD = @CodConta";
                var parameters = new DynamicParameters();
                parameters.Add("@CodConta", vfiltro.CodConta, DbType.Int32);

                if (!string.IsNullOrWhiteSpace(vfiltro.filtro))
                {
                    where += " AND (COD_UNIDAD LIKE @Filtro OR descripcion LIKE @Filtro)";
                    parameters.Add("@Filtro", $"%{vfiltro.filtro}%", DbType.String);
                }

                string paginaActual = string.Empty;
                string paginacionActual = string.Empty;

                if (vfiltro.pagina.HasValue && vfiltro.paginacion.HasValue)
                {
                    paginaActual     = $" OFFSET {vfiltro.pagina.Value} ROWS ";
                    paginacionActual = $" FETCH NEXT {vfiltro.paginacion.Value} ROWS ONLY ";
                }

                // Evitar CS8602: aseguramos que Result no es null y usamos una variable local
                var data = response.Result ??= new UnidadesDtoList();

                var sqlCount = $"SELECT COUNT(*) FROM CntX_Unidades {where}";
                data.Total = connection.QueryFirstOrDefault<int>(sqlCount, parameters);

                var sqlData = $@"
                    SELECT cod_unidad AS unidad, descripcion 
                    FROM CntX_Unidades
                    {where}
                    ORDER BY COD_UNIDAD DESC
                    {paginaActual} {paginacionActual}";

                data.Unidades = connection.Query<UnidadesDto>(sqlData, parameters).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;

                // De nuevo, asegurar que Result no es null antes de usarlo
                var data = response.Result ??= new UnidadesDtoList();
                data.Unidades = new List<UnidadesDto>();
                data.Total = 0;
            }

            return response;
        }

        // --------------------------------------------------------------------
        // Centro de Costos
        // --------------------------------------------------------------------
        public ErrorDto<CentroCostoDtoList> CentroCostosObtener(int codEmpresa, string? filtros)
        {
            var vfiltro = filtros != null
                ? JsonConvert.DeserializeObject<MComprasFiltros>(filtros) ?? new MComprasFiltros()
                : new MComprasFiltros();

            var response = DbHelper.CreateOkResponse(new CentroCostoDtoList());

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);

                var where = "WHERE COD_CONTABILIDAD = @CodConta";
                var parameters = new DynamicParameters();
                parameters.Add("@CodConta", vfiltro.CodConta, DbType.Int32);

                if (!string.IsNullOrWhiteSpace(vfiltro.filtro))
                {
                    where += " AND (cod_centro_costo LIKE @Filtro OR descripcion LIKE @Filtro)";
                    parameters.Add("@Filtro", $"%{vfiltro.filtro}%", DbType.String);
                }

                string paginaActual = string.Empty;
                string paginacionActual = string.Empty;

                if (vfiltro.pagina.HasValue && vfiltro.paginacion.HasValue)
                {
                    paginaActual     = $" OFFSET {vfiltro.pagina.Value} ROWS ";
                    paginacionActual = $" FETCH NEXT {vfiltro.paginacion.Value} ROWS ONLY ";
                }

                // Evitar CS8602 con variable local
                var data = response.Result ??= new CentroCostoDtoList();

                var sqlCount = $"SELECT COUNT(*) FROM CNTX_CENTRO_COSTOS {where}";
                data.Total = connection.QueryFirstOrDefault<int>(sqlCount, parameters);

                var sqlData = $@"
                    SELECT cod_centro_costo AS centrocosto, descripcion 
                    FROM CNTX_CENTRO_COSTOS
                    {where}
                    ORDER BY cod_centro_costo DESC
                    {paginaActual} {paginacionActual}";

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

        // --------------------------------------------------------------------
        // Catálogo Compras
        // --------------------------------------------------------------------
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

        // --------------------------------------------------------------------
        // Factura / Órdenes
        // --------------------------------------------------------------------
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

                var cedJur = connection.QueryFirstOrDefault<string>(sqlProv, new { CodProveedor = codProveedor }) ?? string.Empty;
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
    }
}