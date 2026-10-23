using System.Data;
using System.Globalization;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrProcCuponesDb
    {
        private const int CodigoMensajeControlado = -2;

        private const int LongitudDocumento = 30;
        private const int LongitudDivisa = 10;
        private const int LongitudNotas = 500;
        private const int LongitudUsuario = 30;

        private const string TipoReferencia = "C";
        private const string ConceptoCupon = "Cupon";
        private const string TipoMovimientoCupon = "C";

        private const string EstadoSolicitud = "S";
        private const string EstadoAplicado = "A";
        private const string EstadoAnulado = "X";

        private const string MensajeSatisfactoriamente =
            "satisfactoriamente.";

        private const string MensajeInversionRequerida =
            "El n&uacute;mero de inversi&oacute;n es requerido.";

        private const string MensajeInversionNoExiste =
            "No se localiz&oacute; la inversi&oacute;n indicada.";

        private const string MensajeDivisaLocalNoConfigurada =
            "No se encuentra configurada la divisa local.";

        private const string MensajeDivisaRequerida =
            "La divisa de la inversi&oacute;n es requerida.";

        private const string MensajeTipoCambioInvalido =
            "El tipo de cambio no es v&aacute;lido.";

        private const string MensajeTransaccionRequerida =
            "El n&uacute;mero de transacci&oacute;n es requerido.";

        private const string MensajeDocumentoRequerido =
            "No ha indicado un n&uacute;mero de documento "
            + "para esta transacci&oacute;n.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private const string MensajeMontoRequerido =
            "No se ha indicado ning&uacute;n valor "
            + "para la transacci&oacute;n.";

        private const string MensajeFechaRequerida =
            "La fecha de la transacci&oacute;n es requerida.";

        private const string MensajeFechaInvalida =
            "La fecha de esta transacci&oacute;n es menor "
            + "al &uacute;ltimo corte.";

        private const string MensajeImportePendiente =
            "El importe del cup&oacute;n no ha sido "
            + "registrado completamente.";

        private readonly PortalDB _portalDb;

        public FrmIvrProcCuponesDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Inicializa la pantalla de registro de cupones.
        /// Permite iniciar sin título cuando se abre desde el menú.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Título opcional recibido por la pantalla.</param>
        /// <returns>Información inicial del registro de cupones.</returns>
        public ErrorDto<IvrProcCuponesInicialData>
            IVR_Proc_Cupones_Inicializar(
                int codEmpresa,
                IvrProcCuponesInicializarRequest? request)
        {
            var validation =
                IVR_Proc_Cupones_Inicializar_Validar(request);

            if (validation != null)
            {
                return validation;
            }

            var tituloId = request?.titulo_id ?? 0;

            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    IVR_Proc_Cupones_InicialData_Obtener(
                        connection,
                        tituloId));

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesInicialData>(
                        response.Description
                        ?? "Ocurri&oacute; un error al inicializar "
                        + "el registro de cupones.");
            }

            var result = response.Result;

            if (result == null)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesInicialData>(
                        "No fue posible obtener la informaci&oacute;n "
                        + "inicial del registro de cupones.");
            }

            if (string.IsNullOrWhiteSpace(result.divisa_local))
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesInicialData>(
                        MensajeDivisaLocalNoConfigurada,
                        CodigoMensajeControlado);
            }

            if (tituloId <= 0)
            {
                return response;
            }

            if (result.inversion == null)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesInicialData>(
                        MensajeInversionNoExiste,
                        CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(result.divisa))
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesInicialData>(
                        MensajeDivisaRequerida,
                        CodigoMensajeControlado);
            }

            if (result.tipo_cambio <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesInicialData>(
                        MensajeTipoCambioInvalido,
                        CodigoMensajeControlado);
            }

            return response;
        }

        /// <summary>
        /// Obtiene los movimientos bancarios asociados al cupón.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="transacId">Transacción del cupón.</param>
        /// <returns>Movimientos bancarios y monto registrado.</returns>
        public ErrorDto<IvrProcCuponesTransaccionesData>
            IVR_Proc_Cupones_Transacciones_Obtener(
                int codEmpresa,
                int transacId)
        {
            if (transacId <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesTransaccionesData>(
                        MensajeTransaccionRequerida,
                        CodigoMensajeControlado);
            }

            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    var transacciones =
                        IVR_Proc_Cupones_Transacciones_Consultar(
                            connection,
                            transacId);

                    return new IvrProcCuponesTransaccionesData
                    {
                        transacciones = transacciones,
                        registrado = transacciones.Sum(
                            item => item.importe)
                    };
                });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesTransaccionesData>(
                        response.Description
                        ?? "Ocurri&oacute; un error al consultar "
                        + "los registros bancarios.");
            }

            return response;
        }

        /// <summary>
        /// Guarda una solicitud de registro de cupón.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la solicitud.</param>
        /// <returns>Transacción registrada.</returns>
        public ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Solicitud_Guardar(
                int codEmpresa,
                IvrProcCuponesGuardarRequest? request)
        {
            var validation =
                IVR_Proc_Cupones_Request_Validar(
                    request,
                    requiereDocumento: false,
                    requiereTransaccion: false);

            if (validation != null)
            {
                return validation;
            }

            return IVR_Proc_Cupones_Registrar(
                codEmpresa,
                request!,
                EstadoSolicitud);
        }

        /// <summary>
        /// Procesa definitivamente una solicitud de cupón.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del cupón por procesar.</param>
        /// <returns>Resultado del procesamiento.</returns>
        public ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Procesar(
                int codEmpresa,
                IvrProcCuponesGuardarRequest? request)
        {
            var validation =
                IVR_Proc_Cupones_Request_Validar(
                    request,
                    requiereDocumento: true,
                    requiereTransaccion: true);

            if (validation != null)
            {
                return validation;
            }

            var processValidation =
                IVR_Proc_Cupones_Proceso_Validar(
                    codEmpresa,
                    request!);

            if (processValidation != null)
            {
                return processValidation;
            }

            return IVR_Proc_Cupones_Registrar(
                codEmpresa,
                request!,
                EstadoAplicado);
        }

        /// <summary>
        /// Anula una solicitud de cupón que todavía no fue procesada.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del cupón por anular.</param>
        /// <returns>Resultado de la anulación.</returns>
        public ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Eliminar(
                int codEmpresa,
                IvrProcCuponesGuardarRequest? request)
        {
            var validation =
                IVR_Proc_Cupones_Request_Validar(
                    request,
                    requiereDocumento: false,
                    requiereTransaccion: true);

            if (validation != null)
            {
                return validation;
            }

            return IVR_Proc_Cupones_Registrar(
                codEmpresa,
                request!,
                EstadoAnulado);
        }

        /// <summary>
        /// Elimina un movimiento bancario asociado a un cupón.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Identificadores del movimiento.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto
            IVR_Proc_Cupones_Transaccion_Eliminar(
                int codEmpresa,
                IvrProcCuponesTransaccionEliminarRequest? request)
        {
            var validation =
                IVR_Proc_Cupones_Transaccion_Eliminar_Validar(
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
                        request.cupon_transac_id!.Value.ToString(
                            CultureInfo.InvariantCulture),
                    ref_concepto = ConceptoCupon
                });

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description
                    ?? "Ocurri&oacute; un error al eliminar "
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
                + MensajeSatisfactoriamente);
        }

        /// <summary>
        /// Obtiene la información inicial de la pantalla.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="tituloId">Título opcional.</param>
        /// <returns>Datos iniciales del formulario.</returns>
        private static IvrProcCuponesInicialData
            IVR_Proc_Cupones_InicialData_Obtener(
                SqlConnection connection,
                int tituloId)
        {
            var divisaLocal =
                IVR_Proc_Cupones_DivisaLocal_Obtener(
                    connection);

            var ultimoCorte =
                IVR_Proc_Cupones_UltimoCorte_Consultar(
                    connection);

            var result = new IvrProcCuponesInicialData
            {
                fecha_ultimo_corte = ultimoCorte,
                fecha_transaccion = ultimoCorte.AddDays(1),
                divisa_local = divisaLocal,
                divisa = divisaLocal,
                tipo_cambio = 1,
                estado = EstadoSolicitud,
                estado_descripcion = "Solicitud"
            };

            if (tituloId <= 0)
            {
                return result;
            }

            var inversion =
                IVR_Proc_Cupones_Inversion_Obtener(
                    connection,
                    tituloId);

            result.inversion = inversion;

            if (inversion == null)
            {
                return result;
            }

            var calculo =
                IVR_Proc_Cupones_Calculo_Obtener(
                    connection,
                    tituloId);

            var divisaInversion =
                inversion.cod_divisa.Trim().ToUpperInvariant();

            var tipoCambio =
                IVR_Proc_Cupones_TipoCambio_Obtener(
                    divisaLocal,
                    inversion);

            var importeLocal =
                tipoCambio > 0
                    ? IVR_Proc_Cupones_ImporteLocal_Calcular(
                        calculo.mnt_interes,
                        tipoCambio)
                    : 0;

            var transacciones =
                calculo.cupon_id > 0
                    ? IVR_Proc_Cupones_Transacciones_Consultar(
                        connection,
                        calculo.cupon_id)
                    : [];

            var registrado = transacciones.Sum(
                item => item.importe);

            result.cupon = calculo;
            result.fecha_transaccion =
                calculo.fecha_corte
                ?? ultimoCorte.AddDays(1);
            result.divisa = divisaInversion;
            result.tipo_cambio = tipoCambio;
            result.importe_local = importeLocal;
            result.registrado = registrado;
            result.pendiente = importeLocal - registrado;
            result.transac_id = calculo.cupon_id;
            result.seq_id = calculo.seq_id;
            result.transacciones = transacciones;

            return result;
        }

        /// <summary>
        /// Obtiene la información principal de una inversión.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="tituloId">Identificador del título.</param>
        /// <returns>Información de la inversión.</returns>
        private static IvrProcCuponesInversionData?
            IVR_Proc_Cupones_Inversion_Obtener(
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
                <IvrProcCuponesInversionData>(
                    sql,
                    new
                    {
                        titulo_id = tituloId
                    });
        }

        /// <summary>
        /// Obtiene la divisa local configurada.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <returns>Código de la divisa local.</returns>
        private static string
            IVR_Proc_Cupones_DivisaLocal_Obtener(
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
        /// <param name="connection">Conexión de la empresa.</param>
        /// <returns>Fecha del último cierre.</returns>
        private static DateTime
            IVR_Proc_Cupones_UltimoCorte_Consultar(
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
        /// Ejecuta el cálculo del próximo cupón utilizado por VB6.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="tituloId">Identificador del título.</param>
        /// <returns>Información calculada del cupón.</returns>
        private static IvrProcCuponesCalculoData
            IVR_Proc_Cupones_Calculo_Obtener(
                SqlConnection connection,
                int tituloId)
        {
            return connection.QueryFirstOrDefault
                <IvrProcCuponesCalculoData>(
                    "spIVR_TITULOS_C_CONSULTA",
                    new
                    {
                        TituloId = tituloId
                    },
                    commandType:
                        CommandType.StoredProcedure)
                ?? new IvrProcCuponesCalculoData();
        }

        /// <summary>
        /// Determina el tipo de cambio aplicable al cupón.
        /// </summary>
        /// <param name="divisaLocal">Divisa local.</param>
        /// <param name="inversion">Información de la inversión.</param>
        /// <returns>Tipo de cambio aplicable.</returns>
        private static decimal
            IVR_Proc_Cupones_TipoCambio_Obtener(
                string divisaLocal,
                IvrProcCuponesInversionData inversion)
        {
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
        /// Calcula el importe del cupón en moneda local.
        /// </summary>
        /// <param name="monto">Monto del cupón.</param>
        /// <param name="tipoCambio">Tipo de cambio.</param>
        /// <returns>Importe equivalente en moneda local.</returns>
        private static decimal
            IVR_Proc_Cupones_ImporteLocal_Calcular(
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
        /// Consulta los movimientos bancarios asociados al cupón.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="transacId">Transacción del cupón.</param>
        /// <returns>Lista de movimientos bancarios.</returns>
        private static List<IvrProcCuponesTransaccionData>
            IVR_Proc_Cupones_Transacciones_Consultar(
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
                <IvrProcCuponesTransaccionData>(
                    sql,
                    new
                    {
                        ref_tipo = TipoReferencia,
                        ref_codigo = transacId.ToString(
                            CultureInfo.InvariantCulture),
                        ref_concepto = ConceptoCupon
                    })
                .ToList();
        }

        /// <summary>
        /// Guarda, procesa o anula el registro de un cupón.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del cupón.</param>
        /// <param name="estado">Estado solicitado.</param>
        /// <returns>Resultado del procedimiento.</returns>
        private ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Registrar(
                int codEmpresa,
                IvrProcCuponesGuardarRequest request,
                string estado)
        {
            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    IVR_Proc_Cupones_Registro_Ejecutar(
                        connection,
                        request,
                        estado));

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesGuardarData>(
                        response.Description
                        ?? "Ocurri&oacute; un error al registrar "
                        + "el cup&oacute;n.");
            }

            if (estado == EstadoAnulado)
            {
                return new ErrorDto<IvrProcCuponesGuardarData>
                {
                    Code = 0,
                    Description =
                        "Solicitud de cup&oacute;n eliminada "
                        + MensajeSatisfactoriamente,
                    Result =
                        IVR_Proc_Cupones_Resultado_Crear(
                            request.transac_id ?? 0,
                            request.seq_id ?? 0,
                            EstadoAnulado)
                };
            }

            if (response.Result == null
                || response.Result.transac_id <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesGuardarData>(
                        "El proceso no devolvi&oacute; el "
                        + "n&uacute;mero de transacci&oacute;n.");
            }

            var estadoResultado =
                string.IsNullOrWhiteSpace(response.Result.estado)
                    ? estado
                    : response.Result.estado;

            response.Result.estado = estadoResultado;
            response.Result.estado_descripcion =
                IVR_Proc_Cupones_Estado_Descripcion_Obtener(
                    estadoResultado);

            response.Description =
                estado == EstadoAplicado
                    ? "Transacci&oacute;n procesada "
                      + MensajeSatisfactoriamente
                    : "Solicitud registrada "
                      + MensajeSatisfactoriamente;

            return response;
        }

        /// <summary>
        /// Ejecuta el procedimiento de registro de cupones.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="request">Datos del cupón.</param>
        /// <param name="estado">Estado solicitado.</param>
        /// <returns>Resultado devuelto por el procedimiento.</returns>
        private static IvrProcCuponesGuardarData?
            IVR_Proc_Cupones_Registro_Ejecutar(
                SqlConnection connection,
                IvrProcCuponesGuardarRequest request,
                string estado)
        {
            return connection.QueryFirstOrDefault
                <IvrProcCuponesGuardarData>(
                    "spIVR_TITULOS_C_REGISTRA",
                    new
                    {
                        TituloId =
                            request.titulo_id!.Value,
                        TipoMov =
                            TipoMovimientoCupon,
                        Concepto =
                            ConceptoCupon,
                        Fecha =
                            request.fecha!.Value,
                        Documento =
                            IVR_Proc_Cupones_Texto_Normalizar(
                                request.documento),
                        Cupon =
                            request.cupon!.Value,
                        IntAcum =
                            request.int_acum!.Value,
                        Intereses =
                            request.intereses!.Value,
                        Divisa =
                            IVR_Proc_Cupones_Texto_Normalizar(
                                request.divisa)
                                .ToUpperInvariant(),
                        TipoCambio =
                            request.tipo_cambio!.Value,
                        Notas =
                            IVR_Proc_Cupones_Texto_Normalizar(
                                request.notas),
                        Usuario =
                            IVR_Proc_Cupones_Texto_Normalizar(
                                request.usuario)
                                .ToUpperInvariant(),
                        Estado = estado,
                        TransacId =
                            request.transac_id ?? 0,
                        SeqId =
                            request.seq_id ?? 0
                    },
                    commandType:
                        CommandType.StoredProcedure);
        }

        /// <summary>
        /// Valida las condiciones necesarias para procesar un cupón.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del cupón.</param>
        /// <returns>Error de validación o null.</returns>
        private ErrorDto<IvrProcCuponesGuardarData>?
            IVR_Proc_Cupones_Proceso_Validar(
                int codEmpresa,
                IvrProcCuponesGuardarRequest request)
        {
            var validationResponse = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    var ultimoCorte =
                        IVR_Proc_Cupones_UltimoCorte_Consultar(
                            connection);

                    var registrado =
                        IVR_Proc_Cupones_Registrado_Consultar(
                            connection,
                            request.transac_id!.Value);

                    return (
                        ultimoCorte,
                        registrado);
                });

            if (validationResponse.Code != 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesGuardarData>(
                        validationResponse.Description
                        ?? "Ocurri&oacute; un error al validar "
                        + "el cup&oacute;n.");
            }

            if (request.fecha!.Value
                < validationResponse.Result.ultimoCorte)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    MensajeFechaInvalida);
            }

            var importeLocal =
                IVR_Proc_Cupones_ImporteLocal_Calcular(
                    request.cupon!.Value,
                    request.tipo_cambio!.Value);

            var registrado = decimal.Round(
                validationResponse.Result.registrado,
                2,
                MidpointRounding.AwayFromZero);

            if (importeLocal != registrado)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    MensajeImportePendiente);
            }

            return null;
        }

        /// <summary>
        /// Obtiene el monto registrado en movimientos bancarios.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="transacId">Transacción del cupón.</param>
        /// <returns>Monto registrado.</returns>
        private static decimal
            IVR_Proc_Cupones_Registrado_Consultar(
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
                    ref_concepto = ConceptoCupon
                });
        }

        /// <summary>
        /// Valida el request utilizado para inicializar la pantalla.
        /// </summary>
        /// <param name="request">Request de inicialización.</param>
        /// <returns>Error de validación o null.</returns>
        private static ErrorDto<IvrProcCuponesInicialData>?
            IVR_Proc_Cupones_Inicializar_Validar(
                IvrProcCuponesInicializarRequest? request)
        {
            if (request?.titulo_id < 0)
            {
                return DbHelper.CreateErrorResponse
                    <IvrProcCuponesInicialData>(
                        MensajeInversionRequerida,
                        CodigoMensajeControlado);
            }

            return null;
        }

        /// <summary>
        /// Valida el request utilizado para registrar un cupón.
        /// </summary>
        /// <param name="request">Datos del cupón.</param>
        /// <param name="requiereDocumento">
        /// Indica si el documento es obligatorio.
        /// </param>
        /// <param name="requiereTransaccion">
        /// Indica si la transacción es obligatoria.
        /// </param>
        /// <returns>Error de validación o null.</returns>
        private static ErrorDto<IvrProcCuponesGuardarData>?
            IVR_Proc_Cupones_Request_Validar(
                IvrProcCuponesGuardarRequest? request,
                bool requiereDocumento,
                bool requiereTransaccion)
        {
            var validation =
                IVR_Proc_Cupones_Requeridos_Validar(
                    request,
                    requiereDocumento,
                    requiereTransaccion);

            if (validation != null)
            {
                return validation;
            }

            return IVR_Proc_Cupones_Longitudes_Validar(
                request!);
        }

        /// <summary>
        /// Valida los campos obligatorios del cupón.
        /// </summary>
        /// <param name="request">Datos del cupón.</param>
        /// <param name="requiereDocumento">
        /// Indica si el documento es obligatorio.
        /// </param>
        /// <param name="requiereTransaccion">
        /// Indica si la transacción es obligatoria.
        /// </param>
        /// <returns>Error de validación o null.</returns>
        private static ErrorDto<IvrProcCuponesGuardarData>?
            IVR_Proc_Cupones_Requeridos_Validar(
                IvrProcCuponesGuardarRequest? request,
                bool requiereDocumento,
                bool requiereTransaccion)
        {
            if (request?.titulo_id is null or <= 0)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    MensajeInversionRequerida);
            }

            if (requiereTransaccion
                && request.transac_id is null or <= 0)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    MensajeTransaccionRequerida);
            }

            if (request.fecha == null)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    MensajeFechaRequerida);
            }

            if (requiereDocumento
                && string.IsNullOrWhiteSpace(request.documento))
            {
                return IVR_Proc_Cupones_Error_Crear(
                    MensajeDocumentoRequerido);
            }

            if (request.cupon.GetValueOrDefault() <= 0)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    MensajeMontoRequerido);
            }

            if (request.int_acum == null
                || request.intereses == null)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    "Los montos del cup&oacute;n son requeridos.");
            }

            if (string.IsNullOrWhiteSpace(request.divisa))
            {
                return IVR_Proc_Cupones_Error_Crear(
                    MensajeDivisaRequerida);
            }

            if (request.tipo_cambio is null or <= 0)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    MensajeTipoCambioInvalido);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return IVR_Proc_Cupones_Error_Crear(
                    MensajeUsuarioRequerido);
            }

            return null;
        }

        /// <summary>
        /// Valida las longitudes aceptadas por el procedimiento.
        /// </summary>
        /// <param name="request">Datos del cupón.</param>
        /// <returns>Error de validación o null.</returns>
        private static ErrorDto<IvrProcCuponesGuardarData>?
            IVR_Proc_Cupones_Longitudes_Validar(
                IvrProcCuponesGuardarRequest request)
        {
            var documento =
                IVR_Proc_Cupones_Texto_Normalizar(
                    request.documento);

            var divisa =
                IVR_Proc_Cupones_Texto_Normalizar(
                    request.divisa);

            var notas =
                IVR_Proc_Cupones_Texto_Normalizar(
                    request.notas);

            var usuario =
                IVR_Proc_Cupones_Texto_Normalizar(
                    request.usuario);

            if (documento.Length > LongitudDocumento)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    "El documento no puede superar "
                    + $"{LongitudDocumento} caracteres.");
            }

            if (divisa.Length > LongitudDivisa)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    "La divisa indicada no es v&aacute;lida.");
            }

            if (notas.Length > LongitudNotas)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    "Las notas no pueden superar "
                    + $"{LongitudNotas} caracteres.");
            }

            if (usuario.Length > LongitudUsuario)
            {
                return IVR_Proc_Cupones_Error_Crear(
                    "El usuario no puede superar "
                    + $"{LongitudUsuario} caracteres.");
            }

            return null;
        }

        /// <summary>
        /// Valida la eliminación de un movimiento bancario.
        /// </summary>
        /// <param name="request">Identificadores del movimiento.</param>
        /// <returns>Error de validación o null.</returns>
        private static ErrorDto?
            IVR_Proc_Cupones_Transaccion_Eliminar_Validar(
                IvrProcCuponesTransaccionEliminarRequest?
                    request)
        {
            if (request?.transaccion_id is null or <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El registro bancario es requerido.",
                    CodigoMensajeControlado);
            }

            if (request.cupon_transac_id is null or <= 0)
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
        /// <param name="mensaje">Mensaje visible para el usuario.</param>
        /// <returns>Respuesta con código -2.</returns>
        private static ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Error_Crear(
                string mensaje)
        {
            return DbHelper.CreateErrorResponse
                <IvrProcCuponesGuardarData>(
                    mensaje,
                    CodigoMensajeControlado);
        }

        /// <summary>
        /// Normaliza un texto recibido desde el request.
        /// </summary>
        /// <param name="valor">Texto original.</param>
        /// <returns>Texto sin espacios externos.</returns>
        private static string
            IVR_Proc_Cupones_Texto_Normalizar(
                string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Crea el resultado devuelto por una operación.
        /// </summary>
        /// <param name="transacId">Transacción del cupón.</param>
        /// <param name="seqId">Secuencia del cupón.</param>
        /// <param name="estado">Estado resultante.</param>
        /// <returns>Resultado normalizado.</returns>
        private static IvrProcCuponesGuardarData
            IVR_Proc_Cupones_Resultado_Crear(
                int transacId,
                int seqId,
                string estado)
        {
            return new IvrProcCuponesGuardarData
            {
                transac_id = transacId,
                seq_id = seqId,
                estado = estado,
                estado_descripcion =
                    IVR_Proc_Cupones_Estado_Descripcion_Obtener(
                        estado)
            };
        }

        /// <summary>
        /// Obtiene la descripción visible de un estado.
        /// </summary>
        /// <param name="estado">Código del estado.</param>
        /// <returns>Descripción visible.</returns>
        private static string
            IVR_Proc_Cupones_Estado_Descripcion_Obtener(
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