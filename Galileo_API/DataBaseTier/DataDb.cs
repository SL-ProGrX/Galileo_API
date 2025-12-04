using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class DataDB
    {
        private readonly IConfiguration _config;
        const string AndOperator = " AND ";
        const string Offset = " OFFSET ";
        const string FetchNext = " FETCH NEXT ";
        const string RowsOnly = " ROWS ONLY ";
        const string Rows = " ROWS ";

        public DataDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Método para obtener los proveedores
        /// </summary>
        public ErrorDto<ProveedoresDataLista> Proveedores_Obtener(int CodCliente, ProveedorDataFiltros jFiltros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<ProveedoresDataLista>();
            response.Result = new ProveedoresDataLista();
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ", valWhere;
                using var connection = new SqlConnection(clienteConnString);

                valWhere = $"(ESTADO = 'A' OR ESTADO = 'T')";

                if (jFiltros.autoGestion == true && jFiltros.ventas == true)
                {
                    valWhere += " AND ( WEB_AUTO_GESTION = 1 OR WEB_FERIAS = 1 ) ";
                }
                else if (jFiltros.autoGestion == true)
                {
                    valWhere += " AND WEB_AUTO_GESTION = 1 ";
                }
                else if (jFiltros.ventas == true)
                {
                    valWhere += " AND WEB_FERIAS = 1 ";
                }

                //Busco Total
                query = $"SELECT COUNT(*) FROM CXP_PROVEEDORES WHERE {valWhere} ";
                response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                if (jFiltros.filtro != null)
                {
                    valWhere += " AND ( COD_PROVEEDOR LIKE '%" + jFiltros.filtro + "%' OR DESCRIPCION LIKE '%" + jFiltros.filtro + "%') ";
                }

                if (jFiltros.pagina != null)
                {
                    paginaActual = Offset + jFiltros.pagina + Rows;
                    paginacionActual = FetchNext + jFiltros.paginacion + RowsOnly;
                }

                query = $@"SELECT COD_PROVEEDOR, DESCRIPCION, CEDJUR FROM CXP_PROVEEDORES
                            WHERE 
                             {valWhere}
                        ORDER BY COD_PROVEEDOR
                            {paginaActual}
                            {paginacionActual} ";

                response.Result.Proveedores = connection.Query<ProveedorData>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Result.Total = 0;
                response.Description = ex.Message;
                response.Code = -1;
            }
            return response;
        }

        /// <summary>
        /// Método para obtener los cargos
        /// </summary>
        public CargoDataLista Cargos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            CargoDataLista info = new CargoDataLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                //Busco Total
                query = "select COUNT(COD_CARGO) from CXP_CARGOS where ACTIVO = 1";
                info.Total = connection.Query<int>(query).FirstOrDefault();

                if (filtro != null)
                {
                    filtro = " AND COD_CARGO LIKE '%" + filtro + "%' OR DESCRIPCION LIKE '%" + filtro + "%' ";
                }

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                query = $@"select COD_CARGO, DESCRIPCION, 0 as MONTO from CXP_CARGOS where ACTIVO = 1
                                     {filtro} 
                                    ORDER BY COD_CARGO
                                    {paginaActual}
                                    {paginacionActual} ";

                info.Cargos = connection.Query<CargoData>(query).ToList();
            }
            catch (Exception)
            {
                info.Total = 0;
                info.Cargos = new List<CargoData>();
            }
            return info;
        }

        /// <summary>
        /// Método para obtener bodegas
        /// </summary>
        public BodegaDataLista Bodegas_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            BodegaDataLista info = new BodegaDataLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                //Busco Total
                query = $@"select COUNT(cod_bodega) from pv_bodegas where permite_salidas = 1";
                info.Total = connection.Query<int>(query).FirstOrDefault();

                if (filtro != null)
                {
                    filtro = " AND cod_bodega LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                }

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                query = $@"select  cod_bodega,descripcion from pv_bodegas where permite_salidas = 1
                                     {filtro} 
                                    ORDER BY cod_bodega
                                    {paginaActual}
                                    {paginacionActual} ";

                info.bodegas = connection.Query<BodegaData>(query).ToList();
            }
            catch (Exception)
            {
                info.Total = 0;
                info.bodegas = new List<BodegaData>();
            }
            return info;
        }

        /// <summary>
        /// Método para obtener los artículos
        /// </summary>
        public ErrorDto<ArticuloDataLista> Articulos_Obtener(int CodCliente, ArticuloDataFiltros filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<ArticuloDataLista>
            {
                Code = 0,
                Result = new ArticuloDataLista()
            };
            response.Result.Total = 0;

            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                string joinProdUen = "";
                string whereEstado = BuildArticulosWhereClause(filtro, ref joinProdUen);

                using var connection = new SqlConnection(clienteConnString);

                query = $@"SELECT COUNT(P.COD_PRODUCTO) 
                           FROM pv_productos P {whereEstado}";
                response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                if (filtro.pagina != null)
                {
                    paginaActual = Offset + filtro.pagina + Rows;
                    paginacionActual = FetchNext + filtro.paginacion + RowsOnly;
                }

                query = $@"
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
                    LEFT JOIN PV_PROD_CLASIFICA_SUB Cs ON Cs.COD_PRODCLAS = P.COD_PRODCLAS 
                        AND Cs.COD_LINEA_SUB = P.COD_LINEA_SUB
                    {joinProdUen}
                    {whereEstado}
                    ORDER BY P.COD_PRODUCTO
                    {paginaActual}
                    {paginacionActual}";

                response.Result.Articulos = connection.Query<ArticuloData>(query).ToList();
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

        private static string BuildArticulosWhereClause(ArticuloDataFiltros filtro, ref string joinProdUen)
        {
            var clauses = new List<string>();

            if (filtro.catalogo != 0)
            {
                clauses.Add("P.ESTADO = 'A'");
            }

            if (!string.IsNullOrEmpty(filtro.cod_unidad) && filtro.cod_unidad != "T")
            {
                joinProdUen = " LEFT JOIN CPR_PRODUCTOS_UENS PU ON PU.COD_PRODUCTO = P.COD_PRODUCTO ";
                clauses.Add($"PU.COD_UNIDAD = '{filtro.cod_unidad}'");
            }

            if (!string.IsNullOrEmpty(filtro.filtro))
            {
                clauses.Add(
                    "( P.DESCRIPCION LIKE '%" + filtro.filtro + "%' " +
                    "OR CONCAT( FORMAT(Cs.COD_PRODCLAS, ' 0') , " +
                    "FORMAT(ISNULL(Cs.NIVEL,' 00'), ' 0'), " +
                    "FORMAT(ISNULL(Cs.COD_LINEA_SUB_MADRE,1) , ' 0'), " +
                    "FORMAT(ISNULL(Cs.COD_LINEA_SUB,1) , ' ') " +
                    ",P.COD_PRODUCTO ) LIKE '%" + filtro.filtro + "%' " +
                    "OR P.COD_BARRAS LIKE '%" + filtro.filtro + "%' )"
                );
            }

            if (filtro.familia > 0)
            {
                clauses.Add($"P.COD_PRODCLAS = '{filtro.familia}'");
            }

            if (!string.IsNullOrEmpty(filtro.sublinea))
            {
                clauses.Add($"P.COD_LINEA_SUB = '{filtro.sublinea}'");
            }

            if (clauses.Count > 0)
            {
                return " WHERE " + string.Join(AndOperator, clauses);
            }
            return "";
        }

        /// <summary>
        /// Método para obtener las ordenes de compra
        /// </summary>
        public OrdenesDataLista Ordenes_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string? proveedor, string? familia)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            OrdenesDataLista info = new OrdenesDataLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                //Busco Total
                query = "select COUNT(cod_orden) from cpr_ordenes";
                info.Total = connection.Query<int>(query).FirstOrDefault();

                const string LikeOr = "%' OR ";
                if (filtro != null)
                {
                    filtro = " where (cod_orden LIKE '%" + filtro + LikeOr +
                             "genera_user LIKE '%" + filtro + LikeOr +
                             "cod_solicitud LIKE '%" + filtro + LikeOr +
                             " nota LIKE '%" + filtro + LikeOr +
                             " familia LIKE '%" + filtro + LikeOr +
                             "proveedor LIKE '%" + filtro + "%' )";
                }
                proveedor = (proveedor != null) ? proveedor.Replace("null", "").Trim() : null;
                familia = (familia != null) ? familia.Replace("null", "").Trim() : null;

                if (proveedor != null)
                {
                    if (filtro == null)
                    {
                        filtro += " WHERE ";
                    }
                    else
                    {
                        filtro += AndOperator;
                    }
                    filtro += " proveedor LIKE '%" + proveedor + "%'";
                }

                if (familia != null)
                {
                    if (filtro == null)
                    {
                        filtro += " WHERE ";
                    }
                    else
                    {
                        filtro += AndOperator;
                    }
                    filtro += " familia LIKE '%" + familia + "%'";
                }

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                query = $@"
                        SELECT * FROM (
                            select RIGHT(REPLICATE('0', 10) + CAST(sp.CPR_ID AS VARCHAR), 10) AS cod_solicitud, O.cod_orden,O.genera_user,O.nota,
                            O.COD_PROVEEDOR + '-' + cp.DESCRIPCION AS proveedor,
                            -- Subconsulta para concatenar familias distintas
                            STUFF((
                                SELECT DISTINCT ', ' + ppc2.DESCRIPCION
                                FROM CPR_ORDENES_DETALLE cod2
                                INNER JOIN PV_PRODUCTOS pp2 ON cod2.COD_PRODUCTO = pp2.COD_PRODUCTO
                                LEFT JOIN PV_PROD_CLASIFICA ppc2 ON ppc2.COD_PRODCLAS = pp2.COD_PRODCLAS
                                WHERE cod2.COD_ORDEN = O.COD_ORDEN
                                      AND ppc2.DESCRIPCION IS NOT NULL
                                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS familia
                            from cpr_ordenes O
                            left join CPR_SOLICITUD_PROV sp ON 
                                sp.ADJUDICA_ORDEN  = O.COD_ORDEN 
                                AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
                            LEFT JOIN CXP_PROVEEDORES cp 
                                ON cp.COD_PROVEEDOR = O.COD_PROVEEDOR
                            GROUP BY 
                                sp.CPR_ID, O.cod_orden, O.genera_user, O.nota, O.COD_PROVEEDOR, cp.DESCRIPCION ) T
                             {filtro} 
                            ORDER BY cod_orden
                            {paginaActual}
                            {paginacionActual} ";

                info.Ordenes = connection.Query<OrdenData>(query).ToList();
            }
            catch (Exception)
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

            OrdenesDataLista info = new OrdenesDataLista();
            info.Total = 0;
            try
            {
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                // Busco Total
                var totalQuery = @"select COUNT(O.cod_orden) from cpr_ordenes O                       
                                      left join CPR_SOLICITUD_PROV sp ON 
                                      sp.ADJUDICA_ORDEN  = O.COD_ORDEN 
                                      AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
                               where O.Estado in('A') and O.Proceso in('A','X')";
                info.Total = connection.Query<int>(totalQuery).FirstOrDefault();

                string whereClause = BuildOrdenesFiltroWhereClause(filtro, proveedor, familia, subfamilia);

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                var query = $@"SELECT * FROM (  
                                    SELECT 
                                        RIGHT(REPLICATE('0', 10) + CAST(sp.CPR_ID AS VARCHAR), 10) AS cod_solicitud, 
                                        O.cod_orden, 
                                        O.genera_user,
                                        O.nota, 
                                        O.COD_PROVEEDOR + '-' + cp.DESCRIPCION AS proveedor,
                                        -- Subconsulta para concatenar familias distintas
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
                                        sp.CPR_ID, O.cod_orden, O.genera_user, O.nota, O.COD_PROVEEDOR, cp.DESCRIPCION) T 
                                        {whereClause} 
                                    ORDER BY cod_orden
                                        {paginaActual}
                                        {paginacionActual}";

                info.Ordenes = connection.Query<OrdenData>(query).ToList();
            }
            catch (Exception)
            {
                info.Total = 0;
                info.Ordenes = new List<OrdenData>();
            }
            return info;
        }

        private static string BuildOrdenesFiltroWhereClause(string? filtro, string? proveedor, string? familia, string? subfamilia)
        {
            var clauses = new List<string>();
            string where = "";

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                clauses.Add($"(cod_orden LIKE '%{filtro}%' OR genera_user LIKE '%{filtro}%' OR cod_solicitud LIKE '%{filtro}%' OR nota LIKE '%{filtro}%' OR familia LIKE '%{filtro}%' OR proveedor LIKE '%{filtro}%')");
            }

            proveedor = (proveedor != null) ? proveedor.Replace("null", "").Trim() : null;
            familia = (familia != null) ? familia.Replace("null", "").Trim() : null;
            subfamilia = (subfamilia != null) ? subfamilia.Replace("null", "").Trim() : null;

            if (!string.IsNullOrWhiteSpace(proveedor))
            {
                clauses.Add($"proveedor LIKE '%{proveedor}%'");
            }

            if (!string.IsNullOrWhiteSpace(familia))
            {
                clauses.Add($"familia LIKE '%{familia}%'");
            }

            if (!string.IsNullOrWhiteSpace(subfamilia))
            {
                if (subfamilia == "5")
                {
                    clauses.Add("subfamilia like '%%'");
                }
                else
                {
                    clauses.Add($"subfamilia LIKE '%{subfamilia}%'");
                }
            }

            if (clauses.Count > 0)
            {
                where = "WHERE " + string.Join(AndOperator, clauses);
            }

            return where;
        }

        /// <summary>
        /// Método para obtener las facturas
        /// </summary>
        public FacturasDataLista ObtenerListaFacturas(int CodCliente, int CodProveedor, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            FacturasDataLista info = new FacturasDataLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                //Busco Total
                query = $@"select COUNT(cod_factura)  
                            from cpr_compras E inner join 
                            cxp_Proveedores P on E.cod_proveedor = P.cod_proveedor 
                                    and E.cod_proveedor =  {CodProveedor} ";
                info.Total = connection.Query<int>(query).FirstOrDefault();

                if (filtro != null)
                {
                    filtro = " AND (E.cod_factura LIKE '%" + filtro + "%' OR " +
                                " P.descripcion LIKE '%" + filtro + "%' ) ";
                }

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                query = $@"select  E.cod_factura,P.descripcion as Proveedor,E.total
                                    from cpr_compras E inner join cxp_Proveedores P on E.cod_proveedor = P.cod_proveedor
                                         and E.cod_proveedor =  {CodProveedor} 
                                         {filtro} 
                                        ORDER BY cod_orden
                                        {paginaActual}
                                        {paginacionActual} ";

                info.Facturas = connection.Query<FacturasData>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                info.Total = 0;
                info.Facturas = new List<FacturasData>();
            }
            return info;
        }

        /// <summary>
        /// Método para obtener los usuarios
        /// </summary>
        public UsuarioDataLista Usuarios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            UsuarioDataLista info = new UsuarioDataLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                //Busco Total
                query = "SELECT COUNT(*) FROM usuarios";
                info.Total = connection.Query<int>(query).FirstOrDefault();

                if (filtro != null)
                {
                    filtro = " WHERE nombre LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' AND ESTADO = 'A' ";
                }

                if (filtro == null)
                {
                    filtro = " WHERE ESTADO = 'A' ";
                }

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                query = $@"select nombre,descripcion from usuarios
                                     {filtro}   
                                    ORDER BY nombre
                                    {paginaActual}
                                    {paginacionActual} ";

                info.Usuarios = connection.Query<UsuarioData>(query).ToList();
            }
            catch (Exception)
            {
                info.Total = 0;
                info.Usuarios = new List<UsuarioData>();
            }
            return info;
        }

        /// <summary>
        /// Método para obtener las facturas de proveedores
        /// </summary>
        public FacturasProveedorLista FacturaProveedor_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var filtrosModel = JsonConvert.DeserializeObject<FacturasProveedorDataFiltros>(filtros) ?? new FacturasProveedorDataFiltros();
            FacturasProveedorLista info = new FacturasProveedorLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                //Busco Total
                query = "select COUNT(E.cod_compra) from cpr_Compras E inner join cxp_proveedores P on E.cod_proveedor = P.cod_proveedor";
                info.Total = connection.Query<int>(query).FirstOrDefault();

                if (filtrosModel.filtro != "")
                {
                    filtrosModel.filtro = " WHERE  E.cod_compra LIKE '%" + filtrosModel.filtro + "%'" +
                        " OR E.cod_orden LIKE '%" + filtrosModel.filtro + "%' " +
                        " OR E.cod_factura LIKE '%" + filtrosModel.filtro + "%' " +
                        " OR P.descripcion  LIKE '%" + filtrosModel.filtro + "%'";
                }

                if (filtrosModel.cod_proveedor > 0)
                {
                    filtrosModel.filtro += " AND P.cod_proveedor = " + filtrosModel.cod_proveedor;
                }

                if (filtrosModel.pagina == 0)
                {
                    paginaActual = Offset + filtrosModel.pagina + Rows;
                    paginacionActual = FetchNext + filtrosModel.paginacion + RowsOnly;
                }

                query = $@"select E.cod_compra,E.cod_orden,E.cod_factura, P.descripcion as Proveedor, 
