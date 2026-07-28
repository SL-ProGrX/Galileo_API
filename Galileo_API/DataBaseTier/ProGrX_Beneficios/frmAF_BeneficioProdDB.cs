using Dapper;
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

        /// <summary>
        /// Obtiene la lista de productos de beneficios con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro por código o descripción.</param>
        /// <returns>Lista de productos y total.</returns>
        public ErrorDto<ProductoDataLista> ProductoLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new ProductoDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM afi_bene_productos";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT cod_producto, descripcion, costo_unidad, cod_producto_inv
                                     FROM afi_bene_productos
                                     WHERE (@like IS NULL OR cod_producto LIKE @like OR descripcion LIKE @like)
                                     ORDER BY cod_producto
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.productos = connection.Query<ProductoData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Exporta la lista completa de productos de beneficios.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de productos.</returns>
        public ErrorDto<List<ProductoData>> Producto_Exportar(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT cod_producto, descripcion, costo_unidad
                                     FROM afi_bene_productos ORDER BY cod_producto";
                return connection.Query<ProductoData>(sql).ToList();
            });
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
