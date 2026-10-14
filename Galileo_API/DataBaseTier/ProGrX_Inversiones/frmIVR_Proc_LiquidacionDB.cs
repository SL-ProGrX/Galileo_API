using System.Data;
using System.Globalization;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrProcLiquidacionDb
    {
        private const int CodigoMensajeControlado = -2;

        private const int LongitudDocumento = 30;
        private const int LongitudDivisa = 10;
        private const int LongitudNotas = 500;
        private const int LongitudUsuario = 30;

        private const string TipoReferencia = "L";
        private const string ConceptoLiquidacion = "Liq";
        private const string TipoMovimientoLiquidacion = "L";

        private const string EstadoSolicitud = "S";
        private const string EstadoAplicado = "A";
        private const string EstadoAnulado = "X";

        private const string MensajeInversionRequerida =
            "El n&uacute;mero de inversi&oacute;n es requerido.";

        private const string MensajeInversionNoExiste =
            "No se localiz&oacute; la inversi&oacute;n indicada.";

        private const string MensajeInversionNoActiva =
            "Consulte una inversi&oacute;n que se encuentre activa.";

        private const string MensajeDivisaRequerida =
            "La divisa de la inversi&oacute;n es requerida.";

        private const string MensajeTipoCambioInvalido =
            "El tipo de cambio no es v&aacute;lido.";

        private const string MensajeTransaccionRequerida =
            "El n&uacute;mero de transacci&oacute;n es requerido.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private const string MensajeDocumentoRequerido =
            "No ha indicado un n&uacute;mero de documento "
            + "para esta transacci&oacute;n.";

        private const string MensajeMontoRequerido =
            "No se ha indicado ning&uacute;n valor "
            + "para la transacci&oacute;n.";

        private const string MensajeFechaInvalida =
            "La fecha de esta transacci&oacute;n es menor "
            + "al &uacute;ltimo corte.";

        private const string MensajeInteresesInvalidos =
            "Los intereses al corte de la liquidaci&oacute;n "
            + "no pueden ser negativos.";

        private const string MensajeImportePendiente =
            "El importe de la liquidaci&oacute;n no ha sido "
            + "registrado completamente.";

        private readonly PortalDB _portalDb;

        public FrmIvrProcLiquidacionDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Inicializa la liquidacion a partir del numero de inversion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<IvrProcLiquidacionInicialData>
            IVR_Proc_Liquidacion_Inicializar(
                int codEmpresa,
                IvrProcLiquidacionInicializarRequest? request)
        {
            var validation =
                IVR_Proc_Liquidacion_Inicializar_Validar(request);

            if (validation != null)
            {
                return validation;
            }

            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    IVR_Proc_Liquidacion_InicialData_Obtener(
                        connection,
                        request!.titulo_id!.Value));

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionInicialData>(
                        response.Description ??
                        "Ocurri&oacute; un error al inicializar "
                        + "la liquidaci&oacute;n.");
            }

            var result = response.Result;

            if (result?.inversion == null)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionInicialData>(
                        MensajeInversionNoExiste,
                        CodigoMensajeControlado);
            }

            if (!string.Equals(
                    result.inversion.estado,
                    EstadoAplicado,
                    StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionInicialData>(
                        MensajeInversionNoActiva,
                        CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(result.divisa))
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionInicialData>(
                        MensajeDivisaRequerida,
                        CodigoMensajeControlado);
            }

            if (result.tipo_cambio == 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionInicialData>(
                        MensajeTipoCambioInvalido,
                        CodigoMensajeControlado);
            }

            return response;
        }

        /// <summary>
        /// Obtiene los movimientos bancarios de una liquidacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="transacId"></param>
        /// <returns></returns>
        public ErrorDto<IvrProcLiquidacionTransaccionesData>
            IVR_Proc_Liquidacion_Transacciones_Obtener(
                int codEmpresa,
                int transacId)
        {
            if (transacId <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionTransaccionesData>(
                        MensajeTransaccionRequerida,
                        CodigoMensajeControlado);
            }

            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    var transacciones =
                        IVR_Proc_Liquidacion_Transacciones_Consultar(
                            connection,
                            transacId);

                    return new IvrProcLiquidacionTransaccionesData
                    {
                        transacciones = transacciones,
                        registrado = transacciones.Sum(
                            item => item.importe)
                    };
                });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionTransaccionesData>(
                        response.Description ??
                        "Ocurri&oacute; un error al consultar "
                        + "los registros bancarios.");
            }

            return response;
        }

        /// <summary>
        /// Guarda una nueva solicitud de liquidacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Solicitud_Guardar(
                int codEmpresa,
                IvrProcLiquidacionGuardarRequest? request)
        {
            var validation =
                IVR_Proc_Liquidacion_Request_Validar(
                    request,
                    requiereDocumento: false,
                    requiereTransaccion: false);

            if (validation != null)
            {
                return validation;
            }

            return IVR_Proc_Liquidacion_Registrar(
                codEmpresa,
                request!,
                EstadoSolicitud);
        }

        /// <summary>
        /// Procesa definitivamente una solicitud de liquidacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Procesar(
                int codEmpresa,
                IvrProcLiquidacionGuardarRequest? request)
        {
            var validation =
                IVR_Proc_Liquidacion_Request_Validar(
                    request,
                    requiereDocumento: true,
                    requiereTransaccion: true);

            if (validation != null)
            {
                return validation;
            }

            var processValidation =
                IVR_Proc_Liquidacion_Proceso_Validar(
                    codEmpresa,
                    request!);

            if (processValidation != null)
            {
                return processValidation;
            }

            return IVR_Proc_Liquidacion_Registrar(
                codEmpresa,
                request!,
                EstadoAplicado);
        }

        /// <summary>
        /// Anula una solicitud de liquidacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Eliminar(
                int codEmpresa,
                IvrProcLiquidacionGuardarRequest? request)
        {
            var validation =
                IVR_Proc_Liquidacion_Request_Validar(
                    request,
                    requiereDocumento: false,
                    requiereTransaccion: true);

            if (validation != null)
            {
                return validation;
            }

            return IVR_Proc_Liquidacion_Registrar(
                codEmpresa,
                request!,
                EstadoAnulado);
        }

        /// <summary>
        /// Elimina un movimiento bancario de una liquidacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            IVR_Proc_Liquidacion_Transaccion_Eliminar(
                int codEmpresa,
                IvrProcLiquidacionTransaccionEliminarRequest?
                    request)
        {
            var validation =
                IVR_Proc_Liquidacion_Transaccion_Eliminar_Validar(
                    request);

            if (validation != null)
            {
                return validation;
            }

            const string sql = """
                DELETE FROM IVR_TRANSACCIONES
                WHERE TRANSAC_ID = @transaccion_id
                  AND REF_TIPO = @ref_tipo
                  AND REF_CODIGO = @ref_codigo
                  AND REF_CONCEPTO = @ref_concepto;
                """;

            var response = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    transaccion_id =
                        request!.transaccion_id!.Value,
                    ref_tipo = TipoReferencia,
                    ref_codigo =
                        request.liquidacion_transac_id!.Value
                            .ToString(
                                CultureInfo.InvariantCulture),
                    ref_concepto = ConceptoLiquidacion
                });

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurri&oacute; un error al eliminar "
                    + "el registro bancario.");
            }

            if (response.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    "No se localiz&oacute; el registro "
                    + "bancario indicado.",
                    CodigoMensajeControlado);
            }

            return DbHelper.OkResponse(
                "Registro bancario eliminado "
                + "satisfactoriamente.");
        }

        /// <summary>
        /// Obtiene la informacion inicial de la pantalla.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tituloId"></param>
        /// <returns></returns>
        private static IvrProcLiquidacionInicialData
            IVR_Proc_Liquidacion_InicialData_Obtener(
                SqlConnection connection,
                int tituloId)
        {
            var inversion =
                IVR_Proc_Liquidacion_Inversion_Obtener(
                    connection,
                    tituloId);

            var divisaLocal =
                IVR_Proc_Liquidacion_DivisaLocal_Obtener(
                    connection);

            var ultimoCorte =
                IVR_Proc_Liquidacion_UltimoCorte_Consultar(
                    connection);

            var calculo =
                IVR_Proc_Liquidacion_Calculo_Obtener(
                    connection,
                    tituloId,
                    ultimoCorte);

            var divisaInversion =
                inversion?.cod_divisa.Trim().ToUpperInvariant()
                ?? string.Empty;

            var tipoCambio =
                IVR_Proc_Liquidacion_TipoCambio_Obtener(
                    divisaLocal,
                    inversion);

            var importeLocal =
                tipoCambio != 0
                    ? IVR_Proc_Liquidacion_ImporteLocal_Calcular(
                        calculo.mnt_total,
                        tipoCambio)
                    : 0;

            return new IvrProcLiquidacionInicialData
            {
                inversion = inversion,
                liquidacion = calculo,
                fecha_ultimo_corte = ultimoCorte,
                divisa_local = divisaLocal,
                divisa = divisaInversion,
                tipo_cambio = tipoCambio,
                importe_local = importeLocal,
                registrado = 0,
                pendiente = importeLocal,
                transac_id = 0,
                estado = EstadoSolicitud,
                estado_descripcion = "Solicitud"
            };
        }

        /// <summary>
        /// Obtiene la informacion principal de una inversion.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tituloId"></param>
        /// <returns></returns>
        private static IvrProcLiquidacionInversionData?
            IVR_Proc_Liquidacion_Inversion_Obtener(
                SqlConnection connection,
                int tituloId)
        {
            const string sql = """
                SELECT TOP (1)
                    TITULO_ID AS titulo_id,
                    LTRIM(RTRIM(
                        ISNULL(Administrador_Desc, '')))
                        AS administrador_desc,
                    LTRIM(RTRIM(
                        ISNULL(Cod_Administrador, '')))
                        AS cod_administrador,
                    LTRIM(RTRIM(
                        ISNULL(Instrumento_Desc, '')))
                        AS instrumento_desc,
                    LTRIM(RTRIM(
                        ISNULL(Cod_Instrumento, '')))
                        AS cod_instrumento,
                    LTRIM(RTRIM(
                        ISNULL(Portafolio_Desc, '')))
                        AS portafolio_desc,
                    LTRIM(RTRIM(
                        ISNULL(Cod_Portafolio, '')))
                        AS cod_portafolio,
                    LTRIM(RTRIM(
                        ISNULL(Cod_Divisa, '')))
                        AS cod_divisa,
                    ISNULL(Tipo_Cambio, 0)
                        AS tipo_cambio,
                    LTRIM(RTRIM(
                        ISNULL(Estado, '')))
                        AS estado,
                    LTRIM(RTRIM(
                        ISNULL(Estado_Desc, '')))
                        AS estado_desc
                FROM vIVR_INVERSIONES
                WHERE TITULO_ID = @titulo_id;
                """;

            return connection.QueryFirstOrDefault
                <IvrProcLiquidacionInversionData>(
                    sql,
                    new
                    {
                        titulo_id = tituloId
                    });
        }

        /// <summary>
        /// Obtiene la divisa local configurada.
        /// </summary>
        private static string
            IVR_Proc_Liquidacion_DivisaLocal_Obtener(
                SqlConnection connection)
        {
            const string sql = """
                SELECT TOP (1)
                    LTRIM(RTRIM(COD_DIVISA))
                FROM vSys_Divisas
                WHERE DIVISA_LOCAL = 1;
                """;

            return connection.QueryFirstOrDefault<string>(sql)
                   ?? string.Empty;
        }

        /// <summary>
        /// Obtiene la fecha del último cierre de inversiones.
        /// </summary>
        private static DateTime
            IVR_Proc_Liquidacion_UltimoCorte_Consultar(
                SqlConnection connection)
        {
            const string sql = """
                SELECT ISNULL(
                    MAX(CORTE),
                    dbo.MyGetdate())
                FROM IVR_CIERRES;
                """;

            return connection.QueryFirst<DateTime>(sql);
        }

        /// <summary>
        /// Ejecuta el cálculo de liquidación utilizado por VB6.
        /// </summary>
        private static IvrProcLiquidacionCalculoData
            IVR_Proc_Liquidacion_Calculo_Obtener(
                SqlConnection connection,
                int tituloId,
                DateTime ultimoCorte)
        {
            var calculo =
                connection.QueryFirstOrDefault
                    <IvrProcLiquidacionCalculoData>(
                        "spIVR_TITULOS_L_CONSULTA",
                        new
                        {
                            TituloId = tituloId
                        },
                        commandType:
                            CommandType.StoredProcedure);

            return calculo ??
                   new IvrProcLiquidacionCalculoData
                   {
                       fecha_corte = ultimoCorte.AddDays(1)
                   };
        }

        /// <summary>
        /// Determina el tipo de cambio aplicable.
        /// </summary>
        private static decimal
            IVR_Proc_Liquidacion_TipoCambio_Obtener(
                string divisaLocal,
                IvrProcLiquidacionInversionData? inversion)
        {
            if (inversion == null)
            {
                return 0;
            }

            if (string.Equals(
                    divisaLocal.Trim(),
                    inversion.cod_divisa.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            return inversion.tipo_cambio;
        }

        /// <summary>
        /// Calcula el importe en moneda local.
        /// </summary>
        private static decimal
            IVR_Proc_Liquidacion_ImporteLocal_Calcular(
                decimal monto,
                decimal tipoCambio)
        {
            var factorTipoCambio =
                Convert.ToDecimal(
                    MProGrxMain.fxSys_Tipo_Cambio_Apl(
                        tipoCambio));

            return decimal.Round(
                monto * factorTipoCambio,
                2,
                MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Consulta los movimientos bancarios de la liquidación.
        /// </summary>
        private static List<IvrProcLiquidacionTransaccionData>
            IVR_Proc_Liquidacion_Transacciones_Consultar(
                SqlConnection connection,
                int transacId)
        {
            const string sql = """
                SELECT
                    TRANSAC_ID AS transac_id,
                    LTRIM(RTRIM(
                        ISNULL(Tipo_Mov_Desc, '')))
                        AS tipo_mov_desc,
                    Registro_Fecha AS registro_fecha,
                    ISNULL(Importe, 0) AS importe,
                    LTRIM(RTRIM(
                        ISNULL(cta_Mask, '')))
                        AS cta_mask,
                    LTRIM(RTRIM(
                        ISNULL(Tipo_Doc_Desc, '')))
                        AS tipo_doc_desc,
                    LTRIM(RTRIM(
                        ISNULL(Detalle, '')))
                        AS detalle,
                    TES_NSOLICITUD AS tes_nsolicitud,
                    LTRIM(RTRIM(
                        ISNULL(Tes_Estado_Desc, '')))
                        AS tes_estado_desc,
                    LTRIM(RTRIM(
                        ISNULL(Cod_Divisa, '')))
                        AS cod_divisa,
                    ISNULL(Tipo_Cambio, 0)
                        AS tipo_cambio,
                    ISNULL(Monto, 0) AS monto
                FROM vIVR_TRANSACCIONES
                WHERE REF_TIPO = @ref_tipo
                  AND REF_CODIGO = @ref_codigo
                  AND REF_CONCEPTO = @ref_concepto
                ORDER BY TRANSAC_ID;
                """;

            return connection.Query
                <IvrProcLiquidacionTransaccionData>(
                    sql,
                    new
                    {
                        ref_tipo = TipoReferencia,
                        ref_codigo = transacId.ToString(
                            CultureInfo.InvariantCulture),
                        ref_concepto = ConceptoLiquidacion
                    })
                .ToList();
        }

        /// <summary>
        /// Guarda, procesa o anula una liquidación.
        /// </summary>
        private ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Registrar(
                int codEmpresa,
                IvrProcLiquidacionGuardarRequest request,
                string estado)
        {
            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    IVR_Proc_Liquidacion_Registro_Ejecutar(
                        connection,
                        request,
                        estado));

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionGuardarData>(
                        response.Description ??
                        "Ocurri&oacute; un error al registrar "
                        + "la liquidaci&oacute;n.");
            }

            if (estado == EstadoAnulado)
            {
                return new ErrorDto
                    <IvrProcLiquidacionGuardarData>
                {
                    Code = 0,
                    Description =
                        "Liquidaci&oacute;n eliminada "
                        + "satisfactoriamente.",
                    Result =
                        IVR_Proc_Liquidacion_Resultado_Crear(
                            request.transac_id ?? 0,
                            EstadoAnulado)
                };
            }

            if (response.Result == null ||
                response.Result.transac_id <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionGuardarData>(
                        "El proceso no devolvi&oacute; el "
                        + "n&uacute;mero de transacci&oacute;n.");
            }

            var estadoResultado =
                string.IsNullOrWhiteSpace(
                    response.Result.estado)
                    ? estado
                    : response.Result.estado;

            response.Result.estado = estadoResultado;
            response.Result.estado_descripcion =
                IVR_Proc_Liquidacion_Estado_Descripcion_Obtener(
                    estadoResultado);

            response.Description =
                estado == EstadoAplicado
                    ? "Transacci&oacute;n procesada "
                      + "satisfactoriamente."
                    : "Solicitud registrada "
                      + "satisfactoriamente.";

            return response;
        }

        /// <summary>
        /// Ejecuta el procedimiento de registro.
        /// </summary>
        private static IvrProcLiquidacionGuardarData?
            IVR_Proc_Liquidacion_Registro_Ejecutar(
                SqlConnection connection,
                IvrProcLiquidacionGuardarRequest request,
                string estado)
        {
            return connection.QueryFirstOrDefault
                <IvrProcLiquidacionGuardarData>(
                    "spIVR_TITULOS_L_REGISTRA",
                    new
                    {
                        TituloId =
                            request.titulo_id!.Value,
                        TipoMov =
                            TipoMovimientoLiquidacion,
                        Concepto =
                            ConceptoLiquidacion,
                        Fecha =
                            request.fecha!.Value,
                        Documento =
                            IVR_Proc_Liquidacion_Texto_Normalizar(
                                request.documento),
                        Total =
                            request.total!.Value,
                        IntAcum =
                            request.int_acum!.Value,
                        Intereses =
                            request.intereses!.Value,
                        Principal =
                            request.principal!.Value,
                        Divisa =
                            IVR_Proc_Liquidacion_Texto_Normalizar(
                                request.divisa)
                                .ToUpperInvariant(),
                        TipoCambio =
                            request.tipo_cambio!.Value,
                        Notas =
                            IVR_Proc_Liquidacion_Texto_Normalizar(
                                request.notas),
                        Usuario =
                            IVR_Proc_Liquidacion_Texto_Normalizar(
                                request.usuario)
                                .ToUpperInvariant(),
                        Estado = estado,
                        TransacId =
                            request.transac_id ?? 0
                    },
                    commandType:
                        CommandType.StoredProcedure);
        }

        /// <summary>
        /// Valida las condiciones necesarias para procesar.
        /// </summary>
        private ErrorDto<IvrProcLiquidacionGuardarData>?
            IVR_Proc_Liquidacion_Proceso_Validar(
                int codEmpresa,
                IvrProcLiquidacionGuardarRequest request)
        {
            var validationResponse = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    var ultimoCorte =
                        IVR_Proc_Liquidacion_UltimoCorte_Consultar(
                            connection);

                    var registrado =
                        IVR_Proc_Liquidacion_Registrado_Consultar(
                            connection,
                            request.transac_id!.Value);

                    return (
                        ultimoCorte,
                        registrado);
                });

            if (validationResponse.Code != 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionGuardarData>(
                        validationResponse.Description ??
                        "Ocurri&oacute; un error al validar "
                        + "la liquidaci&oacute;n.");
            }

            if (request.fecha!.Value <
                validationResponse.Result.ultimoCorte)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionGuardarData>(
                        MensajeFechaInvalida,
                        CodigoMensajeControlado);
            }

            if (request.intereses!.Value < 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionGuardarData>(
                        MensajeInteresesInvalidos,
                        CodigoMensajeControlado);
            }

            var importeLocal =
                IVR_Proc_Liquidacion_ImporteLocal_Calcular(
                    request.total!.Value,
                    request.tipo_cambio!.Value);

            var registrado = decimal.Round(
                validationResponse.Result.registrado,
                2,
                MidpointRounding.AwayFromZero);

            if (importeLocal != registrado)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionGuardarData>(
                        MensajeImportePendiente,
                        CodigoMensajeControlado);
            }

            return null;
        }

        /// <summary>
        /// Obtiene el monto registrado en movimientos bancarios.
        /// </summary>
        private static decimal
            IVR_Proc_Liquidacion_Registrado_Consultar(
                SqlConnection connection,
                int transacId)
        {
            const string sql = """
                SELECT ISNULL(
                    SUM(ISNULL(Importe, 0)),
                    0)
                FROM vIVR_TRANSACCIONES
                WHERE REF_TIPO = @ref_tipo
                  AND REF_CODIGO = @ref_codigo
                  AND REF_CONCEPTO = @ref_concepto;
                """;

            return connection.QueryFirst<decimal>(
                sql,
                new
                {
                    ref_tipo = TipoReferencia,
                    ref_codigo = transacId.ToString(
                        CultureInfo.InvariantCulture),
                    ref_concepto = ConceptoLiquidacion
                });
        }

        /// <summary>
        /// Valida el request de inicialización.
        /// </summary>
        private static ErrorDto<IvrProcLiquidacionInicialData>?
            IVR_Proc_Liquidacion_Inicializar_Validar(
                IvrProcLiquidacionInicializarRequest? request)
        {
            if (request?.titulo_id is null or <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcLiquidacionInicialData>(
                        MensajeInversionRequerida,
                        CodigoMensajeControlado);
            }

            return null;
        }

        /// <summary>
        /// Valida el request utilizado para registrar.
        /// </summary>
        private static ErrorDto<IvrProcLiquidacionGuardarData>?
            IVR_Proc_Liquidacion_Request_Validar(
                IvrProcLiquidacionGuardarRequest? request,
                bool requiereDocumento,
                bool requiereTransaccion)
        {
            var validation =
                IVR_Proc_Liquidacion_Requeridos_Validar(
                    request,
                    requiereDocumento,
                    requiereTransaccion);

            if (validation != null)
            {
                return validation;
            }

            return IVR_Proc_Liquidacion_Longitudes_Validar(
                request!);
        }

        /// <summary>
        /// Valida los campos obligatorios del registro.
        /// </summary>
        private static ErrorDto<IvrProcLiquidacionGuardarData>?
            IVR_Proc_Liquidacion_Requeridos_Validar(
                IvrProcLiquidacionGuardarRequest? request,
                bool requiereDocumento,
                bool requiereTransaccion)
        {
            if (request?.titulo_id is null or <= 0)
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    MensajeInversionRequerida);
            }

            if (requiereTransaccion &&
                request.transac_id is null or <= 0)
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    MensajeTransaccionRequerida);
            }

            if (request.fecha == null)
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    "La fecha de la transacci&oacute;n "
                    + "es requerida.");
            }

            if (requiereDocumento &&
                string.IsNullOrWhiteSpace(request.documento))
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    MensajeDocumentoRequerido);
            }

            if (request.total is null or <= 0)
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    MensajeMontoRequerido);
            }

            if (request.int_acum == null ||
                request.intereses == null ||
                request.principal == null)
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    "Los montos de la liquidaci&oacute;n "
                    + "son requeridos.");
            }

            if (string.IsNullOrWhiteSpace(request.divisa))
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    MensajeDivisaRequerida);
            }

            if (request.tipo_cambio is null or 0)
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    MensajeTipoCambioInvalido);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    MensajeUsuarioRequerido);
            }

            return null;
        }

        /// <summary>
        /// Valida las longitudes aceptadas por el procedimiento.
        /// </summary>
        private static ErrorDto<IvrProcLiquidacionGuardarData>?
            IVR_Proc_Liquidacion_Longitudes_Validar(
                IvrProcLiquidacionGuardarRequest request)
        {
            var documento =
                IVR_Proc_Liquidacion_Texto_Normalizar(
                    request.documento);

            var divisa =
                IVR_Proc_Liquidacion_Texto_Normalizar(
                    request.divisa);

            var notas =
                IVR_Proc_Liquidacion_Texto_Normalizar(
                    request.notas);

            var usuario =
                IVR_Proc_Liquidacion_Texto_Normalizar(
                    request.usuario);

            if (documento.Length > LongitudDocumento)
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    "El documento no puede superar "
                    + $"{LongitudDocumento} caracteres.");
            }

            if (divisa.Length > LongitudDivisa)
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    "La divisa indicada no es v&aacute;lida.");
            }

            if (notas.Length > LongitudNotas)
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    "Las notas no pueden superar "
                    + $"{LongitudNotas} caracteres.");
            }

            if (usuario.Length > LongitudUsuario)
            {
                return IVR_Proc_Liquidacion_Error_Crear(
                    "El usuario no puede superar "
                    + $"{LongitudUsuario} caracteres.");
            }

            return null;
        }

        /// <summary>
        /// Valida la eliminación de un movimiento bancario.
        /// </summary>
        private static ErrorDto?
            IVR_Proc_Liquidacion_Transaccion_Eliminar_Validar(
                IvrProcLiquidacionTransaccionEliminarRequest?
                    request)
        {
            if (request?.transaccion_id is null or <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El registro bancario es requerido.",
                    CodigoMensajeControlado);
            }

            if (request.liquidacion_transac_id
                is null or <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeTransaccionRequerida,
                    CodigoMensajeControlado);
            }

            return null;
        }

        /// <summary>
        /// Crea una respuesta de validación controlada.
        /// </summary>
        private static ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Error_Crear(
                string mensaje)
        {
            return DbHelper.CreateErrorResponse
                <IvrProcLiquidacionGuardarData>(
                    mensaje,
                    CodigoMensajeControlado);
        }

        /// <summary>
        /// Normaliza un texto proveniente del request.
        /// </summary>
        private static string
            IVR_Proc_Liquidacion_Texto_Normalizar(
                string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Crea el resultado devuelto por una operación.
        /// </summary>
        private static IvrProcLiquidacionGuardarData
            IVR_Proc_Liquidacion_Resultado_Crear(
                int transacId,
                string estado)
        {
            return new IvrProcLiquidacionGuardarData
            {
                transac_id = transacId,
                estado = estado,
                estado_descripcion =
                    IVR_Proc_Liquidacion_Estado_Descripcion_Obtener(
                        estado)
            };
        }

        /// <summary>
        /// Obtiene la descripción visible de un estado.
        /// </summary>
        private static string
            IVR_Proc_Liquidacion_Estado_Descripcion_Obtener(
                string? estado)
        {
            var estadoNormalizado =
                estado?.Trim().ToUpperInvariant()
                ?? string.Empty;

            return estadoNormalizado switch
            {
                EstadoSolicitud => "Solicitud",
                EstadoAplicado => "Procesada",
                EstadoAnulado => "Anulada",
                _ => estadoNormalizado
            };
        }
    }
}