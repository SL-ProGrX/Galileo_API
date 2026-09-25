using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Globalization;

namespace Galileo.DataBaseTier
{
    public class FrmInvTranReversionDB
    {
        private const int CodigoValidacion = -2;
        private const string EstadoProcesado = "P";
        private const string TipoEntrada = "E";
        private const string TipoSalida = "S";
        private const string TipoTraslado = "T";
        private const string MensajeTipoTransaccionInvalido =
            "El tipo de transacci&oacute;n no es v&aacute;lido.";

        private readonly PortalDB _portalDb;
        private readonly MProGrXAuxiliarDB _auxiliarDb;

        /// <summary>
        /// Agrupa los datos necesarios para afectar el inventario.
        /// </summary>
        private sealed class InvTranReversionAfectacionContext
        {
            public int cod_empresa { get; init; } = 0;

            public IEnumerable<InvProducReversion> productos { get; init; } =
                [];

            public string boleta_inversa { get; init; } = string.Empty;

            public string tipo_inverso { get; init; } = string.Empty;

            public string origen { get; init; } = string.Empty;

            public string fecha_movimiento { get; init; } = string.Empty;

            public string usuario { get; init; } = string.Empty;
        }

        /// <summary>
        /// Inicializa el acceso a datos del formulario de reversión.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTranReversionDB(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _auxiliarDb = new MProGrXAuxiliarDB(config);
        }

