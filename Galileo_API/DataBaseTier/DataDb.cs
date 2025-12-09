using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class DataDB
    {
        private readonly IConfiguration _config;
        const string AndOperator = " AND ";
        const string Offset = " OFFSET ";
        const string FetchNext = " FETCH NEXT ";
        const string RowsOnly = " ROWS ONLY ";

        // Literales reutilizados (regla S1192)
        const string Where = " WHERE ";
        const string CountAll = "SELECT COUNT(*)";

        const string OrderByCodProveedor = " ORDER BY COD_PROVEEDOR";
        const string OrderByCodCargo = " ORDER BY COD_CARGO";
        const string OrderByCodBodega = " ORDER BY cod_bodega";
        const string OrderByProdCodigo = " ORDER BY P.COD_PRODUCTO";
        const string OrderByCodOrden = " ORDER BY cod_orden";
        const string OrderByCodFacturaOrden = " ORDER BY cod_orden";
        const string OrderByCodCompra = " ORDER BY E.cod_compra";
        const string OrderByCodBeneficio = " ORDER BY cod_beneficio";
        const string OrderByNombre = " ORDER BY nombre";
        const string OrderBySocioCedula = " ORDER BY S.cedula";
        const string OrderByCodProducto = " ORDER BY cod_producto";
        const string OrderByCodDepartamento = " ORDER BY cod_departamento";
        const string OrderByCompraDev = " ORDER BY D.cod_compra_dev";

        const string OffsetFetchPage = " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        const string OrderByDynamicTemplate = " ORDER BY {0} {1}";

        const string ParamFiltro = "@Filtro";
        const string ParamOffset = "@Offset";
        const string ParamPageSize = "@PageSize";
        const string CedulaColumn = "cedula";

        public DataDB(IConfiguration config)
        {
            _config = config;
        }

        #region Proveedores

        /// <summary>
        /// Método para obtener los proveedores
        /// </summary>
        public ErrorDto<ProveedoresDataLista> Proveedores_Obtener(int CodCliente, ProveedorDataFiltros jFiltros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<ProveedoresDataLista>
            {
                Result = new ProveedoresDataLista { Total = 0 }
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                var condiciones = new List<string>();
                var parametros = new DynamicParameters();

                // Estado base
                condiciones.Add("(ESTADO = 'A' OR ESTADO = 'T')");

                // autoGestion / ventas
                if (jFiltros.autoGestion == true && jFiltros.ventas == true)
                {
                    condiciones.Add("(WEB_AUTO_GESTION = 1 OR WEB_FERIAS = 1)");
                }
                else if (jFiltros.autoGestion == true)
                {
                    condiciones.Add("WEB_AUTO_GESTION = 1");
                }
                else if (jFiltros.ventas == true)
                {
                    condiciones.Add("WEB_FERIAS = 1");
                }

                // filtro de búsqueda
                if (!string.IsNullOrWhiteSpace(jFiltros.filtro))
                {
                    condiciones.Add("(COD_PROVEEDOR LIKE @Filtro OR DESCRIPCION LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{jFiltros.filtro}%");
                }

                string whereClause = string.Empty;

                if (condiciones.Any(c => !string.IsNullOrWhiteSpace(c)))
                {
                    whereClause = " WHERE " + string.Join(AndOperator, condiciones.Where(c => !string.IsNullOrWhiteSpace(c)));
                }

                // total
                var sqlCount = CountAll + " FROM CXP_PROVEEDORES" + whereClause;
                response.Result.Total = connection.QuerySingle<int>(sqlCount, parametros);

                // paginación
                var orderAndPage = new StringBuilder(OrderByCodProveedor);
                if (jFiltros.pagina != null && jFiltros.paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, jFiltros.pagina.Value);
                    parametros.Add(ParamPageSize, jFiltros.paginacion.Value);
                }

                var sqlData = @"SELECT COD_PROVEEDOR, DESCRIPCION, CEDJUR 
                                FROM CXP_PROVEEDORES"
                              + whereClause
                              + orderAndPage.ToString();

                response.Result.Proveedores = connection.Query<ProveedorData>(sqlData, parametros).ToList();
            }
            catch (Exception ex)
            {
                response.Result.Total = 0;
                response.Description = ex.Message;
                response.Code = -1;
            }

            return response;
        }

        #endregion

        #region Cargos

        /// <summary>
        /// Método para obtener los cargos
        /// </summary>
        public CargoDataLista Cargos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new CargoDataLista { Total = 0 };

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                var condiciones = new List<string> { "ACTIVO = 1" };
                var parametros = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(COD_CARGO LIKE @Filtro OR DESCRIPCION LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtro}%");
                }

                var whereClause = Where + string.Join(AndOperator, condiciones);

                var sqlCount = "SELECT COUNT(COD_CARGO) FROM CXP_CARGOS" + whereClause;
                info.Total = connection.QuerySingle<int>(sqlCount, parametros);

                var orderAndPage = new StringBuilder(OrderByCodCargo);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var sqlData = @"SELECT COD_CARGO, DESCRIPCION, 0 as MONTO 
                                FROM CXP_CARGOS"
                              + whereClause
                              + orderAndPage.ToString();

                info.Cargos = connection.Query<CargoData>(sqlData, parametros).ToList();
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

        /// <summary>
        /// Método para obtener bodegas
        /// </summary>
        public BodegaDataLista Bodegas_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new BodegaDataLista { Total = 0 };

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                var condiciones = new List<string> { "permite_salidas = 1" };
                var parametros = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(cod_bodega LIKE @Filtro OR descripcion LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtro}%");
                }

                var whereClause = Where + string.Join(AndOperator, condiciones);

                var sqlCount = "SELECT COUNT(cod_bodega) FROM pv_bodegas" + whereClause;
                info.Total = connection.QuerySingle<int>(sqlCount, parametros);

                var orderAndPage = new StringBuilder(OrderByCodBodega);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var sqlData = @"SELECT cod_bodega, descripcion 
                                FROM pv_bodegas"
                              + whereClause
                              + orderAndPage.ToString();

                info.bodegas = connection.Query<BodegaData>(sqlData, parametros).ToList();
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

        /// <summary>
        /// Método para obtener los artículos
        /// </summary>
        public ErrorDto<ArticuloDataLista> Articulos_Obtener(int CodCliente, ArticuloDataFiltros filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<ArticuloDataLista>
            {
                Code = 0,
                Result = new ArticuloDataLista { Total = 0 }
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                string joinProdUen = "";
                var parametros = new DynamicParameters();
                string whereEstado = BuildArticulosWhereClause(filtro, ref joinProdUen, parametros);

                var sqlCount = $@"SELECT COUNT(P.COD_PRODUCTO) 
                                  FROM pv_productos P
                                  LEFT JOIN PV_PROD_CLASIFICA_SUB Cs 
                                    ON Cs.COD_PRODCLAS = P.COD_PRODCLAS 
                                   AND Cs.COD_LINEA_SUB = P.COD_LINEA_SUB
                                  {joinProdUen}
                                  {whereEstado}";
                response.Result.Total = connection.QuerySingle<int>(sqlCount, parametros);

                var orderAndPage = new StringBuilder(OrderByProdCodigo);
                if (filtro.pagina != null && filtro.paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, filtro.pagina.Value);
                    parametros.Add(ParamPageSize, filtro.paginacion.Value);
                }

                var sqlData = $@"
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
                    {whereEstado}
                    {orderAndPage}";

                response.Result.Articulos = connection.Query<ArticuloData>(sqlData, parametros).ToList();
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

        private static string BuildArticulosWhereClause(ArticuloDataFiltros filtro, ref string joinProdUen, DynamicParameters parametros)
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
                parametros.Add("@CodUnidad", filtro.cod_unidad);
            }

            if (!string.IsNullOrWhiteSpace(filtro.filtro))
            {
                clauses.Add(
                    "( P.DESCRIPCION LIKE @Filtro " +
                    "OR CONCAT( FORMAT(Cs.COD_PRODCLAS, ' 0') , " +
                    "FORMAT(ISNULL(Cs.NIVEL,' 00'), ' 0'), " +
                    "FORMAT(ISNULL(Cs.COD_LINEA_SUB_MADRE,1) , ' 0'), " +
                    "FORMAT(ISNULL(Cs.COD_LINEA_SUB,1) , ' ') " +
                    ",P.COD_PRODUCTO ) LIKE @Filtro " +
                    "OR P.COD_BARRAS LIKE @Filtro )"
                );
                parametros.Add(ParamFiltro, $"%{filtro.filtro}%");
            }

            if (filtro.familia > 0)
            {
                clauses.Add("P.COD_PRODCLAS = @Familia");
                parametros.Add("@Familia", filtro.familia);
            }

            if (!string.IsNullOrEmpty(filtro.sublinea))
            {
                clauses.Add("P.COD_LINEA_SUB = @Sublinea");
                parametros.Add("@Sublinea", filtro.sublinea);
            }

            if (clauses.Count > 0)
            {
                return Where + string.Join(AndOperator, clauses);
            }
            return string.Empty;
        }

        #endregion

        #region Ordenes

        /// <summary>
        /// Método para obtener las ordenes de compra
        /// </summary>
        public OrdenesDataLista Ordenes_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string? proveedor, string? familia)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new OrdenesDataLista { Total = 0 };
            try
            {
                using var connection = new SqlConnection(clienteConnString);

                // Total sin filtros (mismo comportamiento original)
                var sqlCount = "SELECT COUNT(cod_orden) FROM cpr_ordenes";
                info.Total = connection.QuerySingle<int>(sqlCount);

                var parametros = new DynamicParameters();
                var condiciones = new List<string>();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(cod_orden LIKE @Filtro OR genera_user LIKE @Filtro OR cod_solicitud LIKE @Filtro OR nota LIKE @Filtro OR familia LIKE @Filtro OR proveedor LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtro}%");
                }

                proveedor = string.IsNullOrWhiteSpace(proveedor) ? null : proveedor.Replace("null", "").Trim();
                familia = string.IsNullOrWhiteSpace(familia) ? null : familia.Replace("null", "").Trim();

                if (!string.IsNullOrWhiteSpace(proveedor))
                {
                    condiciones.Add("proveedor LIKE @Proveedor");
                    parametros.Add("@Proveedor", $"%{proveedor}%");
                }

                if (!string.IsNullOrWhiteSpace(familia))
                {
                    condiciones.Add("familia LIKE @Familia");
                    parametros.Add("@Familia", $"%{familia}%");
                }

                var whereClause = condiciones.Count > 0
                    ? Where + string.Join(AndOperator, condiciones)
                    : string.Empty;

                var orderAndPage = new StringBuilder(OrderByCodOrden);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var query = $@"
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
                            ON sp.ADJUDICA_ORDEN  = O.COD_ORDEN 
                           AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
                        LEFT JOIN CXP_PROVEEDORES cp 
                            ON cp.COD_PROVEEDOR = O.COD_PROVEEDOR
                        GROUP BY 
                            sp.CPR_ID, O.cod_orden, O.genera_user, O.nota, O.COD_PROVEEDOR, cp.DESCRIPCION
                    ) T
                    {whereClause}
                    {orderAndPage}";

                info.Ordenes = connection.Query<OrdenData>(query, parametros).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Ordenes = new List<OrdenData>();
            }
            return info;
        }

        /// <summary>
        /// Método para obtener las ordenes de compra por filtro
        /// </summary>
        public OrdenesDataLista OrdenesFiltro_Obtener(int CodCliente, int? pagina, int? paginacion,
            string? filtro, string? proveedor, string? familia, string? subfamilia)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new OrdenesDataLista { Total = 0 };
            try
            {
                using var connection = new SqlConnection(clienteConnString);

                // Total base (sin filtros adicionales, mantiene lógica original)
                var totalQuery = @"SELECT COUNT(O.cod_orden) 
                                   FROM cpr_ordenes O                       
                                   LEFT JOIN CPR_SOLICITUD_PROV sp 
                                     ON sp.ADJUDICA_ORDEN  = O.COD_ORDEN 
                                    AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
                                   WHERE O.Estado in('A') and O.Proceso in('A','X')";
                info.Total = connection.QuerySingle<int>(totalQuery);

                var parametros = new DynamicParameters();
                var whereClause = BuildOrdenesFiltroWhereClause(filtro, proveedor, familia, subfamilia, parametros);

                var orderAndPage = new StringBuilder(OrderByCodOrden);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var query = $@"
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
                    {whereClause}
                    {orderAndPage}";

                info.Ordenes = connection.Query<OrdenData>(query, parametros).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Ordenes = new List<OrdenData>();
            }
            return info;
        }

        private static string BuildOrdenesFiltroWhereClause(string? filtro, string? proveedor, string? familia, string? subfamilia, DynamicParameters parametros)
        {
            var clauses = new List<string>();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                clauses.Add("(cod_orden LIKE @Filtro OR genera_user LIKE @Filtro OR cod_solicitud LIKE @Filtro OR nota LIKE @Filtro OR familia LIKE @Filtro OR proveedor LIKE @Filtro)");
                parametros.Add(ParamFiltro, $"%{filtro}%");
            }

            proveedor = string.IsNullOrWhiteSpace(proveedor) ? null : proveedor.Replace("null", "").Trim();
            familia = string.IsNullOrWhiteSpace(familia) ? null : familia.Replace("null", "").Trim();
            subfamilia = string.IsNullOrWhiteSpace(subfamilia) ? null : subfamilia.Replace("null", "").Trim();

            if (!string.IsNullOrWhiteSpace(proveedor))
            {
                clauses.Add("proveedor LIKE @Proveedor");
                parametros.Add("@Proveedor", $"%{proveedor}%");
            }

            if (!string.IsNullOrWhiteSpace(familia))
            {
                clauses.Add("familia LIKE @Familia");
                parametros.Add("@Familia", $"%{familia}%");
            }

            if (!string.IsNullOrWhiteSpace(subfamilia))
            {
                if (subfamilia == "5")
                {
                    // Sin filtro real
                }
                else
                {
                    clauses.Add("subfamilia LIKE @Subfamilia");
                    parametros.Add("@Subfamilia", $"%{subfamilia}%");
                }
            }

            if (clauses.Count > 0)
            {
                return "WHERE " + string.Join(AndOperator, clauses);
            }

            return string.Empty;
        }

        #endregion

        #region Facturas proveedor

        /// <summary>
        /// Método para obtener las facturas
        /// </summary>
        public FacturasDataLista ObtenerListaFacturas(int CodCliente, int CodProveedor, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var info = new FacturasDataLista { Total = 0 };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var parametros = new DynamicParameters();
                parametros.Add("@CodProveedor", CodProveedor);

                var baseFrom = @" FROM cpr_compras E 
                                  INNER JOIN cxp_Proveedores P 
                                    ON E.cod_proveedor = P.cod_proveedor 
                                   AND E.cod_proveedor = @CodProveedor";

                // Total
                var sqlCount = "SELECT COUNT(cod_factura)" + baseFrom;
                info.Total = connection.QuerySingle<int>(sqlCount, parametros);

                // Filtro
                var condiciones = new List<string>();
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(E.cod_factura LIKE @Filtro OR P.descripcion LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtro}%");
                }

                var whereClause = condiciones.Count > 0
                    ? Where + string.Join(AndOperator, condiciones)
                    : string.Empty;

                var orderAndPage = new StringBuilder(OrderByCodFacturaOrden);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var sqlData = @"SELECT E.cod_factura,
                                       P.descripcion as Proveedor,
                                       E.total"
                              + baseFrom
                              + whereClause
                              + orderAndPage;

                info.Facturas = connection.Query<FacturasData>(sqlData, parametros).ToList();
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

        /// <summary>
        /// Método para obtener los usuarios
        /// </summary>
        public UsuarioDataLista Usuarios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new UsuarioDataLista { Total = 0 };
            try
            {
                using var connection = new SqlConnection(clienteConnString);

                var parametros = new DynamicParameters();
                var condiciones = new List<string> { "ESTADO = 'A'" };

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(nombre LIKE @Filtro OR descripcion LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtro}%");
                }

                var whereClause = Where + string.Join(AndOperator, condiciones);

                var sqlCount = CountAll + " FROM usuarios" + whereClause;
                info.Total = connection.QuerySingle<int>(sqlCount, parametros);

                var orderAndPage = new StringBuilder(OrderByNombre);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var sqlData = @"SELECT nombre, descripcion 
                                FROM usuarios"
                              + whereClause
                              + orderAndPage;

                info.Usuarios = connection.Query<UsuarioData>(sqlData, parametros).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Usuarios = new List<UsuarioData>();
            }
            return info;
        }

        #endregion

        #region Facturas proveedor (lista avanzada)

        /// <summary>
        /// Método para obtener las facturas de proveedores
        /// </summary>
        public FacturasProveedorLista FacturaProveedor_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var filtrosModel = JsonConvert.DeserializeObject<FacturasProveedorDataFiltros>(filtros) ?? new FacturasProveedorDataFiltros();
            var info = new FacturasProveedorLista { Total = 0 };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var parametros = new DynamicParameters();

                var fromBase = @" FROM cpr_Compras E 
                                  INNER JOIN cxp_proveedores P 
                                    ON E.cod_proveedor = P.cod_proveedor
                                  LEFT JOIN CPR_SOLICITUD_PROV s 
                                    ON s.ADJUDICA_ORDEN  = E.COD_ORDEN 
                                   AND s.PROVEEDOR_CODIGO = E.cod_proveedor";

                // Total sin filtros adicionales
                var sqlCount = "SELECT COUNT(E.cod_compra) FROM cpr_Compras E INNER JOIN cxp_proveedores P ON E.cod_proveedor = P.cod_proveedor";
                info.Total = connection.QuerySingle<int>(sqlCount);

                var condiciones = new List<string>();

                if (!string.IsNullOrWhiteSpace(filtrosModel.filtro))
                {
                    condiciones.Add("(E.cod_compra LIKE @Filtro OR E.cod_orden LIKE @Filtro OR E.cod_factura LIKE @Filtro OR P.descripcion LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtrosModel.filtro}%");
                }

                if (filtrosModel.cod_proveedor > 0)
                {
                    condiciones.Add("P.cod_proveedor = @CodProveedor");
                    parametros.Add("@CodProveedor", filtrosModel.cod_proveedor);
                }

                var whereClause = condiciones.Count > 0
                    ? Where + string.Join(AndOperator, condiciones)
                    : string.Empty;

                var orderAndPage = new StringBuilder(OrderByCodCompra);
                if (filtrosModel.pagina == 0) // se mantiene la condición original
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, filtrosModel.pagina);
                    parametros.Add(ParamPageSize, filtrosModel.paginacion);
                }

                var sqlData = @"SELECT E.cod_compra,
                                       E.cod_orden,
                                       E.cod_factura, 
                                       P.descripcion as Proveedor, 
                                       P.cod_proveedor, 
                                       RIGHT(REPLICATE('0', 10) + CAST(s.CPR_ID AS VARCHAR), 10) AS no_solicitud"
                              + fromBase
                              + whereClause
                              + orderAndPage;

                info.Facturas = connection.Query<FacturasProveedorData>(sqlData, parametros).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Facturas = new List<FacturasProveedorData>();
            }
            return info;
        }

        #endregion

        #region Devoluciones

        /// <summary>
        /// Método para obtener las devoluciones de compra
        /// </summary>
        public CompraDevLista Devoluciones_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new CompraDevLista { Total = 0 };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var parametros = new DynamicParameters();

                var baseFrom = @" FROM cpr_compras_dev D 
                                  INNER JOIN cxp_proveedores P 
                                    ON D.cod_proveedor = P.cod_proveedor";

                // Total
                var sqlCount = CountAll + baseFrom;
                info.Total = connection.QuerySingle<int>(sqlCount, parametros);

                var condiciones = new List<string>();
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(D.cod_compra_dev LIKE @Filtro OR D.cod_factura LIKE @Filtro OR P.descripcion LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtro}%");
                }

                var whereClause = condiciones.Count > 0
                    ? Where + string.Join(AndOperator, condiciones)
                    : string.Empty;

                var orderAndPage = new StringBuilder(OrderByCompraDev);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var sqlData = @"SELECT D.cod_compra_dev,
                                       P.descripcion as Proveedor,
                                       D.cod_factura,
                                       D.notas,
                                       D.fecha"
                              + baseFrom
                              + whereClause
                              + orderAndPage;

                info.devoluciones = connection.Query<CompraDevData>(sqlData, parametros).ToList();
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

        /// <summary>
        /// Método para obtener los beneficios
        /// </summary>
        public BeneficioDataLista Beneficios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new BeneficioDataLista { Total = 0 };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var parametros = new DynamicParameters();

                var baseFrom = " FROM afi_beneficios";

                var sqlCount = "SELECT COUNT(cod_beneficio)" + baseFrom;
                info.Total = connection.QuerySingle<int>(sqlCount);

                var condiciones = new List<string>();
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(cod_beneficio LIKE @Filtro OR descripcion LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtro}%");
                }

                var whereClause = condiciones.Count > 0
                    ? Where + string.Join(AndOperator, condiciones)
                    : string.Empty;

                var orderAndPage = new StringBuilder(OrderByCodBeneficio);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var sqlData = @"SELECT cod_beneficio, descripcion"
                              + baseFrom
                              + whereClause
                              + orderAndPage;

                info.Beneficios = connection.Query<BeneficioData>(sqlData, parametros).ToList();
            }
            catch
            {
                info.Total = 0;
                info.Beneficios = new List<BeneficioData>();
            }
            return info;
        }

        #endregion

        #region Socios V1

        /// <summary>
        /// Método para obtener los socios (V1 Galileo)
        /// </summary>
        public ErrorDto<SociosDataLista> Socios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<SociosDataLista>
            {
                Result = new SociosDataLista { Total = 0 }
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var parametros = new DynamicParameters();

                var fromBase = @" FROM SOCIOS S 
                                  LEFT JOIN vAFI_Membresias M ON M.Cedula = S.CEDULA";

                var condiciones = new List<string>();
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add(@"( S.cedula LIKE @Filtro 
                                     OR S.cedular LIKE @Filtro
                                     OR S.nombre LIKE @Filtro
                                     OR M.Membresia LIKE @Filtro )");
                    parametros.Add(ParamFiltro, $"%{filtro}%");
                }

                var whereClause = condiciones.Count > 0
                    ? Where + string.Join(AndOperator, condiciones)
                    : string.Empty;

                var sqlCount = CountAll + fromBase + whereClause;
                response.Result.Total = connection.QuerySingle<int>(sqlCount, parametros);

                var orderAndPage = new StringBuilder(OrderBySocioCedula);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var sqlData = @"SELECT S.cedula, S.cedular, S.nombre, M.Membresia"
                              + fromBase
                              + whereClause
                              + orderAndPage;

                response.Result.socios = connection.Query<SociosData>(sqlData, parametros).ToList();
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

        #region Socios LazyLoad

        /// <summary>
        /// Método para obtener los socios (Lazy load)
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> Socios_Obtener(int CodEmpresa, string jfiltro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltro) ?? new FiltrosLazyLoadData();

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var parametros = new DynamicParameters();

                var fromBase = @" FROM SOCIOS S
                                  LEFT JOIN vAFI_Membresias M ON M.Cedula = S.CEDULA";

                var condiciones = new List<string>();
                if (!string.IsNullOrWhiteSpace(filtro.filtro))
                {
                    condiciones.Add(@"( S.cedula LIKE @Filtro 
                                     OR S.cedular LIKE @Filtro
                                     OR S.nombre LIKE @Filtro
                                     OR M.Membresia LIKE @Filtro )");
                    parametros.Add(ParamFiltro, $"%{filtro.filtro}%");
                }

                var whereClause = condiciones.Count > 0
                    ? Where + string.Join(AndOperator, condiciones)
                    : string.Empty;

                // columnas permitidas para sort
                var columnasPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    CedulaColumn,
                    "cedular",
                    "nombre",
                    "Membresia"
                };

                if (string.IsNullOrWhiteSpace(filtro.sortField) || !columnasPermitidas.Contains(filtro.sortField))
                {
                    filtro.sortField = CedulaColumn;
                }

                var sortDirection = filtro.sortOrder == 0 ? "DESC" : "ASC";
                var orderBy = string.Format(OrderByDynamicTemplate, filtro.sortField, sortDirection);

                var sqlCount = CountAll + fromBase + whereClause;
                response.Result.total = connection.QuerySingle<int>(sqlCount, parametros);

                if (filtro.pagina == 0)
                {
                    var sqlData = $@"
                        SELECT S.cedula, S.cedular, S.nombre, M.Membresia 
                        {fromBase}
                        {whereClause}
                        {orderBy}
                        {Offset} @Offset ROWS
                        {FetchNext} @PageSize {RowsOnly}";

                    parametros.Add(ParamOffset, filtro.pagina);
                    parametros.Add(ParamPageSize, filtro.paginacion);

                    response.Result.lista = connection.Query<SociosData>(sqlData, parametros).ToList();
                }
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

        #region Beneficio Producto

        /// <summary>
        /// Método para obtener los productos de beneficios
        /// </summary>
        public BeneficioProductoLista BeneficioProducto_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new BeneficioProductoLista { Total = 0 };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var parametros = new DynamicParameters();

                var fromBase = " FROM afi_bene_productos";

                var sqlCount = "SELECT COUNT(cod_producto)" + fromBase;
                info.Total = connection.QuerySingle<int>(sqlCount);

                var condiciones = new List<string>();
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(cod_producto LIKE @Filtro OR descripcion LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtro}%");
                }

                var whereClause = condiciones.Count > 0
                    ? Where + string.Join(AndOperator, condiciones)
                    : string.Empty;

                var orderAndPage = new StringBuilder(OrderByCodProducto);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var sqlData = @"SELECT cod_producto, descripcion, COSTO_UNIDAD"
                              + fromBase
                              + whereClause
                              + orderAndPage;

                info.productos = connection.Query<BeneficioProductoData>(sqlData, parametros).ToList();
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

        /// <summary>
        /// Método para obtener los departamentos
        /// </summary>
        public DepartamentoDataLista Departamentos_Obtener(int CodCliente, string Institucion, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var info = new DepartamentoDataLista { Total = 0 };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var parametros = new DynamicParameters();
                parametros.Add("@Institucion", Institucion);

                var condiciones = new List<string> { "cod_institucion = @Institucion" };

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(cod_departamento LIKE @Filtro OR descripcion LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtro}%");
                }

                var whereClause = Where + string.Join(AndOperator, condiciones);

                var sqlCount = "SELECT COUNT(cod_departamento) FROM AFDepartamentos" + whereClause;
                info.Total = connection.QuerySingle<int>(sqlCount, parametros);

                var orderAndPage = new StringBuilder(OrderByCodDepartamento);
                if (pagina != null && paginacion != null)
                {
                    orderAndPage.Append(OffsetFetchPage);
                    parametros.Add(ParamOffset, pagina.Value);
                    parametros.Add(ParamPageSize, paginacion.Value);
                }

                var sqlData = @"SELECT cod_departamento, descripcion 
                                FROM AFDepartamentos"
                              + whereClause
                              + orderAndPage;

                info.departamentos = connection.Query<DepartamentoData>(sqlData, parametros).ToList();
            }
            catch
            {
                info.Total = 0;
                info.departamentos = new List<DepartamentoData>();
            }
            return info;
        }

        #endregion

        #region Catálogos / UEN

        /// <summary>
        /// Obtengo catálogos de tabla SYS y BENE donde tipo es el tipo de catalogo y modulo es el código de la tabla
        /// </summary>
        public List<CatalogosLista> Catalogo_Obtener(int CodCliente, int tipo, int modulo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            List<CatalogosLista> lista;
            try
            {
                using var connection = new SqlConnection(clienteConnString);

                var procedure = "[spAFI_Bene_Catalogos_Consulta]";
                var values = new
                {
                    tipo,
                    Codigo = modulo
                };
                lista = connection.Query<CatalogosLista>(procedure, values, commandType: System.Data.CommandType.StoredProcedure).ToList();
            }
            catch
            {
                lista = new List<CatalogosLista>();
            }

            return lista;
        }

        /// <summary>
        /// Obtiene UENs
        /// </summary>
        public ErrorDto<List<CatalogosLista>> UENS_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CatalogosLista>> { Code = 0 };
            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = @"SELECT COD_UNIDAD as item, DESCRIPCION FROM CORE_UENS";
                response.Result = connection.Query<CatalogosLista>(query).ToList();

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

        #region DropDown Ordenes (Proveedores / Familias)

        public ErrorDto<List<DropDownListaGenericaModel>> CompraOrdenProveedoresLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var connection = new SqlConnection(stringConn);

                var Query = @"
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

                resp.Result = connection.Query<DropDownListaGenericaModel>(Query).ToList();

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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var connection = new SqlConnection(stringConn);

                var Query = @"
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

                resp.Result = connection.Query<DropDownListaGenericaModel>(Query).ToList();


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

        #region TipoProductoSub (jerárquico)

        public ErrorDto<List<TipoProductoSubGradaData>> TipoProductoSub_ObtenerTodos(int CodEmpresa, string Cod_Prodclas)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TipoProductoSubGradaData>>
            {
                Code = 0,
                Result = new List<TipoProductoSubGradaData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                // Convertimos la lista de codigos a lista para IN @param
                var codList = Cod_Prodclas
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();

                var query = @"
                        SELECT Cod_Prodclas, Cod_Linea_Sub, Descripcion, Activo, Cabys,
                               COD_CUENTA, NIVEL, COD_LINEA_SUB_MADRE
                        FROM PV_PROD_CLASIFICA_SUB 
                        WHERE Cod_Prodclas IN @CodProdclas";

                var info = connection.Query<TipoProductoSubDto>(query, new { CodProdclas = codList }).ToList();
                foreach (TipoProductoSubDto dt in info)
                {
                    dt.Estado = dt.Activo ? "ACTIVO" : "INACTIVO";

                    if (dt.Nivel == 1)
                    {
                        response.Result.Add(new TipoProductoSubGradaData
                        {
                            key = dt.Cod_Linea_Sub,
                            icon = "",
                            label = dt.Descripcion,
                            data = dt,
                            children = TipoProductoSub_SeguienteNivel(CodEmpresa, dt)
                        });
                    }
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new List<TipoProductoSubGradaData>();
            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = @"
                        SELECT Cod_Prodclas, Cod_Linea_Sub, Descripcion, Activo, Cabys,
                               COD_CUENTA, NIVEL, COD_LINEA_SUB_MADRE
                        FROM PV_PROD_CLASIFICA_SUB 
                        WHERE Cod_Prodclas = @CodProdclas 
                          AND COD_LINEA_SUB_MADRE = @CodLineaSubMadre";

                var info = connection.Query<TipoProductoSubDto>(query,
                    new
                    {
                        CodProdclas = padre.Cod_Prodclas,
                        CodLineaSubMadre = padre.Cod_Linea_Sub
                    }).ToList();

                foreach (TipoProductoSubDto dt in info)
                {
                    dt.Estado = dt.Activo ? "ACTIVO" : "INACTIVO";

                    response.Add(new TipoProductoSubGradaData
                    {
                        key = dt.Cod_Linea_Sub,
                        icon = "",
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

        #region Personas

        /// <summary>
        /// Obtener lista de personas
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> Personas_Obtener(int CodEmpresa, string jfiltro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltro) ?? new FiltrosLazyLoadData();

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var parametros = new DynamicParameters();

                var fromBase = " FROM socios";

                var condiciones = new List<string>();
                if (!string.IsNullOrWhiteSpace(filtro.filtro))
                {
                    condiciones.Add("(cedula LIKE @Filtro OR cedulaR LIKE @Filtro OR nombre LIKE @Filtro)");
                    parametros.Add(ParamFiltro, $"%{filtro.filtro}%");
                }

                var whereClause = condiciones.Count > 0
                    ? Where + string.Join(AndOperator, condiciones)
                    : string.Empty;

                // sortField permitido
                var columnasPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    CedulaColumn,
                    "cedulaR",
                    "nombre"
                };

                if (string.IsNullOrWhiteSpace(filtro.sortField) || !columnasPermitidas.Contains(filtro.sortField))
                {
                    filtro.sortField = CedulaColumn;
                }
                var sortDirection = filtro.sortOrder == 0 ? "DESC" : "ASC";
                var orderBy = string.Format(OrderByDynamicTemplate, filtro.sortField, sortDirection);

                var sqlCount = CountAll + fromBase + whereClause;
                response.Result.total = connection.QuerySingle<int>(sqlCount, parametros);

                if (filtro.pagina == 0)
                {
                    var sqlData = $@"
                        SELECT cedula, cedulaR, nombre
                        {fromBase}
                        {whereClause}
                        {orderBy}
                        {Offset} @Offset ROWS
                        {FetchNext} @PageSize {RowsOnly}";

                    parametros.Add(ParamOffset, filtro.pagina);
                    parametros.Add(ParamPageSize, filtro.paginacion);

                    response.Result.lista = connection.Query<AFCedulaDto>(sqlData, parametros).ToList();
                }
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