P.cod_proveedor, RIGHT(REPLICATE('0', 10) + CAST(s.CPR_ID AS VARCHAR), 10)  AS no_solicitud
                                from cpr_Compras E 
                                inner join cxp_proveedores P on E.cod_proveedor = P.cod_proveedor
                                left JOIN CPR_SOLICITUD_PROV s ON 
                                         s.ADJUDICA_ORDEN  = E .COD_ORDEN 
                                     AND s.PROVEEDOR_CODIGO = E.cod_proveedor
                                         {filtrosModel.filtro} 
                                        ORDER BY E.cod_compra
                                        {paginaActual}
                                        {paginacionActual} ";

                info.Facturas = connection.Query<FacturasProveedorData>(query).ToList();
            }
            catch (Exception)
            {
                info.Total = 0;
                info.Facturas = new List<FacturasProveedorData>();
            }
            return info;
        }

        /// <summary>
        /// Método para obtener las devoluciones de compra
        /// </summary>
        public CompraDevLista Devoluciones_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            CompraDevLista info = new CompraDevLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                //Busco Total
                query = "select count(*) from cpr_compras_dev D inner join cxp_proveedores P on D.cod_proveedor = P.cod_proveedor";
                info.Total = connection.Query<int>(query).FirstOrDefault();

                if (filtro != null)
                {
                    filtro = " WHERE  D.cod_compra_dev LIKE '%" + filtro + "%' " +
                        " OR D.cod_factura LIKE '%" + filtro + "%' " +
                        " OR P.descripcion  LIKE '%" + filtro + "%'";
                }

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                query = $@"select D.cod_compra_dev,P.descripcion as Proveedor,D.cod_factura,D.notas,D.fecha
                                  from cpr_compras_dev D inner join cxp_proveedores P on D.cod_proveedor = P.cod_proveedor
                                         {filtro} 
                                       ORDER BY D.cod_compra_dev
                                        {paginaActual}
                                        {paginacionActual} ";

                info.devoluciones = connection.Query<CompraDevData>(query).ToList();
            }
            catch (Exception)
            {
                info.Total = 0;
                info.devoluciones = new List<CompraDevData>();
            }
            return info;
        }

        /// <summary>
        /// Método para obtener los beneficios
        /// </summary>
        public BeneficioDataLista Beneficios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            BeneficioDataLista info = new BeneficioDataLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                //Busco Total
                query = "select count(cod_beneficio) from afi_beneficios";
                info.Total = connection.Query<int>(query).FirstOrDefault();

                if (filtro != null)
                {
                    filtro = " WHERE  cod_beneficio LIKE '%" + filtro + "%' " +
                        " OR descripcion LIKE '%" + filtro + "%' ";
                }

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                query = $@"select cod_beneficio,descripcion from afi_beneficios
                                     {filtro} 
                                   ORDER BY cod_beneficio
                                    {paginaActual}
                                    {paginacionActual} ";
                info.Beneficios = connection.Query<BeneficioData>(query).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                info.Total = 0;
                info.Beneficios = new List<BeneficioData>();
            }
            return info;
        }

        /// <summary>
        /// Método para obtener los socios (V1 Galileo)
        /// </summary>
        public ErrorDto<SociosDataLista> Socios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<SociosDataLista>();
            response.Result = new SociosDataLista();
            response.Result.Total = 0;

            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                if (filtro != null)
                {
                    filtro = " WHERE  S.cedula LIKE '%" + filtro + "%' " +
                        " OR S.cedular LIKE '%" + filtro + "%' " +
                    " OR S.nombre LIKE '%" + filtro + "%'" +
                    " OR M.Membresia LIKE '%" + filtro + "%' ";
                }
                //Busco Total
                query = $"Select count(*) from SOCIOS S left join vAFI_Membresias M ON M.Cedula = S.CEDULA {filtro}";
                response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                query = $@"Select S.cedula,S.cedular,S.nombre, M.Membresia from SOCIOS S
                                      left join vAFI_Membresias M ON M.Cedula = S.CEDULA
                                         {filtro} 
                                       ORDER BY S.cedula
                                        {paginaActual}
                                        {paginacionActual} ";
                response.Result.socios = connection.Query<SociosData>(query).ToList();
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

        /// <summary>
        /// Método para obtener los socios
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

                if (!string.IsNullOrEmpty(filtro.filtro))
                {
                    filtro.filtro = $@"WHERE ( 
                                             S.cedula like '%{filtro.filtro}%' 
                                          OR S.cedular like '%{filtro.filtro}%'
                                          OR S.nombre like '%{filtro.filtro}%'
                                          OR M.membresia like '%{filtro.filtro}%'
                                      )";
                }

                if (filtro.sortField == "" || filtro.sortField == null)
                {
                    filtro.sortField = "cedula";
                }

                //Busco Total
                var query = $"Select count(*) from SOCIOS S left join vAFI_Membresias M ON M.Cedula = S.CEDULA {filtro.filtro}";
                response.Result.total = connection.Query<int>(query).FirstOrDefault();

                if (filtro.pagina == 0)
                {
                    query = $@"Select S.cedula,S.cedular,S.nombre, M.Membresia from SOCIOS S
                                      left join vAFI_Membresias M ON M.Cedula = S.CEDULA
                           {filtro.filtro} order by {filtro.sortField} {(filtro.sortOrder == 0 ? "DESC" : "ASC")}  
                                      OFFSET {filtro.pagina} ROWS
                                      FETCH NEXT {filtro.paginacion} ROWS ONLY ";

                    response.Result.lista = connection.Query<SociosData>(query).ToList();
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

        /// <summary>
        /// Método para obtener los productos de beneficios
        /// </summary>
        public BeneficioProductoLista BeneficioProducto_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            BeneficioProductoLista info = new BeneficioProductoLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                //Busco Total
                query = "Select count(cod_producto) From afi_bene_productos";
                info.Total = connection.Query<int>(query).FirstOrDefault();

                if (filtro != null)
                {
                    filtro = " WHERE  cod_producto LIKE '%" + filtro + "%' " +
                        " OR descripcion LIKE '%" + filtro + "%' ";
                }

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                query = $@"Select cod_producto,descripcion, COSTO_UNIDAD From afi_bene_productos
                                     {filtro} 
                                   ORDER BY cod_producto
                                    {paginaActual}
                                    {paginacionActual} ";
                info.productos = connection.Query<BeneficioProductoData>(query).ToList();
            }
            catch (Exception)
            {
                info.Total = 0;
                info.productos = new List<BeneficioProductoData>();
            }
            return info;
        }

        /// <summary>
        /// Método para obtener los departamentos
        /// </summary>
        public DepartamentoDataLista Departamentos_Obtener(int CodCliente, string Institucion, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            DepartamentoDataLista info = new DepartamentoDataLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                //Busco Total
                query = $"Select count(cod_departamento) From AFDepartamentos where cod_institucion = '{Institucion}' ";
                info.Total = connection.Query<int>(query).FirstOrDefault();

                if (filtro != null)
                {
                    filtro = " AND  cod_departamento LIKE '%" + filtro + "%' " +
                        " OR descripcion LIKE '%" + filtro + "%' ";
                }

                if (pagina != null)
                {
                    paginaActual = Offset + pagina + Rows;
                    paginacionActual = FetchNext + paginacion + RowsOnly;
                }

                query = $@"select cod_departamento,descripcion from AFDepartamentos where cod_institucion = '{Institucion}' 
                                     {filtro} 
                                   ORDER BY cod_departamento
                                    {paginaActual}
                                    {paginacionActual} ";
                info.departamentos = connection.Query<DepartamentoData>(query).ToList();
            }
            catch (Exception)
            {
                info.Total = 0;
                info.departamentos = new List<DepartamentoData>();
            }
            return info;
        }

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
                    tipo = tipo,
                    Codigo = modulo
                };
                lista = connection.Query<CatalogosLista>(procedure, values, commandType: System.Data.CommandType.StoredProcedure).ToList();
            }
            catch (Exception)
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
            var response = new ErrorDto<List<CatalogosLista>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = $@"select COD_UNIDAD as item, DESCRIPCION FROM CORE_UENS";
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

        public ErrorDto<List<DropDownListaGenericaModel>> CompraOrdenProveedoresLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var connection = new SqlConnection(stringConn);

                var Query = $@"SELECT  DISTINCT
                                  O.COD_PROVEEDOR as item,
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

                var Query = $@"SELECT DISTINCT
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

                var query = $@"SELECT Cod_Prodclas,Cod_Linea_Sub, Descripcion, Activo, Cabys 
                                          ,COD_CUENTA, NIVEL, COD_LINEA_SUB_MADRE
                                              FROM PV_PROD_CLASIFICA_SUB 
                                        WHERE Cod_Prodclas IN ({Cod_Prodclas})";

                var info = connection.Query<TipoProductoSubDto>(query).ToList();
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

                var query = $@"SELECT Cod_Prodclas,Cod_Linea_Sub, Descripcion, Activo, Cabys 
                                            ,COD_CUENTA, NIVEL, COD_LINEA_SUB_MADRE
                                                FROM PV_PROD_CLASIFICA_SUB 
                                          WHERE Cod_Prodclas = '{padre.Cod_Prodclas}' 
                                          AND COD_LINEA_SUB_MADRE = '{padre.Cod_Linea_Sub}' ";

                var info = connection.Query<TipoProductoSubDto>(query).ToList();
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
            catch (Exception)
            {
                response = new List<TipoProductoSubGradaData>();
            }
            return response;
        }

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

                var query = $@"SELECT count(*) From socios";
                response.Result.total = connection.Query<int>(query).FirstOrDefault();

                if (filtro.filtro != null && filtro.filtro != "")
                {
                    filtro.filtro = $@"WHERE ( 
                                             cedula like '%{filtro.filtro}%' 
                                          OR cedulaR like '%{filtro.filtro}%'
                                          OR nombre like '%{filtro.filtro}%'
                                      )";
                }

                if (filtro.sortField == "" || filtro.sortField == null)
                {
                    filtro.sortField = "cedula";
                }

                if (filtro.pagina == 0)
                {
                    query = $@"SELECT cedula,cedulaR,nombre From socios
                           {filtro.filtro} order by {filtro.sortField} {(filtro.sortOrder == 0 ? "DESC" : "ASC")}  
                                      OFFSET {filtro.pagina} ROWS
                                      FETCH NEXT {filtro.paginacion} ROWS ONLY ";

                    response.Result.lista = connection.Query<AFCedulaDto>(query).ToList();
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
    }
}