        /// <summary>
        /// Obtiene una transacción de inventario para su reversión.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodBoleta">Código de la boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Información de la transacción.</returns>
        public ErrorDto<TranReversionData> InvTranReversion_Obtener(
            int CodEmpresa,
            string CodBoleta,
            string TipoTran)
        {
            string boleta = CodBoleta?.Trim() ?? string.Empty;
            string tipo =
                TipoTran?.Trim().ToUpperInvariant() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(boleta))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo de la boleta es requerido.",
                    CodigoValidacion,
                    new TranReversionData());
            }

            if (!INV_TranReversion_Tipo_Validar(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeTipoTransaccionInvalido,
                    CodigoValidacion,
                    new TranReversionData());
            }

            const string query = """
                SELECT
                    X.boleta,
                    X.tipo,
                    X.cod_entsal,
                    RTRIM(C.cod_entsal) + ' - ' +
                        RTRIM(C.descripcion) AS causa,
                    X.estado,
                    ISNULL(X.plantilla, 0) AS plantilla,
                    ISNULL(X.documento, '') AS documento,
                    ISNULL(X.notas, '') AS notas,
                    X.fecha,
                    ISNULL(X.genera_user, '') AS genera_user,
                    X.genera_fecha,
                    ISNULL(X.autoriza_user, '') AS autoriza_user,
                    X.autoriza_fecha,
                    ISNULL(X.procesa_user, '') AS procesa_user,
                    X.procesa_fecha,
                    ISNULL(X.total, 0) AS total,
                    ISNULL(X.asiento_numero, '') AS asiento_numero
                FROM PV_INVTRANSAC X
                INNER JOIN pv_entrada_salida C
                    ON X.cod_entsal = C.cod_entsal
                WHERE X.boleta = @boleta
                  AND X.tipo = @tipo;
                """;

            ErrorDto<TranReversionData?> respuesta =
                DbHelper.ExecuteSingleQuery(
                    _portalDb,
                    CodEmpresa,
                    query,
                    default(TranReversionData),
                    new
                    {
                        boleta,
                        tipo
                    });

            if (respuesta.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    respuesta.Description ??
                    "Ocurri&oacute; un error al consultar la transacci&oacute;n.",
                    respuesta.Code.GetValueOrDefault(-1),
                    new TranReversionData());
            }

            if (respuesta.Result is null)
            {
                return DbHelper.CreateOkResponse(
                    new TranReversionData(),
                    "No se encontr&oacute; la boleta solicitada.");
            }

            respuesta.Result.estado =
                INV_TranReversion_EstadoDescripcion_Obtener(
                    respuesta.Result.estado);

            return DbHelper.CreateOkResponse(
                respuesta.Result);
        }

        /// <summary>
        /// Obtiene los productos asociados con una transacción.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodBoleta">Código de la boleta.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Productos asociados con la transacción.</returns>
        public ErrorDto<List<InvProducReversion>>
            InvProducLineas_Obtener(
                int CodEmpresa,
                string CodBoleta,
                string TipoTran)
        {
            string boleta = CodBoleta?.Trim() ?? string.Empty;
            string tipo =
                TipoTran?.Trim().ToUpperInvariant() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(boleta))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo de la boleta es requerido.",
                    CodigoValidacion,
                    new List<InvProducReversion>());
            }

            if (!INV_TranReversion_Tipo_Validar(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeTipoTransaccionInvalido,
                    CodigoValidacion,
                    new List<InvProducReversion>());
            }

            const string query = """
                SELECT
                    D.linea,
                    RTRIM(D.cod_producto) AS cod_producto,
                    RTRIM(P.descripcion) AS descripcion,
                    ISNULL(D.cantidad, 0) AS cantidad,
                    RTRIM(D.cod_bodega) AS cod_bodega,
                    RTRIM(B.descripcion) AS bodega,
                    RTRIM(
                        ISNULL(D.cod_bodega_destino, '')
                    ) AS cod_bodega_destino,
                    RTRIM(
                        ISNULL(X.descripcion, '')
                    ) AS bodega_d,
                    ISNULL(D.precio, 0) AS precio,
                    ISNULL(D.cantidad, 0) *
                        ISNULL(D.precio, 0) AS total,
                    ISNULL(D.despacho, 0) AS despacho
                FROM PV_INVTRADET D
                INNER JOIN pv_productos P
                    ON D.cod_producto = P.cod_producto
                INNER JOIN PV_Bodegas B
                    ON D.cod_bodega = B.cod_bodega
                LEFT JOIN PV_Bodegas X
                    ON D.cod_bodega_destino = X.cod_bodega
                WHERE D.boleta = @boleta
                  AND D.tipo = @tipo
                ORDER BY D.linea;
                """;

            ErrorDto<List<InvProducReversion>> respuesta =
                DbHelper.ExecuteListQuery<InvProducReversion>(
                    _portalDb,
                    CodEmpresa,
                    query,
                    new
                    {
                        boleta,
                        tipo
                    });

            return respuesta.Code == 0
                ? DbHelper.CreateOkResponse(
                    respuesta.Result ??
                    new List<InvProducReversion>())
                : DbHelper.CreateErrorResponse(
                    respuesta.Description ??
                    "Ocurri&oacute; un error al consultar los productos de la transacci&oacute;n.",
                    respuesta.Code.GetValueOrDefault(-1),
                    new List<InvProducReversion>());
        }

        /// <summary>
        /// Obtiene la boleta anterior o siguiente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="scrollValue">Dirección de navegación.</param>
        /// <param name="CodBoleta">Boleta actual.</param>
        /// <param name="TipoTran">Tipo de transacción.</param>
        /// <returns>Boleta encontrada.</returns>
        public ErrorDto<TranReversionData>
            InvTranReversion_scroll(
                int CodEmpresa,
                int scrollValue,
                string? CodBoleta,
                string TipoTran)
        {
            string boleta = CodBoleta?.Trim() ?? string.Empty;
            string tipo =
                TipoTran?.Trim().ToUpperInvariant() ??
                string.Empty;

            if (!INV_TranReversion_Tipo_Validar(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeTipoTransaccionInvalido,
                    CodigoValidacion,
                    new TranReversionData());
            }

            if (scrollValue is not 0 and not 1)
            {
                return DbHelper.CreateErrorResponse(
                    "La direcci&oacute;n de navegaci&oacute;n no es v&aacute;lida.",
                    CodigoValidacion,
                    new TranReversionData());
            }

            string query = scrollValue == 1
                ? """
                  SELECT TOP 1
                      boleta
                  FROM pv_invTransac
                  WHERE tipo = @tipo
                    AND boleta > @boleta
                  ORDER BY boleta ASC;
                  """
                : """
                  SELECT TOP 1
                      boleta
                  FROM pv_invTransac
                  WHERE tipo = @tipo
                    AND boleta < @boleta
                  ORDER BY boleta DESC;
                  """;

            ErrorDto<TranReversionData?> respuesta =
                DbHelper.ExecuteSingleQuery(
                    _portalDb,
                    CodEmpresa,
                    query,
                    default(TranReversionData),
                    new
                    {
                        tipo,
                        boleta
                    });

            if (respuesta.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    respuesta.Description ??
                    "Ocurri&oacute; un error al navegar entre las boletas.",
                    respuesta.Code.GetValueOrDefault(-1),
                    new TranReversionData());
            }

            return DbHelper.CreateOkResponse(
                respuesta.Result ??
                new TranReversionData());
        }

        /// <summary>
        /// Genera la transacción inversa y afecta el inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos necesarios para la reversión.</param>
        /// <returns>Resultado de la reversión.</returns>
        public ErrorDto InvTranReversion_Insertar(
            int CodEmpresa,
            TranReversionInsert request)
        {
            ErrorDto validacion =
                INV_TranReversion_Request_Validar(request);

            if (validacion.Code != 0)
            {
                return validacion;
            }

            string boletaOriginal = request.boleta.Trim();
            string tipoOriginal =
                request.tipo.Trim().ToUpperInvariant();
            string usuario = request.usuario.Trim();
            DateTime fechaReversion =
                request.fecha.GetValueOrDefault();

            (string tipoInverso, string origen) =
                INV_TranReversion_DatosInversos_Obtener(
                    tipoOriginal);

            string fechaPeriodo = fechaReversion.ToString(
                "yyyyMMdd HH:mm:ss",
                CultureInfo.InvariantCulture);

            if (!_auxiliarDb.fxInvPeriodos(
                    CodEmpresa,
                    fechaPeriodo))
            {
                return DbHelper.ErrorResponse(
                    "El per&iacute;odo en el que desea realizar el movimiento se encuentra cerrado.",
                    CodigoValidacion);
            }

            ErrorDto<ErrorDto> resultado = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    string estado =
                        INV_TranReversion_Estado_Obtener(
                            connection,
                            tipoOriginal,
                            boletaOriginal);

                    if (string.IsNullOrWhiteSpace(estado))
                    {
                        return DbHelper.ErrorResponse(
                            "No se encontr&oacute; la boleta indicada.",
                            CodigoValidacion);
                    }

                    if (!string.Equals(
                            estado,
                            EstadoProcesado,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return DbHelper.ErrorResponse(
                            "La boleta consultada no se encuentra procesada.",
                            CodigoValidacion);
                    }

                    string boletaInversa =
                        INV_TranReversion_Consecutivo_Obtener(
                            connection,
                            tipoInverso);

                    INV_TranReversion_Encabezado_Insertar(
                        connection,
                        boletaInversa,
                        tipoInverso,
                        request);

                    INV_TranReversion_Detalle_Insertar(
                        connection,
                        boletaInversa,
                        tipoInverso,
                        request);

                    List<InvProducReversion> productos =
                        INV_TranReversion_ProductosProcesar_Obtener(
                            connection,
                            boletaInversa,
                            tipoInverso);

                    if (productos.Count == 0)
                    {
                        return DbHelper.ErrorResponse(
                            "La transacci&oacute;n no contiene productos para efectuar la reversi&oacute;n.",
                            CodigoValidacion);
                    }

                    DateTime fechaAfectacion =
                        fechaReversion.Date.Add(
                            DateTime.Now.TimeOfDay);

                    var contexto =
                        new InvTranReversionAfectacionContext
                        {
                            cod_empresa = CodEmpresa,
                            productos = productos,
                            boleta_inversa = boletaInversa,
                            tipo_inverso = tipoInverso,
                            origen = origen,
                            fecha_movimiento =
                                fechaAfectacion.ToString(
                                    "yyyyMMdd HH:mm:ss",
                                    CultureInfo.InvariantCulture),
                            usuario = usuario
                        };

                    ErrorDto afectacion =
                        INV_TranReversion_Inventario_Afectar(
                            contexto);

                    if (afectacion.Code != 0)
                    {
                        return afectacion;
                    }

                    string descripcion =
                        $"Reversi&oacute;n de '{tipoOriginal}', " +
                        $"boleta: '{boletaOriginal}', realizada con " +
                        $"'{origen}', boleta: '{boletaInversa}'.";

                    return DbHelper.OkResponse(descripcion);
                });

            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ??
                    "Ocurri&oacute; un error al generar la reversi&oacute;n.",
                    resultado.Code.GetValueOrDefault(-1));
            }

            return resultado.Result ??
                   DbHelper.ErrorResponse(
                       "No fue posible determinar el resultado de la reversi&oacute;n.");
        }

        /// <summary>
        /// Valida los datos requeridos para generar una reversión.
        /// </summary>
        /// <param name="request">Datos de la reversión.</param>
        /// <returns>Resultado de la validación.</returns>
        private static ErrorDto
            INV_TranReversion_Request_Validar(
                TranReversionInsert request)
        {
            if (string.IsNullOrWhiteSpace(request.boleta))
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo de la boleta es requerido.",
                    CodigoValidacion);
            }

            string tipo =
                request.tipo?.Trim().ToUpperInvariant() ??
                string.Empty;

            if (!INV_TranReversion_Tipo_Validar(tipo))
            {
                return DbHelper.ErrorResponse(
                    MensajeTipoTransaccionInvalido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(request.cod_entsal))
            {
                return DbHelper.ErrorResponse(
                    "La causa de la reversi&oacute;n es requerida.",
                    CodigoValidacion);
            }

            if (!request.fecha.HasValue)
            {
                return DbHelper.ErrorResponse(
                    "La fecha de la reversi&oacute;n es requerida.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoValidacion);
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Valida los tipos de transacción admitidos.
        /// </summary>
        /// <param name="tipo">Tipo de transacción.</param>
        /// <returns>True cuando el tipo es válido.</returns>
        private static bool INV_TranReversion_Tipo_Validar(
            string tipo)
        {
            return tipo is
                TipoEntrada or
                TipoSalida or
                TipoTraslado;
        }

        /// <summary>
        /// Obtiene el tipo inverso y el origen del movimiento.
        /// </summary>
        /// <param name="tipoOriginal">Tipo original.</param>
        /// <returns>Tipo inverso y origen.</returns>
        private static (string tipoInverso, string origen)
            INV_TranReversion_DatosInversos_Obtener(
                string tipoOriginal)
        {
            return tipoOriginal switch
            {
                TipoEntrada => (TipoSalida, "Salida"),
                TipoSalida => (TipoEntrada, "Entrada"),
                TipoTraslado => (TipoTraslado, "Traslado"),
                _ => (string.Empty, "No Ident.")
            };
        }

        /// <summary>
        /// Obtiene la descripción correspondiente al estado.
        /// </summary>
        /// <param name="estado">Código del estado.</param>
        /// <returns>Descripción del estado.</returns>
        private static string
            INV_TranReversion_EstadoDescripcion_Obtener(
                string? estado)
        {
            return estado?.Trim().ToUpperInvariant() switch
            {
                "S" => "Solicitada",
                "A" => "Autorizada",
                "P" => "Procesada",
                "R" => "Rechazada",
                _ => estado?.Trim() ?? string.Empty
            };
        }

        /// <summary>
        /// Obtiene el estado actual de una boleta.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="tipo">Tipo de transacción.</param>
        /// <param name="boleta">Código de la boleta.</param>
        /// <returns>Estado de la boleta.</returns>
        private static string INV_TranReversion_Estado_Obtener(
            IDbConnection connection,
            string tipo,
            string boleta)
        {
            const string query = """
                SELECT estado
                FROM pv_InvTranSac
                WHERE tipo = @tipo
                  AND boleta = @boleta;
                """;

            return connection
                       .QueryFirstOrDefault<string>(
                           query,
                           new
                           {
                               tipo,
                               boleta
                           })
                       ?.Trim() ??
                   string.Empty;
        }

        /// <summary>
        /// Obtiene el siguiente consecutivo para el tipo inverso.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="tipoInverso">Tipo inverso.</param>
        /// <returns>Boleta con formato de diez dígitos.</returns>
        private static string
            INV_TranReversion_Consecutivo_Obtener(
                IDbConnection connection,
                string tipoInverso)
        {
            const string query = """
                SELECT
                    ISNULL(
                        MAX(CONVERT(decimal(18, 0), boleta)),
                        0
                    ) + 1
                FROM pv_InvTranSac
                WHERE tipo = @tipoInverso;
                """;

            decimal consecutivo =
                connection.QueryFirstOrDefault<decimal>(
                    query,
                    new
                    {
                        tipoInverso
                    });

            return consecutivo
                .ToString(
                    "0",
                    CultureInfo.InvariantCulture)
                .PadLeft(10, '0');
        }

        /// <summary>
        /// Inserta el encabezado de la transacción inversa.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="boletaInversa">Nueva boleta.</param>
        /// <param name="tipoInverso">Tipo inverso.</param>
        /// <param name="request">Datos de la reversión.</param>
        private static void
            INV_TranReversion_Encabezado_Insertar(
                IDbConnection connection,
                string boletaInversa,
                string tipoInverso,
                TranReversionInsert request)
        {
            const string query = """
                INSERT INTO pv_InvTranSac
                (
                    boleta,
                    tipo,
                    cod_entsal,
                    genera_fecha,
                    documento,
                    notas,
                    genera_user,
                    estado,
                    plantilla,
                    fecha,
                    fecha_sistema,
                    autoriza_fecha,
                    autoriza_user,
                    procesa_fecha,
                    procesa_user
                )
                VALUES
                (
                    @boletaInversa,
                    @tipoInverso,
                    @codEntsal,
                    GETDATE(),
                    @documento,
                    @notas,
                    @usuario,
                    'P',
                    0,
                    @fecha,
                    GETDATE(),
                    GETDATE(),
                    @usuario,
                    GETDATE(),
                    @usuario
                );
                """;

            connection.Execute(
                query,
                new
                {
                    boletaInversa,
                    tipoInverso,
                    codEntsal = request.cod_entsal.Trim(),
                    documento =
                        $"Rev.{request.boleta.Trim()}",
                    notas =
                        request.notas?.Trim() ??
                        string.Empty,
                    usuario = request.usuario.Trim(),
                    fecha = request.fecha.GetValueOrDefault()
                });
        }

        /// <summary>
        /// Copia el detalle original a la transacción inversa.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="boletaInversa">Nueva boleta.</param>
        /// <param name="tipoInverso">Tipo inverso.</param>
        /// <param name="request">Datos de la reversión.</param>
        private static void INV_TranReversion_Detalle_Insertar(
            IDbConnection connection,
            string boletaInversa,
            string tipoInverso,
            TranReversionInsert request)
        {
            string query = tipoInverso == TipoTraslado
                ? """
                  INSERT INTO pv_invTraDet
                  (
                      linea,
                      boleta,
                      tipo,
                      cod_bodega,
                      cod_producto,
                      cod_bodega_destino,
                      cantidad,
                      precio,
                      despacho
                  )
                  SELECT
                      linea,
                      @boletaInversa,
                      @tipoInverso,
                      cod_bodega_destino,
                      cod_producto,
                      cod_bodega,
                      cantidad,
                      precio,
                      cantidad
                  FROM pv_invTraDet
                  WHERE tipo = @tipoOriginal
                    AND boleta = @boletaOriginal;
                  """
                : """
                  INSERT INTO pv_invTraDet
                  (
                      linea,
                      boleta,
                      tipo,
                      cod_bodega,
                      cod_producto,
                      cod_bodega_destino,
                      cantidad,
                      precio,
                      despacho
                  )
                  SELECT
                      linea,
                      @boletaInversa,
                      @tipoInverso,
                      cod_bodega,
                      cod_producto,
                      cod_bodega_destino,
                      cantidad,
                      precio,
                      cantidad
                  FROM pv_invTraDet
                  WHERE tipo = @tipoOriginal
                    AND boleta = @boletaOriginal;
                  """;

            connection.Execute(
                query,
                new
                {
                    boletaInversa,
                    tipoInverso,
                    tipoOriginal =
                        request.tipo
                            .Trim()
                            .ToUpperInvariant(),
                    boletaOriginal = request.boleta.Trim()
                });
        }

        /// <summary>
        /// Obtiene los productos copiados a la transacción inversa.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="boletaInversa">Nueva boleta.</param>
        /// <param name="tipoInverso">Tipo inverso.</param>
        /// <returns>Productos que deben afectar el inventario.</returns>
        private static List<InvProducReversion>
            INV_TranReversion_ProductosProcesar_Obtener(
                IDbConnection connection,
                string boletaInversa,
                string tipoInverso)
        {
            const string query = """
                SELECT
                    linea,
                    RTRIM(cod_producto) AS cod_producto,
                    ISNULL(cantidad, 0) AS cantidad,
                    RTRIM(cod_bodega) AS cod_bodega,
                    RTRIM(
                        ISNULL(cod_bodega_destino, '')
                    ) AS cod_bodega_destino,
                    ISNULL(precio, 0) AS precio
                FROM pv_invTraDet
                WHERE tipo = @tipoInverso
                  AND boleta = @boletaInversa
                ORDER BY linea;
                """;

            return connection
                .Query<InvProducReversion>(
                    query,
                    new
                    {
                        tipoInverso,
                        boletaInversa
                    })
                .ToList();
        }

        /// <summary>
        /// Afecta el inventario mediante el auxiliar compartido.
        /// </summary>
        /// <param name="contexto">Datos comunes de la afectación.</param>
        /// <returns>Resultado de la afectación.</returns>
        private ErrorDto
            INV_TranReversion_Inventario_Afectar(
                InvTranReversionAfectacionContext contexto)
        {
            foreach (
                InvProducReversion producto
                in contexto.productos)
            {
                if (contexto.tipo_inverso == TipoTraslado)
                {
                    ErrorDto salida =
                        INV_TranReversion_Producto_Afectar(
                            contexto,
                            producto,
                            producto.cod_bodega,
                            TipoSalida);

                    if (salida.Code != 0)
                    {
                        return salida;
                    }

                    ErrorDto entrada =
                        INV_TranReversion_Producto_Afectar(
                            contexto,
                            producto,
                            producto.cod_bodega_destino,
                            TipoEntrada);

                    if (entrada.Code != 0)
                    {
                        return entrada;
                    }

                    continue;
                }

                ErrorDto afectacion =
                    INV_TranReversion_Producto_Afectar(
                        contexto,
                        producto,
                        producto.cod_bodega,
                        contexto.tipo_inverso);

                if (afectacion.Code != 0)
                {
                    return afectacion;
                }
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Afecta un producto mediante sbInvInventario.
        /// </summary>
        /// <param name="contexto">Datos comunes de la afectación.</param>
        /// <param name="producto">Producto que debe procesarse.</param>
        /// <param name="codBodega">Bodega que debe afectarse.</param>
        /// <param name="tipoMovimiento">Entrada o salida.</param>
        /// <returns>Resultado de la afectación.</returns>
        private ErrorDto
            INV_TranReversion_Producto_Afectar(
                InvTranReversionAfectacionContext contexto,
                InvProducReversion producto,
                string codBodega,
                string tipoMovimiento)
        {
            if (string.IsNullOrWhiteSpace(codBodega))
            {
                return DbHelper.ErrorResponse(
                    "La bodega del producto es requerida.",
                    CodigoValidacion);
            }

            var inventarioRequest =
                new CompraInventarioDto
                {
                    CodProducto = producto.cod_producto,
                    Cantidad = producto.cantidad,
                    CodBodega = codBodega.Trim(),
                    CodTipo = contexto.boleta_inversa,
                    Origen = contexto.origen,
                    Fecha = contexto.fecha_movimiento,
                    Precio = producto.precio,
                    ImpConsumo = 0m,
                    ImpVentas = 0m,
                    TipoMov = tipoMovimiento,
                    Usuario = contexto.usuario
                };

            ErrorDto respuesta =
                _auxiliarDb.sbInvInventario(
                    contexto.cod_empresa,
                    inventarioRequest);

            return respuesta.Code == 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse(
                    respuesta.Description ??
                    "Ocurri&oacute; un error al afectar el inventario.",
                    respuesta.Code.GetValueOrDefault(-1));
        }
    }
}