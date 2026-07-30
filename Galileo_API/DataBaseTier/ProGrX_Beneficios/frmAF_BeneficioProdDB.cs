using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Productos de Beneficios (frmAF_BeneficioProd).
    /// Consultas y helpers aquí; guardado en el parcial .Guardar.
    /// </summary>
    public partial class FrmAfBeneficioProdDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficioProdDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        // ==========================
        // Cuerpos SQL constantes
        // ==========================

        private const string SqlProductosSelect = @"
SELECT cod_producto, descripcion, costo_unidad, cod_producto_inv
FROM afi_bene_productos
";

        private const string SqlProductosWhere = @"
WHERE (@filtro IS NULL)
   OR (cod_producto LIKE @like)
   OR (descripcion LIKE @like)
";

        private const string SqlProductosCount = @"
SELECT COUNT(1)
FROM afi_bene_productos
" + SqlProductosWhere;

        /// <summary>
        /// Obtiene la lista de productos de beneficios con paginación, filtro y ordenamiento.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa (página, paginación, filtro y orden).</param>
        /// <returns>Lista de productos y total de registros.</returns>
        public ErrorDto<ProductoDataLista> AfiBeneficioProd_ProductoLista_Obtener(int CodCliente, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var lista = QueryProductos(connection, filtros, true, out var total);
                return new ProductoDataLista { total = total, productos = lista };
            });
        }

        /// <summary>
        /// Exporta la lista completa de productos de beneficios aplicando el filtro vigente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa; se ignora la paginación.</param>
        /// <returns>Lista de productos sin paginar.</returns>
        public ErrorDto<List<ProductoData>> AfiBeneficioProd_Producto_Exportar(int CodCliente, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                QueryProductos(connection, filtros, false, out _));
        }

        // ==========================
        // Helpers privados
        // ==========================

        /// <summary>
        /// Consulta los productos aplicando filtro, orden y, opcionalmente, paginación.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <param name="usarPaginacion">Indica si se aplica OFFSET/FETCH.</param>
        /// <param name="total">Total de registros que cumplen el filtro.</param>
        /// <returns>Lista de productos.</returns>
        private static List<ProductoData> QueryProductos(
            SqlConnection connection,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSort(filtros);

            total = connection.QuerySingle<int>(SqlProductosCount, new { filtro, like });

            var sqlList = SqlProductosSelect + SqlProductosWhere + $"\nORDER BY {sortField} {sortOrder}";

            var offset = filtros?.pagina ?? 0;
            var fetch = filtros?.paginacion ?? 0;

            if (usarPaginacion && fetch > 0)
            {
                sqlList += "\nOFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";
            }
            else
            {
                sqlList += ";";
            }

            return connection.Query<ProductoData>(sqlList, new { filtro, like, offset, fetch }).ToList();
        }

        /// <summary>
        /// Construye el texto de filtro y su patrón LIKE. Devuelve nulos cuando no hay filtro.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el filtro normalizado y su patrón LIKE.</returns>
        private static (string? filtro, string? like) BuildFiltroLike(FiltrosLazyLoadData filtros)
        {
            var texto = filtros?.filtro?.Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                return (null, null);
            }

            return (texto, $"%{texto}%");
        }

        /// <summary>
        /// Resuelve el campo y la dirección de ordenamiento usando una lista blanca de columnas.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el campo y la dirección de ordenamiento.</returns>
        private static (string sortField, string sortOrder) ResolveSort(FiltrosLazyLoadData filtros)
        {
            // ORDER BY seguro (whitelist), nunca se concatena texto recibido del usuario.
            var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "descripcion" => "descripcion",
                "costo_unidad" => "costo_unidad",
                "cod_producto_inv" => "cod_producto_inv",
                _ => "cod_producto"
            };

            // Convención de PrimeNG: -1 descendente, 1 ascendente (ASC por defecto).
            var sortOrder = filtros?.sortOrder == -1 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }

        /// <summary>
        /// Verifica si un producto existe en el catálogo.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="cod_producto">Código del producto.</param>
        /// <returns>True si existe.</returns>
        private static bool Producto_Existe(SqlConnection connection, string cod_producto)
        {
            const string sql = "SELECT COUNT(*) FROM afi_bene_productos WHERE cod_producto = @cod_producto";
            return connection.QueryFirstOrDefault<int>(sql, new { cod_producto }) > 0;
        }

        /// <summary>
        /// Determina si el producto de inventario pertenece a la clasificación de Beneficios Solidarios (tarjeta regalo).
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="cod_producto_inv">Código de producto de inventario.</param>
        /// <returns>1 si aplica como tarjeta regalo; 0 en caso contrario.</returns>
        private static int EsTarjetaRegalo(SqlConnection connection, string? cod_producto_inv)
        {
            const string sql = @"SELECT CASE WHEN EXISTS (
                                        SELECT 1 FROM PV_PRODUCTOS
                                        WHERE COD_PRODCLAS = (SELECT COD_PRODCLAS FROM PV_PROD_CLASIFICA WHERE DESCRIPCION = 'Beneficios Solidarios')
                                          AND COD_PRODUCTO = @cod_producto_inv)
                                     THEN 1 ELSE 0 END";
            return connection.QueryFirstOrDefault<int>(sql, new { cod_producto_inv });
        }
    }
}
