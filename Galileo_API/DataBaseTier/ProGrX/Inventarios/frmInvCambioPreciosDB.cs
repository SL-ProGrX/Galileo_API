using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvCambioPreciosDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvCambioPreciosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvCambioPreciosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta estándar para operaciones no query.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="successMessage">Mensaje de éxito.</param>
        /// <param name="errorMessage">Mensaje de error.</param>
        /// <returns>Respuesta estándar para operaciones no query.</returns>
        private static ErrorDto CrearRespuestaNonQuery(ErrorDto result, string successMessage, string errorMessage)
        {
            return result.Code == 0
                ? DbHelper.OkResponse(successMessage)
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene el tipo de producto a registrar según unidad y familia.
        /// </summary>
        /// <param name="precio">Datos del producto.</param>
        /// <returns>Tipo de producto calculado.</returns>
        private static string ObtenerTipoProducto(PrecioExcelDto precio)
        {
            if (string.Equals(precio.unidad_medida, "SP", StringComparison.OrdinalIgnoreCase))
            {
                return "S";
            }

            if (string.Equals(precio.familia, "ACTIVOS", StringComparison.OrdinalIgnoreCase))
            {
                return "A";
            }

            return "P";
        }

        /// <summary>
        /// Obtiene el siguiente código de producto disponible.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <returns>Código de producto generado.</returns>
        private static string ObtenerSiguienteCodigoProducto(IDbConnection connection)
        {
            return connection.QueryFirstOrDefault<string>(
                "select FORMAT(NEXT VALUE FOR SeqPv_Productos, '000000')") ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el código de categoría a partir de su descripción.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="categoria">Descripción de categoría.</param>
        /// <returns>Código de categoría encontrado.</returns>
        private static string ObtenerCodigoCategoria(IDbConnection connection, string? categoria)
        {
            return connection.QueryFirstOrDefault<string>(
                @"SELECT COD_PRODCLAS
                  FROM PV_PROD_CLASIFICA
                  WHERE UPPER(DESCRIPCION) = UPPER(@Categoria)",
                new { Categoria = categoria }) ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el código de familia a partir de su descripción y categoría.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="familia">Descripción de familia.</param>
        /// <param name="categoria">Código de categoría.</param>
        /// <returns>Código de familia encontrado.</returns>
        private static string ObtenerCodigoFamilia(IDbConnection connection, string? familia, string categoria)
        {
            return connection.QueryFirstOrDefault<string>(
                @"SELECT COD_LINEA_SUB
                  FROM PV_PROD_CLASIFICA_SUB
                  WHERE UPPER(DESCRIPCION) = UPPER(@Familia)
                    AND COD_PRODCLAS = @Categoria
                    AND COD_LINEA_SUB_MADRE IS NOT NULL",
                new
                {
                    Familia = familia,
                    Categoria = categoria
                }) ?? string.Empty;
        }

        /// <summary>
        /// Inserta un nuevo producto desde el archivo de cambios de precios.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="precio">Datos del producto.</param>
        /// <param name="tipoProducto">Tipo de producto calculado.</param>
        /// <param name="categoria">Código de categoría.</param>
        /// <param name="familia">Código de familia.</param>
        private static void InsertarProductoNuevo(IDbConnection connection, PrecioExcelDto precio, string tipoProducto, string categoria, string familia)
        {
            connection.Execute(
                @"INSERT INTO PV_PRODUCTOS (
                        COD_PRODUCTO, COD_MARCA, COD_UNIDAD, COD_PRODCLAS, COD_BARRAS, LOTES, DESCRIPCION,
                        TIPO_PRODUCTO, ESTADO, MODELO, OBSERVACION, COSTO_REGULAR, PRECIO_REGULAR, DIR_FOTOGRAFIA,
                        COD_FABRICANTE, COMISION_MONTO, COMISION_UNIDAD, IMPUESTO_VENTAS, IMPUESTO_CONSUMO,
                        INVENTARIO_CALCULA, INVENTARIO_MINIMO, INVENTARIO_MAXIMO, FRACCIONES, PRECIO_COMPRA,
                        DESCUENTO_TIPO, DESCUENTO_VALOR, COD_CUENTA, EXISTENCIA, USER_CREA, USER_MODIFICA,
                        ULTIMA_MODIFICACION, PORC_UTILIDAD, TIPO_CAMBIO, SIMILAR, COD_LINEA_SUB, CABYS,
                        FE_SINC_FECHA, FE_SINC_USER, I_STOCK, I_VENTAENLINEA, REGISTRO_FECHA, VENTA_FREQ_DIAS,
                        VENTA_QTY_MAX, TIPO_ACTIVO, I_FILTRADO, PUNTO_REORDEN, TIEMPO_ENTREGA_DIAS)
                  VALUES (
                        @Cod_Producto, '01', @Unidad_Medida, @Categoria, 0, 0, @Descripcion,
                        @TipoProducto, 'A', '', @Notas, 0, @Precio_Nuevo, '', '', 0, 0, 0, 0,
                        'N', 0, 0, 1, NULL, NULL, NULL, NULL, NULL, 'demo', NULL,
                        NULL, 0, 0, NULL, @Familia, 1, 0, NULL, NULL, 1, 0, NULL, 1,
                        99, 0, 0, 0)",
                new
                {
                    precio.cod_producto,
                    Unidad_Medida = precio.unidad_medida,
                    Categoria = categoria,
                    precio.descripcion,
                    TipoProducto = tipoProducto,
                    precio.notas,
                    Precio_Nuevo = precio.precio_nuevo,
                    Familia = familia
                });
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene el detalle de una orden de compra para cambio de precios.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodFactura">Código de factura.</param>
        /// <param name="CodProveedor">Código del proveedor.</param>
        /// <returns>Detalle de productos de la orden.</returns>
        public ErrorDto<List<FacturaPrecioDetalleDto>> OrdenesDetalle_Obtener(int CodEmpresa, string CodFactura, int? CodProveedor)
        {
            return DbHelper.ExecuteListQuery<FacturaPrecioDetalleDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"select D.Cod_factura,
                         D.cod_producto,
                         P.descripcion,
                         D.cantidad,
                         D.cod_bodega,
                         D.precio,
                         isnull(D.descuento,0) as descuento,
                         D.imp_ventas,
                         0 as Total
                  from cpr_Compras_detalle D
                  inner join pv_productos P on D.cod_producto = P.cod_producto
                  where D.cod_factura = @CodFactura
                    and D.cod_proveedor = @CodProveedor
                  order by D.Linea",
                new
                {
                    CodFactura,
                    CodProveedor
                });
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Actualiza los precios de una factura de compra.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Detalle de precio a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PreciosFactura_Actualiza(int CodEmpresa, FacturaPrecioDetalleDto request)
        {
            if (request.nuevo_precio <= 0)
            {
                return DbHelper.ErrorResponse("El nuevo precio debe ser mayor que cero.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE cpr_Compras_detalle
                  SET PRECIO = @NuevoPrecio
                  WHERE COD_FACTURA = @CodFactura
                    AND cod_producto = @CodProducto",
                new
                {
                    NuevoPrecio = request.nuevo_precio,
                    CodFactura = request.cod_factura,
                    CodProducto = request.cod_producto
                });

            return CrearRespuestaNonQuery(result, "Ok", "Error al actualizar el precio de la factura.");
        }

        /// <summary>
        /// Actualiza o crea productos según la carga masiva de precios.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="precio">Datos del producto a procesar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CambiosPrecios_Actualizar(int CodEmpresa, PrecioExcelDto precio)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                if (string.Equals(precio.no_existe, "NO", StringComparison.OrdinalIgnoreCase))
                {
                    if (precio.cod_producto == "0")
                    {
                        precio.cod_producto = ObtenerSiguienteCodigoProducto(connection);
                    }

                    var tipoProducto = ObtenerTipoProducto(precio);
                    var categoria = ObtenerCodigoCategoria(connection, precio.categoria);
                    var familia = ObtenerCodigoFamilia(connection, precio.familia, categoria);

                    InsertarProductoNuevo(connection, precio, tipoProducto, categoria, familia);
                    return new ErrorDto { Code = 0, Description = "Ok" };
                }

                connection.Execute(
                    @"UPDATE PV_PRODUCTOS
                      SET PRECIO_REGULAR = @PrecioNuevo
                      WHERE COD_PRODUCTO = @CodProducto",
                    new
                    {
                        PrecioNuevo = precio.precio_nuevo,
                        CodProducto = precio.cod_producto
                    });

                return new ErrorDto { Code = 0, Description = "Ok" };
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar los cambios de precios.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}