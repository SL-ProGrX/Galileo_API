using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvCambioPreciosDB
    {
        private const int CodigoValidacion = -2;
        private const int MaximoRegistros = 10000;
        private const decimal IncrementoUtilidad = 0.001m;

        private const string TipoPrecioRequerido =
            "El tipo de precio es requerido.";

        private const string UsuarioRequerido =
            "El usuario es requerido.";

        private const string RegistrosRequeridos =
            "Debe proporcionar al menos un registro.";

        private const string MaximoRegistrosExcedido =
            "La cantidad de registros excede el m&aacute;ximo permitido.";

        private const string CodigoProductoRequerido =
            "El c&oacute;digo del producto es requerido.";

        private const string PrecioMayorCero =
            "El precio debe ser mayor que cero.";

        private const string ProductoNoEncontrado =
            "No se encontr&oacute; el producto indicado.";

        private const string CostoProductoInvalido =
            "El costo regular del producto debe ser mayor que cero.";

        private const string ErrorTiposPrecio =
            "Ocurri&oacute; un error al consultar los tipos de precio.";

        private const string ErrorCargaArchivo =
            "Ocurri&oacute; un error al cargar y validar el archivo de precios.";

        private const string ErrorProcesarArchivo =
            "Ocurri&oacute; un error al procesar los cambios de precio.";

        private const string ErrorDetalleFactura =
            "Ocurri&oacute; un error al consultar el detalle de la factura.";

        private const string ErrorActualizarFactura =
            "Ocurri&oacute; un error al actualizar los precios de venta.";

        private const string SolicitudRequerida = "La solicitud es requerida.";

        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos del formulario de cambio de precios.
        /// </summary>
        /// <param name="config">Configuraci&oacute;n de la aplicaci&oacute;n.</param>
        public FrmInvCambioPreciosDB(
            IConfiguration config)
        {
            _portalDb = new PortalDB(
                config ??
                throw new ArgumentNullException(
                    nameof(config)));
        }

        /// <summary>
        /// Obtiene los tipos de precio disponibles.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de la empresa.</param>
        /// <returns>Listado de tipos de precio.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_CambioPrecios_TiposPrecio_Obtener(
                int CodEmpresa)
        {
            const string query = """
            SELECT
                IdX AS item,
                ItmX AS descripcion
            FROM vInv_Tipos_Precios
            ORDER BY ItmX;
            """;

            var resultado =
                DbHelper.ExecuteListQuery<
                    DropDownListaGenericaModel>(
                        _portalDb,
                        CodEmpresa,
                        query);

            return resultado.Code == 0
                ? DbHelper.CreateOkResponse(
                    resultado.Result ?? [])
                : DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    ErrorTiposPrecio,
                    resultado.Code.GetValueOrDefault(-1),
                    new List<DropDownListaGenericaModel>());
        }

        /// <summary>
        /// Obtiene los proveedores disponibles para buscar facturas.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de la empresa.</param>
        /// <returns>Listado de proveedores.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_CambioPrecios_Proveedores_Obtener(
                int CodEmpresa)
        {
            const string query = """
            SELECT
                cod_proveedor AS item,
                descripcion
            FROM cxp_proveedores
            ORDER BY cod_proveedor;
            """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel>(
                    _portalDb,
                    CodEmpresa,
                    query);
        }

        /// <summary>
        /// Carga los registros del archivo y retorna el resultado validado por el proceso de inventarios.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de la empresa.</param>
        /// <param name="request">Informaci&oacute;n del archivo que se debe cargar.</param>
        /// <returns>Registros validados y totales de la carga.</returns>
        public ErrorDto<CambioPrecioArchivoCargaResponse>
            INV_CambioPrecios_Archivo_Cargar(
                int CodEmpresa,
                CambioPrecioArchivoCargaRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    SolicitudRequerida,
                    CodigoValidacion,
                    new CambioPrecioArchivoCargaResponse());
            }

            string validacion =
                INV_CambioPrecios_Archivo_Validar(
                    request.tipo_precio,
                    request.usuario,
                    request.registros);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    CodigoValidacion,
                    new CambioPrecioArchivoCargaResponse());
            }

            string tipoPrecio =
                request.tipo_precio.Trim();

            string usuario =
                request.usuario.Trim();

            List<CambioPrecioArchivoDetalleDto> registros =
                INV_CambioPrecios_Registros_Normalizar(
                    request.registros);

            var resultado = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    try
                    {
                        INV_CambioPrecios_Archivo_Subir(
                            connection,
                            transaction,
                            tipoPrecio,
                            usuario,
                            registros);

                        List<CambioPrecioArchivoResultadoDto>
                            registrosValidados =
                                INV_CambioPrecios_Archivo_Consultar(
                                    connection,
                                    transaction,
                                    tipoPrecio,
                                    usuario);

                        transaction.Commit();

                        return INV_CambioPrecios_Archivo_Respuesta_Crear(
                            registrosValidados);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            return resultado.Code == 0
                ? DbHelper.CreateOkResponse(
                    resultado.Result ??
                    new CambioPrecioArchivoCargaResponse())
                : DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    ErrorCargaArchivo,
                    resultado.Code.GetValueOrDefault(-1),
                    new CambioPrecioArchivoCargaResponse());
        }

        /// <summary>
        /// Procesa los cambios de precio seleccionados.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de la empresa.</param>
        /// <param name="request">Registros seleccionados y datos del proceso.</param>
        /// <returns>Resultado del procesamiento.</returns>
        public ErrorDto
         INV_CambioPrecios_Archivo_Procesar(
             int CodEmpresa,
             CambioPrecioArchivoProcesarRequest? request)
            {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    SolicitudRequerida,
                    CodigoValidacion);
            }

            string validacion =
                INV_CambioPrecios_Archivo_Validar(
                    request.tipo_precio,
                    request.usuario,
                    request.registros);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            string tipoPrecio =
                request.tipo_precio.Trim();

            string usuario =
                request.usuario.Trim();

            List<CambioPrecioArchivoDetalleDto> registros =
                INV_CambioPrecios_Registros_Normalizar(
                    request.registros);

            var resultado = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    try
                    {
                        const string query = """
                            EXEC spInv_ListaPrecios_Procesa
                                @Codigo,
                                @Precio,
                                @TipoPrecio,
                                @Usuario;
                            """;

                        foreach (
                            CambioPrecioArchivoDetalleDto registro
                            in registros)
                        {
                            connection.Execute(
                                query,
                                new
                                {
                                    Codigo =
                                        registro.codigo,
                                    Precio =
                                        registro.precio,
                                    TipoPrecio =
                                        tipoPrecio,
                                    Usuario =
                                        usuario
                                },
                                transaction);
                        }

                        transaction.Commit();
                        return registros.Count;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            return resultado.Code == 0
                ? DbHelper.OkResponse(
                    "Actualizaci&oacute;n de precios finalizada correctamente.")
                : DbHelper.ErrorResponse(
                    resultado.Description ??
                    ErrorProcesarArchivo,
                    resultado.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene los productos relacionados con una factura de proveedor.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de la empresa.</param>
        /// <param name="codFactura">C&oacute;digo de la factura.</param>
        /// <param name="codProveedor">C&oacute;digo del proveedor.</param>
        /// <returns>Detalle de productos de la factura.</returns>
        public ErrorDto<List<FacturaPrecioDetalleDto>>
            INV_CambioPrecios_Factura_Detalle_Obtener(
                int CodEmpresa,
                string codFactura,
                int codProveedor)
        {
            if (string.IsNullOrWhiteSpace(codFactura))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo de la factura es requerido.",
                    CodigoValidacion,
                    new List<FacturaPrecioDetalleDto>());
            }

            if (codProveedor <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo del proveedor es requerido.",
                    CodigoValidacion,
                    new List<FacturaPrecioDetalleDto>());
            }

            const string query = """
                SELECT
                    P.cod_producto,
                    P.descripcion,
                    ISNULL(P.porc_utilidad, 0)
                        AS porc_utilidad,
                    ISNULL(P.costo_regular, 0)
                        AS costo_regular,
                    ISNULL(P.precio_regular, 0) *
                    (
                        (
                            ISNULL(P.impuesto_ventas, 0) /
                            100.0
                        ) + 1
                    ) AS precio_regular_impuesto,
                    ISNULL(D.precio, 0)
                        AS precio_factura,
                    CAST(0 AS DECIMAL(18, 2))
                        AS nuevo_precio
                FROM Cpr_Compras E
                INNER JOIN Cpr_Compras_detalle D
                    ON E.cod_factura = D.cod_factura
                    AND E.cod_proveedor = D.cod_proveedor
                INNER JOIN PV_PRODUCTOS P
                    ON D.cod_producto = P.cod_producto
                WHERE E.cod_proveedor = @CodProveedor
                    AND E.cod_factura = @CodFactura
                ORDER BY D.linea;
                """;

            var resultado =
                DbHelper.ExecuteListQuery<
                    FacturaPrecioDetalleDto>(
                        _portalDb,
                        CodEmpresa,
                        query,
                        new
                        {
                            CodFactura =
                                codFactura.Trim(),
                            CodProveedor =
                                codProveedor
                        });

            return resultado.Code == 0
                ? DbHelper.CreateOkResponse(
                    resultado.Result ?? [])
                : DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    ErrorDetalleFactura,
                    resultado.Code.GetValueOrDefault(-1),
                    new List<FacturaPrecioDetalleDto>());
        }

        /// <summary>
        /// Actualiza el precio regular y el margen de utilidad de los productos seleccionados.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de la empresa.</param>
        /// <param name="request">Productos y precios nuevos que se deben aplicar.</param>
        /// <returns>Resultado de la actualizaci&oacute;n.</returns>
        public ErrorDto
            INV_CambioPrecios_Factura_Precios_Actualizar(
                int CodEmpresa,
                CambioPrecioFacturaActualizarRequest request)
        {
            string validacion =
                INV_CambioPrecios_Factura_Validar(
                    request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            List<FacturaPrecioActualizarDto> registros =
                request.registros
                    .Select(registro =>
                        new FacturaPrecioActualizarDto
                        {
                            cod_producto =
                                registro.cod_producto.Trim(),
                            nuevo_precio =
                                registro.nuevo_precio
                        })
                    .ToList();

            var resultado = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    try
                    {
                        foreach (
                            FacturaPrecioActualizarDto registro
                            in registros)
                        {
                            INV_CambioPrecios_Producto_Precio_Actualizar(
                                connection,
                                transaction,
                                registro);
                        }

                        transaction.Commit();
                        return registros.Count;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            return resultado.Code == 0
                ? DbHelper.OkResponse(
                    "Los precios de venta fueron actualizados correctamente.")
                : DbHelper.ErrorResponse(
                    resultado.Description ??
                    ErrorActualizarFactura,
                    resultado.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Sube los registros del archivo al proceso temporal de cambio de precios.
        /// </summary>
        /// <param name="connection">Conexi&oacute;n activa.</param>
        /// <param name="transaction">Transacci&oacute;n activa.</param>
        /// <param name="tipoPrecio">Tipo de precio seleccionado.</param>
        /// <param name="usuario">Usuario que realiza la carga.</param>
        /// <param name="registros">Registros obtenidos del archivo.</param>
        private static void
            INV_CambioPrecios_Archivo_Subir(
                IDbConnection connection,
                IDbTransaction transaction,
                string tipoPrecio,
                string usuario,
                IReadOnlyList<CambioPrecioArchivoDetalleDto> registros)
        {
            const string query = """
                EXEC spInv_ListaPrecios_Sube
                    @Codigo,
                    @Precio,
                    @TipoPrecio,
                    @Usuario,
                    @Linea,
                    @Inicializa;
                """;

            for (
                int indice = 0;
                indice < registros.Count;
                indice++)
            {
                CambioPrecioArchivoDetalleDto registro =
                    registros[indice];

                connection.Execute(
                    query,
                    new
                    {
                        Codigo =
                            registro.codigo,
                        Precio =
                            registro.precio,
                        TipoPrecio =
                            tipoPrecio,
                        Usuario =
                            usuario,
                        Linea =
                            indice + 1,
                        Inicializa =
                            indice == 0 ? 1 : 0
                    },
                    transaction);
            }
        }

        /// <summary>
        /// Consulta los registros validados por el proceso de cambio de precios.
        /// </summary>
        /// <param name="connection">Conexi&oacute;n activa.</param>
        /// <param name="transaction">Transacci&oacute;n activa.</param>
        /// <param name="tipoPrecio">Tipo de precio seleccionado.</param>
        /// <param name="usuario">Usuario que realiz&oacute; la carga.</param>
        /// <returns>Registros validados.</returns>
        private static List<CambioPrecioArchivoResultadoDto>
            INV_CambioPrecios_Archivo_Consultar(
                IDbConnection connection,
                IDbTransaction transaction,
                string tipoPrecio,
                string usuario)
        {
            const string query = """
                EXEC spInv_ListaPrecios_Consulta
                    @TipoPrecio,
                    @Usuario,
                    @Modo;
                """;

            return connection
                .Query<CambioPrecioArchivoResultadoDto>(
                    query,
                    new
                    {
                        TipoPrecio =
                            tipoPrecio,
                        Usuario =
                            usuario,
                        Modo = 1
                    },
                    transaction)
                .ToList();
        }

        /// <summary>
        /// Crea la respuesta de la carga con los totales calculados.
        /// </summary>
        /// <param name="registros">Registros validados por el proceso.</param>
        /// <returns>Respuesta de carga.</returns>
        private static CambioPrecioArchivoCargaResponse
            INV_CambioPrecios_Archivo_Respuesta_Crear(
                List<CambioPrecioArchivoResultadoDto> registros)
        {
            List<CambioPrecioArchivoResultadoDto>
                existentes = registros
                    .Where(registro =>
                        !string.Equals(
                            registro.detalle,
                            "-1",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            return new CambioPrecioArchivoCargaResponse
            {
                registros = registros,
                casos = registros.Count,
                no_existen =
                    registros.Count -
                    existentes.Count,
                monto_total =
                    existentes.Sum(
                        registro =>
                            registro.monto_01)
            };
        }

        /// <summary>
        /// Actualiza el precio regular y la utilidad de un producto.
        /// </summary>
        /// <param name="connection">Conexi&oacute;n activa.</param>
        /// <param name="transaction">Transacci&oacute;n activa.</param>
        /// <param name="registro">Producto y precio nuevo.</param>
        private static void
            INV_CambioPrecios_Producto_Precio_Actualizar(
                IDbConnection connection,
                IDbTransaction transaction,
                FacturaPrecioActualizarDto registro)
        {
            const string queryProducto = """
                SELECT
                    ISNULL(costo_regular, 0)
                        AS costo_regular,
                    ISNULL(porc_utilidad, 0)
                        AS porc_utilidad,
                    (
                        (
                            ISNULL(impuesto_ventas, 0) /
                            100.0
                        ) + 1
                    ) AS factor_impuesto
                FROM PV_PRODUCTOS
                WHERE cod_producto = @CodProducto;
                """;

            ProductoPrecioCalculoDto? producto =
                connection.QueryFirstOrDefault<
                    ProductoPrecioCalculoDto>(
                        queryProducto,
                        new
                        {
                            CodProducto =
                                registro.cod_producto
                        },
                        transaction);

            if (producto is null)
            {
                throw new InvalidOperationException(
                    $"{ProductoNoEncontrado} " +
                    $"Producto: {registro.cod_producto}.");
            }

            if (producto.costo_regular <= 0)
            {
                throw new InvalidOperationException(
                    $"{CostoProductoInvalido} " +
                    $"Producto: {registro.cod_producto}.");
            }

            decimal nuevaUtilidad =
                INV_CambioPrecios_Utilidad_Calcular(
                    producto,
                    registro.nuevo_precio);

            decimal precioRegular =
                producto.costo_regular *
                (
                    1 +
                    (
                        nuevaUtilidad /
                        100
                    )
                );

            const string queryActualizar = """
                UPDATE PV_PRODUCTOS
                SET
                    precio_regular = @PrecioRegular,
                    porc_utilidad = @PorcUtilidad
                WHERE cod_producto = @CodProducto;
                """;

            int actualizados =
                connection.Execute(
                    queryActualizar,
                    new
                    {
                        PrecioRegular =
                            precioRegular,
                        PorcUtilidad =
                            nuevaUtilidad,
                        CodProducto =
                            registro.cod_producto
                    },
                    transaction);

            if (actualizados != 1)
            {
                throw new InvalidOperationException(
                    $"{ProductoNoEncontrado} " +
                    $"Producto: {registro.cod_producto}.");
            }
        }

        /// <summary>
        /// Calcula el margen de utilidad necesario para superar el precio solicitado.
        /// </summary>
        /// <param name="producto">Datos actuales del producto.</param>
        /// <param name="precioNuevo">Precio nuevo con impuesto.</param>
        /// <returns>Margen de utilidad calculado.</returns>
        private static decimal
            INV_CambioPrecios_Utilidad_Calcular(
                ProductoPrecioCalculoDto producto,
                decimal precioNuevo)
        {
            decimal precioCalculado =
                INV_CambioPrecios_PrecioImpuesto_Calcular(
                    producto.costo_regular,
                    producto.porc_utilidad,
                    producto.factor_impuesto);

            if (precioCalculado > precioNuevo)
            {
                return producto.porc_utilidad;
            }

            decimal utilidadRequerida =
                (
                    (
                        precioNuevo /
                        (
                            producto.costo_regular *
                            producto.factor_impuesto
                        )
                    ) - 1
                ) * 100;

            decimal diferencia =
                utilidadRequerida -
                producto.porc_utilidad;

            decimal pasos =
                Math.Floor(
                    diferencia /
                    IncrementoUtilidad) + 1;

            return producto.porc_utilidad +
                (
                    pasos *
                    IncrementoUtilidad
                );
        }

        /// <summary>
        /// Calcula el precio de venta con impuesto.
        /// </summary>
        /// <param name="costoRegular">Costo regular del producto.</param>
        /// <param name="porcUtilidad">Porcentaje de utilidad.</param>
        /// <param name="factorImpuesto">Factor del impuesto de venta.</param>
        /// <returns>Precio calculado con impuesto.</returns>
        private static decimal
            INV_CambioPrecios_PrecioImpuesto_Calcular(
                decimal costoRegular,
                decimal porcUtilidad,
                decimal factorImpuesto)
        {
            return costoRegular *
                (
                    1 +
                    (
                        porcUtilidad /
                        100
                    )
                ) *
                factorImpuesto;
        }

        /// <summary>
        /// Valida la solicitud de carga o procesamiento del archivo.
        /// </summary>
        /// <param name="tipoPrecio">Tipo de precio seleccionado.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="registros">Registros por procesar.</param>
        /// <returns>Mensaje de validaci&oacute;n.</returns>
        private static string
            INV_CambioPrecios_Archivo_Validar(
                string? tipoPrecio,
                string? usuario,
                IReadOnlyCollection<CambioPrecioArchivoDetalleDto>?
                    registros)
        {
            if (string.IsNullOrWhiteSpace(tipoPrecio))
            {
                return TipoPrecioRequerido;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return UsuarioRequerido;
            }

            if (
                registros is null ||
                registros.Count == 0)
            {
                return RegistrosRequeridos;
            }

            if (registros.Count > MaximoRegistros)
            {
                return MaximoRegistrosExcedido;
            }

            foreach (
                CambioPrecioArchivoDetalleDto registro
                in registros)
            {
                if (
                    registro is null ||
                    string.IsNullOrWhiteSpace(
                        registro.codigo))
                {
                    return CodigoProductoRequerido;
                }

                if (registro.precio <= 0)
                {
                    return PrecioMayorCero;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Valida los registros de precios de factura.
        /// </summary>
        /// <param name="request">Solicitud de actualizaci&oacute;n.</param>
        /// <returns>Mensaje de validaci&oacute;n.</returns>
        private static string
            INV_CambioPrecios_Factura_Validar(
                CambioPrecioFacturaActualizarRequest?
                    request)
        {
            if (
                request?.registros is null ||
                request.registros.Count == 0)
            {
                return RegistrosRequeridos;
            }

            if (
                request.registros.Count >
                MaximoRegistros)
            {
                return MaximoRegistrosExcedido;
            }

            foreach (
                FacturaPrecioActualizarDto registro
                in request.registros)
            {
                if (
                    registro is null ||
                    string.IsNullOrWhiteSpace(
                        registro.cod_producto))
                {
                    return CodigoProductoRequerido;
                }

                if (registro.nuevo_precio <= 0)
                {
                    return PrecioMayorCero;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Normaliza los registros provenientes del archivo.
        /// </summary>
        /// <param name="registros">Registros originales.</param>
        /// <returns>Registros normalizados.</returns>
        private static List<CambioPrecioArchivoDetalleDto>
            INV_CambioPrecios_Registros_Normalizar(
                IEnumerable<CambioPrecioArchivoDetalleDto>
                    registros)
        {
            return registros
                .Select(registro =>
                    new CambioPrecioArchivoDetalleDto
                    {
                        codigo =
                            registro.codigo.Trim(),
                        precio =
                            registro.precio
                    })
                .ToList();
        }

        private sealed class ProductoPrecioCalculoDto
        {
            public decimal costo_regular { get; set; } = 0m;
            public decimal porc_utilidad { get; set; } = 0m;
            public decimal factor_impuesto { get; set; } = 1m;
        }
    }
}