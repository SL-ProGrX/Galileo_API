using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class DataDB
    {
        private readonly PortalDB _portalDB;
        private const string _filtroParam = "@Filtro";

        public DataDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        #region Helpers comunes

        private static string BuildPaginationClause(
            DynamicParameters parameters,
            int? pagina,
            int? paginacion)
        {
            if (pagina.HasValue && paginacion.HasValue)
            {
                parameters.Add("@Offset", pagina.Value, DbType.Int32);
                parameters.Add("@PageSize", paginacion.Value, DbType.Int32);
                return " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            }

            return string.Empty;
        }

        #endregion

        #region Proveedores

        public ErrorDto<ProveedoresDataLista> Proveedores_Obtener(int CodCliente, ProveedorDataFiltros jFiltros)
        {
            var response = new ErrorDto<ProveedoresDataLista>
            {
                Result = new ProveedoresDataLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                // WHERE base sin texto del usuario
                var baseWhere = "(ESTADO = 'A' OR ESTADO = 'T')";

                if (jFiltros.autoGestion == true && jFiltros.ventas == true)
                {
                    baseWhere += " AND (WEB_AUTO_GESTION = 1 OR WEB_FERIAS = 1)";
                }
                else if (jFiltros.autoGestion == true)
                {
                    baseWhere += " AND WEB_AUTO_GESTION = 1";
                }
                else if (jFiltros.ventas == true)
                {
                    baseWhere += " AND WEB_FERIAS = 1";
                }

                // Total
                var countSql = $"SELECT COUNT(*) FROM CXP_PROVEEDORES WHERE {baseWhere}";
                response.Result.Total = connection.QueryFirstOrDefault<int>(countSql);

                // Data
                var where = baseWhere;
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(jFiltros.filtro))
                {
                    where += " AND (COD_PROVEEDOR LIKE @Filtro OR DESCRIPCION LIKE @Filtro)";
                    parameters.Add(_filtroParam, $"%{jFiltros.filtro}%", DbType.String);
                }

                var pagination = BuildPaginationClause(parameters, jFiltros.pagina, jFiltros.paginacion);

                var dataSql = $@"
                    SELECT COD_PROVEEDOR, DESCRIPCION, CEDJUR 
                    FROM CXP_PROVEEDORES
                    WHERE {where}
                    ORDER BY COD_PROVEEDOR
                    {pagination}";

                response.Result.Proveedores = connection.Query<ProveedorData>(dataSql, parameters).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Total = 0;
                response.Result.Proveedores = new List<ProveedorData>();
            }

            return response;
        }

        #endregion

        #region Cargos

        public CargoDataLista Cargos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var info = new CargoDataLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                const string countSql = "SELECT COUNT(COD_CARGO) FROM CXP_CARGOS WHERE ACTIVO = 1";
                info.Total = connection.QueryFirstOrDefault<int>(countSql);

                var where = "ACTIVO = 1";
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    where += " AND (COD_CARGO LIKE @Filtro OR DESCRIPCION LIKE @Filtro)";
                    parameters.Add(_filtroParam, $"%{filtro}%", DbType.String);
                }

                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT COD_CARGO, DESCRIPCION, 0 AS MONTO 
                    FROM CXP_CARGOS 
                    WHERE {where}
                    ORDER BY COD_CARGO
                    {pagination}";

                info.Cargos = connection.Query<CargoData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Cargos = new List<CargoData>();
            }

            return info;
        }

        #endregion

        #region Bodegas

        public BodegaDataLista Bodegas_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var info = new BodegaDataLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                const string countSql = "SELECT COUNT(cod_bodega) FROM pv_bodegas WHERE permite_salidas = 1";
                info.Total = connection.QueryFirstOrDefault<int>(countSql);

                var where = "permite_salidas = 1";
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    where += " AND (cod_bodega LIKE @Filtro OR descripcion LIKE @Filtro)";
                    parameters.Add(_filtroParam, $"%{filtro}%", DbType.String);
                }

                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT cod_bodega, descripcion 
                    FROM pv_bodegas 
                    WHERE {where}
                    ORDER BY cod_bodega
                    {pagination}";

                info.bodegas = connection.Query<BodegaData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.bodegas = new List<BodegaData>();
            }

            return info;
        }

        #endregion

        #region Artículos

        public ErrorDto<ArticuloDataLista> Articulos_Obtener(int CodCliente, ArticuloDataFiltros filtro)
        {
            var response = new ErrorDto<ArticuloDataLista>
            {
                Result = new ArticuloDataLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                var joinProdUen = string.Empty;
                var parameters = new DynamicParameters();
                var where = BuildArticulosWhereClause(filtro, ref joinProdUen, parameters);

                var countSql = $@"
                    SELECT COUNT(P.COD_PRODUCTO) 
                    FROM pv_productos P
                    LEFT JOIN PV_PROD_CLASIFICA_SUB Cs 
                        ON Cs.COD_PRODCLAS = P.COD_PRODCLAS 
                       AND Cs.COD_LINEA_SUB = P.COD_LINEA_SUB
                    {joinProdUen}
                    {where}";
                response.Result.Total = connection.QueryFirstOrDefault<int>(countSql, parameters);

                var pagination = BuildPaginationClause(parameters, filtro.pagina, filtro.paginacion);

                var dataSql = $@"
                    SELECT 
                        CONCAT(
                            FORMAT(Cs.COD_PRODCLAS, ' 0'), 
                            FORMAT(ISNULL(Cs.NIVEL,' 00'), ' 0'), 
                            FORMAT(ISNULL(Cs.COD_LINEA_SUB_MADRE,1), ' 0'),
                            FORMAT(ISNULL(Cs.COD_LINEA_SUB,1), ' '),
                            P.COD_PRODUCTO
                        ) AS CODIGO,
                        P.COD_PRODUCTO, P.CABYS, P.DESCRIPCION, P.COD_BARRAS, P.EXISTENCIA, 
                        P.COSTO_REGULAR, P.PRECIO_REGULAR, P.IMPUESTO_VENTAS, P.COD_FABRICANTE, 
                        P.I_STOCK, P.TIPO_PRODUCTO AS Tipo, P.cod_unidad AS unidad
                    FROM pv_productos P
                    LEFT JOIN PV_PROD_CLASIFICA_SUB Cs 
                        ON Cs.COD_PRODCLAS = P.COD_PRODCLAS 
                       AND Cs.COD_LINEA_SUB = P.COD_LINEA_SUB
                    {joinProdUen}
                    {where}
                    ORDER BY P.COD_PRODUCTO
                    {pagination}";

                response.Result.Articulos = connection.Query<ArticuloData>(dataSql, parameters).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Total = 0;
                response.Result.Articulos = new List<ArticuloData>();
            }

            return response;
        }

        private static string BuildArticulosWhereClause(
            ArticuloDataFiltros filtro,
            ref string joinProdUen,
            DynamicParameters parameters)
        {
            var clauses = new List<string>();

            if (filtro.catalogo != 0)
            {
                clauses.Add("P.ESTADO = 'A'");
            }

            if (!string.IsNullOrEmpty(filtro.cod_unidad) && filtro.cod_unidad != "T")
            {
                joinProdUen = " LEFT JOIN CPR_PRODUCTOS_UENS PU ON PU.COD_PRODUCTO = P.COD_PRODUCTO ";
                clauses.Add("PU.COD_UNIDAD = @CodUnidad");
                parameters.Add("@CodUnidad", filtro.cod_unidad, DbType.String);
            }

            if (!string.IsNullOrWhiteSpace(filtro.filtro))
            {
                clauses.Add(@"
                (
                    P.DESCRIPCION LIKE @Filtro
                    OR CONCAT(
                        FORMAT(Cs.COD_PRODCLAS, ' 0'),
                        FORMAT(ISNULL(Cs.NIVEL,' 00'), ' 0'),
                        FORMAT(ISNULL(Cs.COD_LINEA_SUB_MADRE,1), ' 0'),
                        FORMAT(ISNULL(Cs.COD_LINEA_SUB,1), ' '),
                        P.COD_PRODUCTO
                    ) LIKE @Filtro
                    OR P.COD_BARRAS LIKE @Filtro
                )");
                parameters.Add(_filtroParam, $"%{filtro.filtro}%", DbType.String);
            }

            if (filtro.familia > 0)
            {
                clauses.Add("P.COD_PRODCLAS = @Familia");
                parameters.Add("@Familia", filtro.familia, DbType.Int32);
            }

            if (!string.IsNullOrEmpty(filtro.sublinea))
            {
                clauses.Add("P.COD_LINEA_SUB = @Sublinea");
                parameters.Add("@Sublinea", filtro.sublinea, DbType.String);
            }

            return clauses.Count > 0
                ? " WHERE " + string.Join(" AND ", clauses)
                : string.Empty;
        }

        #endregion

        #region Órdenes de compra

        public OrdenesDataLista Ordenes_Obtener(
            int CodCliente,
            int? pagina,
            int? paginacion,
            string? filtro,
            string? proveedor,
            string? familia)
        {
            var info = new OrdenesDataLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                const string countSql = "SELECT COUNT(cod_orden) FROM cpr_ordenes";
                info.Total = connection.QueryFirstOrDefault<int>(countSql);

                var parameters = new DynamicParameters();
                var where = BuildOrdenesFiltroWhereClause(filtro, proveedor, familia, null, parameters);

                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT * FROM (
                        SELECT 
                            RIGHT(REPLICATE('0', 10) + CAST(sp.CPR_ID AS VARCHAR), 10) AS cod_solicitud, 
                            O.cod_orden,
                            O.genera_user,
                            O.nota,
                            O.COD_PROVEEDOR + '-' + cp.DESCRIPCION AS proveedor,
                            STUFF((
                                SELECT DISTINCT ', ' + ppc2.DESCRIPCION
                                FROM CPR_ORDENES_DETALLE cod2
                                INNER JOIN PV_PRODUCTOS pp2 ON cod2.COD_PRODUCTO = pp2.COD_PRODUCTO
                                LEFT JOIN PV_PROD_CLASIFICA ppc2 ON ppc2.COD_PRODCLAS = pp2.COD_PRODCLAS
                                WHERE cod2.COD_ORDEN = O.COD_ORDEN
                                      AND ppc2.DESCRIPCION IS NOT NULL
                                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS familia
                        FROM cpr_ordenes O
                        LEFT JOIN CPR_SOLICITUD_PROV sp 
                            ON sp.ADJUDICA_ORDEN = O.COD_ORDEN 
                           AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
                        LEFT JOIN CXP_PROVEEDORES cp 
                            ON cp.COD_PROVEEDOR = O.COD_PROVEEDOR
                        GROUP BY 
                            sp.CPR_ID, O.cod_orden, O.genera_user, O.nota, O.COD_PROVEEDOR, cp.DESCRIPCION
                    ) T
                    {where}
                    ORDER BY cod_orden
                    {pagination}";

                info.Ordenes = connection.Query<OrdenData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Ordenes = new List<OrdenData>();
            }

            return info;
        }

        public OrdenesDataLista OrdenesFiltro_Obtener(
            int CodCliente,
            int? pagina,
            int? paginacion,
            string? filtro,
            string? proveedor,
            string? familia,
            string? subfamilia)
        {
            var info = new OrdenesDataLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                const string totalSql = @"
                    SELECT COUNT(O.cod_orden) 
                    FROM cpr_ordenes O
                    LEFT JOIN CPR_SOLICITUD_PROV sp 
                        ON sp.ADJUDICA_ORDEN = O.COD_ORDEN 
                       AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
                    WHERE O.Estado IN ('A') AND O.Proceso IN ('A','X')";
                info.Total = connection.QueryFirstOrDefault<int>(totalSql);

                var parameters = new DynamicParameters();
                var where = BuildOrdenesFiltroWhereClause(filtro, proveedor, familia, subfamilia, parameters);
                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT * FROM (  
                        SELECT 
                            RIGHT(REPLICATE('0', 10) + CAST(sp.CPR_ID AS VARCHAR), 10) AS cod_solicitud, 
                            O.cod_orden, 
                            O.genera_user,
                            O.nota, 
                            O.COD_PROVEEDOR + '-' + cp.DESCRIPCION AS proveedor,
                            STUFF((
                                SELECT DISTINCT ', ' + CAST(ppc2.COD_PRODCLAS AS VARCHAR)
                                FROM CPR_ORDENES_DETALLE cod2
                                INNER JOIN PV_PRODUCTOS pp2 ON cod2.COD_PRODUCTO = pp2.COD_PRODUCTO
                                LEFT JOIN PV_PROD_CLASIFICA ppc2 ON ppc2.COD_PRODCLAS = pp2.COD_PRODCLAS
                                WHERE cod2.COD_ORDEN = O.COD_ORDEN
                                      AND ppc2.DESCRIPCION IS NOT NULL
                                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS familia,
                            STUFF((
                                SELECT DISTINCT ', ' + CAST(pp2.COD_LINEA_SUB AS VARCHAR)
                                FROM CPR_ORDENES_DETALLE cod2
                                INNER JOIN PV_PRODUCTOS pp2 ON cod2.COD_PRODUCTO = pp2.COD_PRODUCTO
                                WHERE cod2.COD_ORDEN = O.COD_ORDEN
                                      AND pp2.DESCRIPCION IS NOT NULL
                                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS subfamilia
                        FROM cpr_ordenes O
                        LEFT JOIN CPR_SOLICITUD_PROV sp 
                            ON sp.ADJUDICA_ORDEN = O.COD_ORDEN 
                           AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
                        LEFT JOIN CXP_PROVEEDORES cp 
                            ON cp.COD_PROVEEDOR = O.COD_PROVEEDOR
                        WHERE 
                            O.Estado IN ('A') 
                            AND O.Proceso IN ('A', 'X')
                        GROUP BY 
                            sp.CPR_ID, O.cod_orden, O.genera_user, O.nota, O.COD_PROVEEDOR, cp.DESCRIPCION
                    ) T 
                    {where}
                    ORDER BY cod_orden
                    {pagination}";

                info.Ordenes = connection.Query<OrdenData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Ordenes = new List<OrdenData>();
            }

            return info;
        }

        private static string BuildOrdenesFiltroWhereClause(
            string? filtro,
            string? proveedor,
            string? familia,
            string? subfamilia,
            DynamicParameters parameters)
        {
            var clauses = new List<string>();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                clauses.Add(@"
                (
                    cod_orden   LIKE @Filtro
                    OR genera_user LIKE @Filtro
                    OR cod_solicitud LIKE @Filtro
                    OR nota LIKE @Filtro
                    OR familia LIKE @Filtro
                    OR proveedor LIKE @Filtro
                )");
                parameters.Add(_filtroParam, $"%{filtro}%", DbType.String);
            }

            proveedor = proveedor?.Replace("null", string.Empty).Trim();
            familia = familia?.Replace("null", string.Empty).Trim();
            subfamilia = subfamilia?.Replace("null", string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(proveedor))
            {
                clauses.Add("proveedor LIKE @Proveedor");
                parameters.Add("@Proveedor", $"%{proveedor}%", DbType.String);
            }

            if (!string.IsNullOrWhiteSpace(familia))
            {
                clauses.Add("familia LIKE @Familia");
                parameters.Add("@Familia", $"%{familia}%", DbType.String);
            }

            if (!string.IsNullOrWhiteSpace(subfamilia) && subfamilia != "5")
            {
                clauses.Add("subfamilia LIKE @Subfamilia");
                parameters.Add("@Subfamilia", $"%{subfamilia}%", DbType.String);
            }

            return clauses.Count > 0
                ? "WHERE " + string.Join(" AND ", clauses)
                : string.Empty;
        }

        #endregion

        #region Facturas por proveedor

        public FacturasDataLista ObtenerListaFacturas(
            int CodCliente,
            int CodProveedor,
            int? pagina,
            int? paginacion,
            string? filtro)
        {
            var info = new FacturasDataLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                var baseWhere = "E.cod_proveedor = @CodProveedor";
                var parameters = new DynamicParameters();
                parameters.Add("@CodProveedor", CodProveedor, DbType.Int32);

                var countSql = $@"
                    SELECT COUNT(cod_factura)  
                    FROM cpr_compras E 
                    INNER JOIN cxp_Proveedores P ON E.cod_proveedor = P.cod_proveedor 
                    WHERE {baseWhere}";
                info.Total = connection.QueryFirstOrDefault<int>(countSql, parameters);

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    baseWhere += " AND (E.cod_factura LIKE @Filtro OR P.descripcion LIKE @Filtro)";
                    parameters.Add(_filtroParam, $"%{filtro}%", DbType.String);
                }

                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT E.cod_factura,
                           P.descripcion AS Proveedor,
                           E.total
                    FROM cpr_compras E 
                    INNER JOIN cxp_Proveedores P ON E.cod_proveedor = P.cod_proveedor
                    WHERE {baseWhere}
                    ORDER BY E.cod_factura
                    {pagination}";

                info.Facturas = connection.Query<FacturasData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Facturas = new List<FacturasData>();
            }

            return info;
        }

        #endregion

        #region Usuarios

        public UsuarioDataLista Usuarios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var info = new UsuarioDataLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                const string countSql = "SELECT COUNT(*) FROM usuarios";
                info.Total = connection.QueryFirstOrDefault<int>(countSql);

                string where;
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    where = "(nombre LIKE @Filtro OR descripcion LIKE @Filtro) AND ESTADO = 'A'";
                    parameters.Add(_filtroParam, $"%{filtro}%", DbType.String);
                }
                else
                {
                    where = "ESTADO = 'A'";
                }

                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT nombre, descripcion 
                    FROM usuarios
                    WHERE {where}
                    ORDER BY nombre
                    {pagination}";

                info.Usuarios = connection.Query<UsuarioData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Usuarios = new List<UsuarioData>();
            }

            return info;
        }

        #endregion

        #region Facturas proveedor (filtros avanzados)

        public FacturasProveedorLista FacturaProveedor_Obtener(int CodCliente, string filtros)
        {
            var filtrosModel = JsonConvert.DeserializeObject<FacturasProveedorDataFiltros>(filtros) ??
                               new FacturasProveedorDataFiltros();

            var info = new FacturasProveedorLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                const string countSql = @"
                    SELECT COUNT(E.cod_compra) 
                    FROM cpr_Compras E 
                    INNER JOIN cxp_proveedores P ON E.cod_proveedor = P.cod_proveedor";
                info.Total = connection.QueryFirstOrDefault<int>(countSql);

                var clauses = new List<string>();
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(filtrosModel.filtro))
                {
                    clauses.Add(@"
                        (E.cod_compra LIKE @Filtro
                         OR E.cod_orden LIKE @Filtro
                         OR E.cod_factura LIKE @Filtro
                         OR P.descripcion LIKE @Filtro)");
                    parameters.Add(_filtroParam, $"%{filtrosModel.filtro}%", DbType.String);
                }

                if (filtrosModel.cod_proveedor > 0)
                {
                    clauses.Add("P.cod_proveedor = @CodProveedor");
                    parameters.Add("@CodProveedor", filtrosModel.cod_proveedor, DbType.Int32);
                }

                var where = clauses.Count > 0
                    ? "WHERE " + string.Join(" AND ", clauses)
                    : string.Empty;

                var pagination = BuildPaginationClause(parameters, filtrosModel.pagina, filtrosModel.paginacion);

                var dataSql = $@"
                    SELECT E.cod_compra,
                           E.cod_orden,
                           E.cod_factura,
                           P.descripcion AS Proveedor, 
                           P.cod_proveedor,
                           RIGHT(REPLICATE('0', 10) + CAST(s.CPR_ID AS VARCHAR), 10) AS no_solicitud
                    FROM cpr_Compras E 
                    INNER JOIN cxp_proveedores P ON E.cod_proveedor = P.cod_proveedor
                    LEFT JOIN CPR_SOLICITUD_PROV s 
                        ON s.ADJUDICA_ORDEN = E.COD_ORDEN 
                       AND s.PROVEEDOR_CODIGO = E.cod_proveedor
                    {where}
                    ORDER BY E.cod_compra
                    {pagination}";

                info.Facturas = connection.Query<FacturasProveedorData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Facturas = new List<FacturasProveedorData>();
            }

            return info;
        }

        #endregion

        #region Devoluciones de compra

        public CompraDevLista Devoluciones_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var info = new CompraDevLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                const string countSql = @"
                    SELECT COUNT(*) 
                    FROM cpr_compras_dev D 
                    INNER JOIN cxp_proveedores P ON D.cod_proveedor = P.cod_proveedor";
                info.Total = connection.QueryFirstOrDefault<int>(countSql);

                var parameters = new DynamicParameters();
                var where = string.Empty;

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    where = @"
                        WHERE  D.cod_compra_dev LIKE @Filtro
                               OR D.cod_factura LIKE @Filtro
                               OR P.descripcion LIKE @Filtro";
                    parameters.Add(_filtroParam, $"%{filtro}%", DbType.String);
                }

                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT D.cod_compra_dev,
                           P.descripcion AS Proveedor,
                           D.cod_factura,
                           D.notas,
                           D.fecha
                    FROM cpr_compras_dev D 
                    INNER JOIN cxp_proveedores P ON D.cod_proveedor = P.cod_proveedor
                    {where}
                    ORDER BY D.cod_compra_dev
                    {pagination}";

                info.devoluciones = connection.Query<CompraDevData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.devoluciones = new List<CompraDevData>();
            }

            return info;
        }

        #endregion

        #region Beneficios

        public BeneficioDataLista Beneficios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var info = new BeneficioDataLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                const string countSql = "SELECT COUNT(cod_beneficio) FROM afi_beneficios";
                info.Total = connection.QueryFirstOrDefault<int>(countSql);

                var parameters = new DynamicParameters();
                var where = string.Empty;

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    where = @"
                        WHERE cod_beneficio LIKE @Filtro
                           OR descripcion LIKE @Filtro";
                    parameters.Add(_filtroParam, $"%{filtro}%", DbType.String);
                }

                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT cod_beneficio, descripcion 
                    FROM afi_beneficios
                    {where}
                    ORDER BY cod_beneficio
                    {pagination}";

                info.Beneficios = connection.Query<BeneficioData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Beneficios = new List<BeneficioData>();
            }

            return info;
        }

        #endregion

        #region Socios (V1 Galileo)

        public ErrorDto<SociosDataLista> Socios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var response = new ErrorDto<SociosDataLista>
            {
                Result = new SociosDataLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                var parameters = new DynamicParameters();
                var where = string.Empty;

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    where = @"
                        WHERE  S.cedula   LIKE @Filtro
                            OR S.cedular  LIKE @Filtro
                            OR S.nombre   LIKE @Filtro
                            OR M.Membresia LIKE @Filtro";
                    parameters.Add(_filtroParam, $"%{filtro}%", DbType.String);
                }

                var countSql = $@"
                    SELECT COUNT(*) 
                    FROM SOCIOS S 
                    LEFT JOIN vAFI_Membresias M ON M.Cedula = S.CEDULA 
                    {where}";
                response.Result.Total = connection.QueryFirstOrDefault<int>(countSql, parameters);

                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT S.cedula, S.cedular, S.nombre, M.Membresia 
                    FROM SOCIOS S
                    LEFT JOIN vAFI_Membresias M ON M.Cedula = S.CEDULA
                    {where}
                    ORDER BY S.cedula
                    {pagination}";

                response.Result.socios = connection.Query<SociosData>(dataSql, parameters).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Total = 0;
                response.Result.socios = new List<SociosData>();
            }

            return response;
        }

        #endregion

        #region Socios (lazy load genérico con ordenamiento seguro)

        public ErrorDto<TablasListaGenericaModel> Socios_Obtener(int CodEmpresa, string jfiltro)
        {
            var filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltro) ??
                         new FiltrosLazyLoadData();

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var parameters = new DynamicParameters();
                var where = string.Empty;

                if (!string.IsNullOrWhiteSpace(filtro.filtro))
                {
                    where = @"
                        WHERE ( 
                            S.cedula   LIKE @Filtro
                         OR S.cedular  LIKE @Filtro
                         OR S.nombre   LIKE @Filtro
                         OR M.membresia LIKE @Filtro
                        )";
                    parameters.Add(_filtroParam, $"%{filtro.filtro}%", DbType.String);
                }

                // sortField como parámetro seguro
                var sortField = string.IsNullOrWhiteSpace(filtro.sortField)
                    ? "cedula"
                    : filtro.sortField;

                parameters.Add("@SortField", sortField, DbType.String);

                var countSql = $@"
                    SELECT COUNT(*) 
                    FROM SOCIOS S 
                    LEFT JOIN vAFI_Membresias M ON M.Cedula = S.CEDULA 
                    {where}";
                response.Result.total = connection.QueryFirstOrDefault<int>(countSql, parameters);

                var pagination = BuildPaginationClause(parameters, filtro.pagina, filtro.paginacion);

                var orderDirection = filtro.sortOrder == 0 ? "DESC" : "ASC";

                var dataSql = $@"
                    SELECT S.cedula, S.cedular, S.nombre, M.Membresia 
                    FROM SOCIOS S
                    LEFT JOIN vAFI_Membresias M ON M.Cedula = S.CEDULA
                    {where}
                    ORDER BY 
                        CASE WHEN @SortField = 'cedula'    THEN S.cedula    END {orderDirection},
                        CASE WHEN @SortField = 'cedular'   THEN S.cedular   END {orderDirection},
                        CASE WHEN @SortField = 'nombre'    THEN S.nombre    END {orderDirection},
                        CASE WHEN @SortField = 'Membresia' THEN M.Membresia END {orderDirection}
                    {pagination}";

                response.Result.lista = connection.Query<SociosData>(dataSql, parameters).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = new List<SociosData>();
            }

            return response;
        }

        #endregion

        #region Beneficio productos

        public BeneficioProductoLista BeneficioProducto_Obtener(
            int CodCliente,
            int? pagina,
            int? paginacion,
            string? filtro)
        {
            var info = new BeneficioProductoLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                const string countSql = "SELECT COUNT(cod_producto) FROM afi_bene_productos";
                info.Total = connection.QueryFirstOrDefault<int>(countSql);

                var parameters = new DynamicParameters();
                var where = string.Empty;

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    where = @"
                        WHERE cod_producto LIKE @Filtro
                           OR descripcion LIKE @Filtro";
                    parameters.Add(_filtroParam, $"%{filtro}%", DbType.String);
                }

                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT cod_producto, descripcion, COSTO_UNIDAD 
                    FROM afi_bene_productos
                    {where}
                    ORDER BY cod_producto
                    {pagination}";

                info.productos = connection.Query<BeneficioProductoData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.productos = new List<BeneficioProductoData>();
            }

            return info;
        }

        #endregion

        #region Departamentos

        public DepartamentoDataLista Departamentos_Obtener(
            int CodCliente,
            string Institucion,
            int? pagina,
            int? paginacion,
            string? filtro)
        {
            var info = new DepartamentoDataLista();

            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                var parameters = new DynamicParameters();
                parameters.Add("@Institucion", Institucion, DbType.String);

                const string countSql = @"
                    SELECT COUNT(cod_departamento) 
                    FROM AFDepartamentos 
                    WHERE cod_institucion = @Institucion";
                info.Total = connection.QueryFirstOrDefault<int>(countSql, parameters);

                var whereExtra = string.Empty;

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    whereExtra = @"
                        AND (cod_departamento LIKE @Filtro
                             OR descripcion LIKE @Filtro)";
                    parameters.Add(_filtroParam, $"%{filtro}%", DbType.String);
                }

                var pagination = BuildPaginationClause(parameters, pagina, paginacion);

                var dataSql = $@"
                    SELECT cod_departamento, descripcion 
                    FROM AFDepartamentos 
                    WHERE cod_institucion = @Institucion
                    {whereExtra}
                    ORDER BY cod_departamento
                    {pagination}";

                info.departamentos = connection.Query<DepartamentoData>(dataSql, parameters).ToList();
            }
            catch
            {
                info.Total = 0;
                info.departamentos = new List<DepartamentoData>();
            }

            return info;
        }

        #endregion

        #region Catálogo AFI

        public List<CatalogosLista> Catalogo_Obtener(int CodCliente, int tipo, int modulo)
        {
            try
            {
                using var connection = _portalDB.CreateConnection(CodCliente);

                const string proc = "[spAFI_Bene_Catalogos_Consulta]";
                var values = new
                {
                    tipo,
                    Codigo = modulo
                };

                return connection.Query<CatalogosLista>(proc, values, commandType: CommandType.StoredProcedure).ToList();
            }
            catch
            {
                return new List<CatalogosLista>();
            }
        }

        #endregion

        #region UENs

        public ErrorDto<List<CatalogosLista>> UENS_Obtener(int CodEmpresa)
        {
            var response = new ErrorDto<List<CatalogosLista>> { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"SELECT COD_UNIDAD AS item, DESCRIPCION FROM CORE_UENS";
                response.Result = connection.Query<CatalogosLista>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        #endregion

        #region Listas para filtros de órdenes

        public ErrorDto<List<DropDownListaGenericaModel>> CompraOrdenProveedoresLista_Obtener(int CodEmpresa)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT DISTINCT
                        O.COD_PROVEEDOR AS item,
                        cp.DESCRIPCION AS descripcion
                    FROM cpr_ordenes O
                    LEFT JOIN CPR_SOLICITUD_PROV sp 
                        ON sp.ADJUDICA_ORDEN = O.COD_ORDEN 
                       AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
                    LEFT JOIN CXP_PROVEEDORES cp 
                        ON cp.COD_PROVEEDOR = O.COD_PROVEEDOR
                    WHERE 
                        O.Estado IN ('A') 
                        AND O.Proceso IN ('A', 'X')
                    GROUP BY 
                        sp.CPR_ID, O.cod_orden, O.genera_user, O.nota, O.COD_PROVEEDOR, cp.DESCRIPCION";

                resp.Result = connection.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = new List<DropDownListaGenericaModel>();
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CompraOrdenFamiliaLista_Obtener(int CodEmpresa)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT DISTINCT
                        STUFF((
                            SELECT DISTINCT ', ' + CAST(ppc2.COD_PRODCLAS AS VARCHAR)
                            FROM CPR_ORDENES_DETALLE cod2
                            INNER JOIN PV_PRODUCTOS pp2 ON cod2.COD_PRODUCTO = pp2.COD_PRODUCTO
                            LEFT JOIN PV_PROD_CLASIFICA ppc2 ON ppc2.COD_PRODCLAS = pp2.COD_PRODCLAS
                            WHERE cod2.COD_ORDEN = O.COD_ORDEN
                                  AND ppc2.DESCRIPCION IS NOT NULL
                            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS item,
                        STUFF((
                            SELECT DISTINCT ', ' + ppc2.DESCRIPCION
                            FROM CPR_ORDENES_DETALLE cod2
                            INNER JOIN PV_PRODUCTOS pp2 ON cod2.COD_PRODUCTO = pp2.COD_PRODUCTO
                            LEFT JOIN PV_PROD_CLASIFICA ppc2 ON ppc2.COD_PRODCLAS = pp2.COD_PRODCLAS
                            WHERE cod2.COD_ORDEN = O.COD_ORDEN
                                  AND ppc2.DESCRIPCION IS NOT NULL
                            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS descripcion        
                    FROM cpr_ordenes O
                    LEFT JOIN CPR_SOLICITUD_PROV sp 
                        ON sp.ADJUDICA_ORDEN = O.COD_ORDEN 
                       AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
                    LEFT JOIN CXP_PROVEEDORES cp 
                        ON cp.COD_PROVEEDOR = O.COD_PROVEEDOR
                    WHERE 
                        O.Estado IN ('A') 
                        AND O.Proceso IN ('A', 'X')
                    GROUP BY 
                        sp.CPR_ID, O.cod_orden, O.genera_user, O.nota, O.COD_PROVEEDOR, cp.DESCRIPCION";

                resp.Result = connection.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = new List<DropDownListaGenericaModel>();
            }

            return resp;
        }

        #endregion

        #region TipoProducto sub-gradas

        public ErrorDto<List<TipoProductoSubGradaData>> TipoProductoSub_ObtenerTodos(int CodEmpresa, string Cod_Prodclas)
        {
            var response = new ErrorDto<List<TipoProductoSubGradaData>>
            {
                Code = 0,
                Result = new List<TipoProductoSubGradaData>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var cods = (Cod_Prodclas ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToArray();

                if (cods.Length == 0)
                {
                    return response;
                }

                const string sql = @"
                    SELECT Cod_Prodclas, Cod_Linea_Sub, Descripcion, Activo, Cabys, 
                           COD_CUENTA, NIVEL, COD_LINEA_SUB_MADRE
                    FROM PV_PROD_CLASIFICA_SUB 
                    WHERE Cod_Prodclas IN @Cods";

                var info = connection.Query<TipoProductoSubDto>(sql, new { Cods = cods }).ToList();

                foreach (var dt in info.Where(x => x.Nivel == 1))
                {
                    dt.Estado = dt.Activo ? "ACTIVO" : "INACTIVO";

                    response.Result.Add(new TipoProductoSubGradaData
                    {
                        key = dt.Cod_Linea_Sub,
                        icon = string.Empty,
                        label = dt.Descripcion,
                        data = dt,
                        children = TipoProductoSub_SeguienteNivel(CodEmpresa, dt)
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<TipoProductoSubGradaData>();
            }

            return response;
        }

        public List<TipoProductoSubGradaData> TipoProductoSub_SeguienteNivel(int CodEmpresa, TipoProductoSubDto padre)
        {
            var response = new List<TipoProductoSubGradaData>();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT Cod_Prodclas, Cod_Linea_Sub, Descripcion, Activo, Cabys,
                           COD_CUENTA, NIVEL, COD_LINEA_SUB_MADRE
                    FROM PV_PROD_CLASIFICA_SUB 
                    WHERE Cod_Prodclas = @CodProdClas
                      AND COD_LINEA_SUB_MADRE = @CodLineaSubMadre";

                var info = connection.Query<TipoProductoSubDto>(
                    sql,
                    new
                    {
                        CodProdClas = padre.Cod_Prodclas,
                        CodLineaSubMadre = padre.Cod_Linea_Sub
                    }).ToList();

                foreach (var dt in info)
                {
                    dt.Estado = dt.Activo ? "ACTIVO" : "INACTIVO";

                    response.Add(new TipoProductoSubGradaData
                    {
                        key = dt.Cod_Linea_Sub,
                        icon = string.Empty,
                        label = dt.Descripcion,
                        data = dt,
                        children = TipoProductoSub_SeguienteNivel(CodEmpresa, dt)
                    });
                }
            }
            catch
            {
                response = new List<TipoProductoSubGradaData>();
            }

            return response;
        }

        #endregion

        #region Personas (lazy)

        public ErrorDto<TablasListaGenericaModel> Personas_Obtener(int CodEmpresa, string jfiltro)
        {
            var filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltro) ??
                         new FiltrosLazyLoadData();

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string countSql = "SELECT COUNT(*) FROM socios";
                response.Result.total = connection.QueryFirstOrDefault<int>(countSql);

                var parameters = new DynamicParameters();
                var where = string.Empty;

                if (!string.IsNullOrWhiteSpace(filtro.filtro))
                {
                    where = @"
                        WHERE ( 
                            cedula  LIKE @Filtro
                         OR cedulaR LIKE @Filtro
                         OR nombre LIKE @Filtro
                        )";
                    parameters.Add(_filtroParam, $"%{filtro.filtro}%", DbType.String);
                }

                var sortField = string.IsNullOrWhiteSpace(filtro.sortField)
                    ? "cedula"
                    : filtro.sortField;

                parameters.Add("@SortField", sortField, DbType.String);

                var pagination = BuildPaginationClause(parameters, filtro.pagina, filtro.paginacion);
                var orderDirection = filtro.sortOrder == 0 ? "DESC" : "ASC";

                var dataSql = $@"
                    SELECT cedula, cedulaR, nombre 
                    FROM socios
                    {where}
                    ORDER BY 
                        CASE WHEN @SortField = 'cedula'  THEN cedula  END {orderDirection},
                        CASE WHEN @SortField = 'cedulaR' THEN cedulaR END {orderDirection},
                        CASE WHEN @SortField = 'nombre'  THEN nombre  END {orderDirection}
                    {pagination}";

                response.Result.lista = connection.Query<AFCedulaDto>(dataSql, parameters).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new TablasListaGenericaModel();
            }

            return response;
        }

        #endregion
    }
}