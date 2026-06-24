using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvEtiquetasDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvEtiquetasDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvEtiquetasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la consulta SQL según la opción de generación de etiquetas.
        /// </summary>
        /// <param name="opcion">Opción solicitada.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string ObtenerQueryGenerateSato(int opcion)
        {
            return opcion switch
            {
                0 => @"select 1 as Cantidad,
                              modelo,
                              cod_barras,
                              descripcion,
                              cod_producto,
                              round((precio_regular * ((impuesto_ventas / 100)+1)), @Value) as Precio
                       from pv_productos
                       where cod_producto = @CodProducto",
                1 => @"select E.Cantidad,
                              P.modelo,
                              P.cod_barras,
                              P.descripcion,
                              P.cod_producto,
                              round((precio_regular * ((impuesto_ventas / 100)+1)), @Value) as Precio
                       from pv_productos P
                       inner join Cpr_Compras_detalle E on P.cod_producto = E.cod_producto
                       where E.cod_proveedor = @CodProveedor
                         and E.cod_factura = @CodFactura",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Obtiene los parámetros de la consulta de etiquetas.
        /// </summary>
        /// <param name="request">Solicitud de generación.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosGenerateSato(GenerateSatoRequest request) => new
        {
            request.CodProducto,
            request.CodProveedor,
            request.CodFactura,
            request.Value
        };

        /// <summary>
        /// Genera el código de barras interno cuando el producto no posee uno válido.
        /// </summary>
        /// <param name="barData">Información actual del código de barras y clasificación.</param>
        /// <param name="codProducto">Código del producto.</param>
        /// <returns>Código de barras generado.</returns>
        private static string GenerarCodigoBarras(CodBarrasData barData, string codProducto)
        {
            return "2000"
                + barData.Cod_ProdClas.ToString().PadLeft(3, '0').Substring(0, 3)
                + codProducto.Trim().PadLeft(5, '0').Substring(0, 5);
        }

        /// <summary>
        /// Actualiza el código de barras del producto.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codProducto">Código del producto.</param>
        /// <param name="codBarras">Código de barras a guardar.</param>
        private static void ActualizarCodigoBarras(System.Data.IDbConnection connection, string codProducto, string codBarras)
        {
            connection.Execute(
                @"update pv_productos
                  set cod_barras = @CodBarras
                  where cod_producto = @CodProducto",
                new
                {
                    CodBarras = codBarras,
                    CodProducto = codProducto
                });
        }

        /// <summary>
        /// Completa los códigos de barras faltantes en la lista de productos.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="productos">Lista de productos a revisar.</param>
        private static void CompletarCodigosBarras(System.Data.IDbConnection connection, IEnumerable<ProductData> productos)
        {
            foreach (ProductData item in productos.Where(x => string.IsNullOrEmpty(x.Cod_Barras)))
            {
                var barData = connection.QueryFirstOrDefault<CodBarrasData>(
                    @"select cod_barras,
                             cod_ProdClas
                      from pv_productos
                      where cod_producto = @CodProducto",
                    new { CodProducto = item.Cod_Producto });

                if (barData is null)
                {
                    continue;
                }

                string xBarra = (barData.Cod_Barras ?? string.Empty).Trim();
                if (xBarra.Length < 12)
                {
                    xBarra = GenerarCodigoBarras(barData, item.Cod_Producto ?? string.Empty);
                    ActualizarCodigoBarras(connection, item.Cod_Producto ?? string.Empty, xBarra);
                    item.Cod_Barras = xBarra;
                }
            }
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Genera los datos necesarios para la impresión de etiquetas SATO.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Solicitud de generación de etiquetas.</param>
        /// <returns>Listado de productos para impresión.</returns>
        public ErrorDto<List<ProductData>> GenerateSato(int CodEmpresa, GenerateSatoRequest request)
        {
            var query = ObtenerQueryGenerateSato(request.Opcion);
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ErrorDto<List<ProductData>>
                {
                    Code = -2,
                    Description = "La opción de generación indicada no es válida.",
                    Result = new List<ProductData>()
                };
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var productos = connection.Query<ProductData>(query, CrearParametrosGenerateSato(request)).ToList();
                CompletarCodigosBarras(connection, productos);
                return productos;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<ProductData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al generar las etiquetas SATO.", result.Code.GetValueOrDefault(-1), new List<ProductData>());
        }

        #endregion
    }
}