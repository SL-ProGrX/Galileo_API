using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrAbonosComprobanteDb
    {
        private readonly PortalDB _portalDb;
        private readonly MRecibos _mRecibos;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MCntLinkDB _mCntLinkDb;

        private const string MensajeOperacionRequerida = "Debe indicar la operaci&oacute;n.";
        private const string MensajeOperacionNoEncontrada =
            "No se encontr&oacute; operaci&oacute;n para abonos, puede que se encuentre cancelada.";

        /// <summary>Inicializa las dependencias de datos del formulario.</summary>
        /// <param name="config">Configuración de conexiones y servicios del API.</param>
        public FrmCrAbonosComprobanteDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mRecibos = new MRecibos(config);
            _mProGrxMain = new MProGrxMain(config);
            _mCntLinkDb = new MCntLinkDB(config);
        }

        /// <summary>
        /// Obtiene el encabezado de una operacion activa para reemitir su comprobante.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="operacion">Numero de operacion.</param>
        /// <returns>Datos del encabezado de la operacion.</returns>
        public ErrorDto<CrAbonosComprobanteOperacionData> CrAbonosComprobante_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeOperacionRequerida,
                    -2,
                    new CrAbonosComprobanteOperacionData());
            }

            const string sql = @"
            update reg_creditos
               set saldo_mes = saldo
             where id_solicitud = @Operacion
               and isnull(saldo_mes, 0) = 0;

            select top 1
                isnull(R.id_solicitud, 0) as operacion,
                rtrim(isnull(R.proceso, '')) as proceso,
                rtrim(isnull(R.cedula, '')) as cedula,
                rtrim(isnull(S.nombre, '')) as nombre,
                rtrim(isnull(R.codigo, '')) as codigo,
                rtrim(isnull(C.descripcion, '')) as descripcion,
                isnull(R.opex, 0) as opex,
                case when isnull(R.opex, 0) = 1 then 'OPEX' else '' end as opex_descripcion,
                convert(bit, case
                    when isnull(C.retencion, 'N') = 'S' or isnull(C.poliza, 'N') = 'S'
                    then 1 else 0 end) as retencion,
                rtrim(isnull(Ofi.descripcion, '')) as oficina_descripcion,
                rtrim(isnull(C.descripcion, '')) as linea_descripcion,
                rtrim(isnull(Pre.descripcion, '')) as recurso_descripcion
            from reg_creditos R
            inner join Catalogo C on R.codigo = C.codigo
            inner join Socios S on R.cedula = S.cedula
            left join Sif_Oficinas Ofi on R.cod_oficina_r = Ofi.cod_oficina
            left join Catalogo_Grupos Pre on R.cod_grupo = Pre.cod_grupo
            where R.id_solicitud = @Operacion
              and R.estado = 'A';";

            var response = DbHelper.ExecuteSingleQuery<CrAbonosComprobanteOperacionData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { Operacion = operacion });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    response.Description ?? "No fue posible consultar la operaci&oacute;n.",
                    response.Code.GetValueOrDefault(-1),
                    new CrAbonosComprobanteOperacionData());
            }

            if (response.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeOperacionNoEncontrada,
                    -2,
                    new CrAbonosComprobanteOperacionData());
            }

            return DbHelper.CreateOkResponse(response.Result);
        }

        /// <summary>
        /// Obtiene las operaciones activas para la busqueda F4.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Lista de operaciones activas.</returns>
        public ErrorDto<List<CrAbonosComprobanteOperacionListaItem>>
            CrAbonosComprobante_Operaciones_Lista_Obtener(int codEmpresa)
        {
            const string sql = @"
            select top 1000
                isnull(R.id_solicitud, 0) as operacion,
                rtrim(isnull(R.codigo, '')) as codigo,
                rtrim(isnull(S.cedula, '')) as cedula,
                rtrim(isnull(S.nombre, '')) as nombre,
                rtrim(isnull(C.descripcion, '')) as descripcion
            from reg_creditos R
            inner join Socios S on R.cedula = S.cedula
            inner join Catalogo C on R.codigo = C.codigo
            where R.estado = 'A'
            order by S.cedula, R.id_solicitud;";

            return DbHelper.ExecuteListQuery<CrAbonosComprobanteOperacionListaItem>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene los tipos de documento habilitados por el formulario original.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Tipos de documento para el selector.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CrAbonosComprobante_TiposDocumento_Obtener(int codEmpresa)
        {
            _ = codEmpresa;
            return DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>
            {
                new() { item = "RE", descripcion = "Recibo" },
                new() { item = "NC", descripcion = "Nota Crédito" },
                new() { item = "DP", descripcion = "Depósitos" }
            });
        }

        /// <summary>
        /// Reconstruye el comprobante historico del abono y registra los asientos contables.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Datos funcionales del comprobante.</param>
        /// <returns>Resultado del registro y de la impresion.</returns>
        public ErrorDto<CrAbonosComprobanteAplicarResultadoData> CrAbonosComprobante_Aplicar(
            int codEmpresa,
            CrAbonosComprobanteAplicarRequest request)
        {
            var resultado = new CrAbonosComprobanteAplicarResultadoData();
            NormalizarRequest(request);

            var validacion = ValidarRequest(request, resultado);
            if (validacion is not null)
            {
                return validacion;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();

                var operacion = ObtenerOperacionInterna(conn, tx, request.operacion);
                if (operacion is null)
                {
                    return DbHelper.CreateErrorResponse(MensajeOperacionNoEncontrada, -2, resultado);
                }

                var existe = conn.QueryFirstOrDefault<int>(@"
                    select count(1)
                    from sif_transacciones
                    where tipo_documento = @TipoDocumento
                      and cod_transaccion = @NumDocumento;",
                    new
                    {
                        TipoDocumento = request.tipo_documento,
                        NumDocumento = request.num_documento
                    },
                    tx);

                if (existe > 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "Ya existe el comprobante creado, verifique.",
                        -2,
                        resultado);
                }

                var globalesResponse = _mProGrxMain.sbSifParametrosInicializa(
                    codEmpresa,
                    request.usuario);

                if (globalesResponse.Code != 0 || globalesResponse.Result is null)
                {
                    return DbHelper.CreateErrorResponse(
                        globalesResponse.Description ?? "No fue posible obtener los par&aacute;metros globales.",
                        globalesResponse.Code.GetValueOrDefault(-1),
                        resultado);
                }

                var validacionCuenta = ResolverCuentaDocumento(
                    codEmpresa,
                    conn,
                    tx,
                    globalesResponse.Result.SysDocVersion,
                    request,
                    resultado,
                    out var cuentaDocumento);

                if (validacionCuenta is not null)
                {
                    return validacionCuenta;
                }

                var tipoAfectacion = globalesResponse.Result.SysDocVersion == 1
                    ? TipoDocumentoNumero(request.tipo_documento)
                    : request.tipo_documento;

                var afectacion = conn.QueryFirstOrDefault<CrAbonosComprobanteAfectacionData>(
                    "exec spCrdDocumentoAfectacion @TipoDocumento, @NumDocumento, 'R'",
                    new
                    {
                        TipoDocumento = tipoAfectacion,
                        NumDocumento = request.num_documento
                    },
                    tx) ?? new CrAbonosComprobanteAfectacionData();

                var movimientos = conn.Query<CrAbonosComprobanteMovimientoData>(@"
                    select saldo_anterior, saldo_actual, cod_concepto, mov_usuario, mov_fecha
                    from crd_operacion_transac
                    where id_solicitud = @Operacion
                      and tipo_documento = @TipoDocumento
                      and num_comprobante = @NumDocumento
                    order by id_seq;",
                    new
                    {
                        Operacion = request.operacion,
                        TipoDocumento = tipoAfectacion,
                        NumDocumento = request.num_documento
                    },
                    tx).AsList();

                if (movimientos.Count == 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se localizan movimientos registrados con este comprobante.",
                        -2,
                        resultado);
                }

                var cuentas = conn.QueryFirstOrDefault<CrAbonosComprobanteOperacionCtasData>(
                    "exec spCrdOperacionCtas @Operacion",
                    new { Operacion = request.operacion },
                    tx);

                if (cuentas is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No fue posible obtener las cuentas de la operaci&oacute;n.",
                        -2,
                        resultado);
                }

                var primerMovimiento = movimientos[0];
                var ultimoMovimiento = movimientos[^1];
                var oficinaData = conn.QueryFirstOrDefault<CrAbonosComprobanteOficinaData>(
                    "exec sbSIFOficinasUsuario @Usuario",
                    new { Usuario = primerMovimiento.mov_usuario },
                    tx);
                var oficina = string.IsNullOrWhiteSpace(oficinaData?.titular)
                    ? globalesResponse.Result.GOficinaTitular
                    : oficinaData.titular;

                var proximoPago = conn.QueryFirstOrDefault<CrAbonosComprobanteProximoPagoData>(
                    "exec spCrdOperacionFechaProxPago @Operacion",
                    new { Operacion = request.operacion },
                    tx) ?? new CrAbonosComprobanteProximoPagoData();

                var montoTotal = afectacion.IntCor
                    + afectacion.IntMor
                    + afectacion.Principal
                    + afectacion.Cargos
                    + afectacion.Polizas;

                var concepto = ObtenerConcepto(request.tipo_abono);
                InsertarDocumento(
                    conn,
                    tx,
                    new DocumentoRegistroContexto
                    {
                        Request = request,
                        Operacion = operacion,
                        PrimerMovimiento = primerMovimiento,
                        UltimoMovimiento = ultimoMovimiento,
                        Afectacion = afectacion,
                        ProximoPago = proximoPago,
                        Oficina = oficina,
                        Concepto = concepto,
                        MontoTotal = montoTotal
                    });

                var asientoContexto = new AsientoRegistroContexto
                {
                    Conn = conn,
                    Tx = tx,
                    Request = request,
                    Cuentas = cuentas,
                    Enlace = globalesResponse.Result.GEnlace,
                    Deposito = request.referencia_documento
                };

                RegistrarAsiento(asientoContexto, afectacion.IntCor, cuentas.ctaintc, "C");
                RegistrarAsiento(asientoContexto, afectacion.IntMor, cuentas.ctaintm, "C");
                RegistrarCargos(asientoContexto, afectacion.Cargos);
                RegistrarPoliza(asientoContexto, afectacion.Polizas);
                RegistrarAsiento(asientoContexto, afectacion.Principal, cuentas.ctaamortiza, "C");
                RegistrarAsiento(asientoContexto, montoTotal, cuentaDocumento, "D");

                tx.Commit();

                // En el formulario original uRecibos se inicializa siempre en True:
                // por eso la reconstrucción genera el recibo después de confirmar la transacción.
                var impresion = _mRecibos.sbImprimeRecibo(
                    codEmpresa,
                    request.num_documento,
                    request.tipo_documento,
                    request.usuario,
                    pFolder: "Creditos");

                CompletarResultado(resultado, request, montoTotal, impresion);

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, resultado);
            }
        }

        /// <summary>Normaliza los valores recibidos antes de validarlos.</summary>
        /// <param name="request">Solicitud del comprobante que se normalizará.</param>
        private static void NormalizarRequest(CrAbonosComprobanteAplicarRequest request)
        {
            request.tipo_documento = (request.tipo_documento ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
            request.num_documento = (request.num_documento ?? string.Empty).Trim();
            request.usuario = (request.usuario ?? string.Empty).Trim();
            request.cuenta_documento = (request.cuenta_documento ?? string.Empty).Trim();
            request.referencia_documento = (request.referencia_documento ?? string.Empty)
                .Trim()[..Math.Min((request.referencia_documento ?? string.Empty).Trim().Length, 30)];
            request.detalle_documento = (request.detalle_documento ?? string.Empty)
                .Trim()
                .ToUpperInvariant()
                .Replace("'", string.Empty);
        }

        /// <summary>Valida los datos mínimos para reconstruir el comprobante.</summary>
        /// <param name="request">Solicitud normalizada.</param>
        /// <param name="resultado">Resultado vacío usado en una respuesta de error.</param>
        /// <returns>Error funcional o <see langword="null"/> cuando la solicitud es válida.</returns>
        private static ErrorDto<CrAbonosComprobanteAplicarResultadoData>? ValidarRequest(
            CrAbonosComprobanteAplicarRequest request,
            CrAbonosComprobanteAplicarResultadoData resultado)
        {
            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(MensajeOperacionRequerida, -2, resultado);
            }

            if (request.tipo_abono is < 0 or > 3)
            {
                return DbHelper.CreateErrorResponse("El tipo de abono no es v&aacute;lido.", -2, resultado);
            }

            if (request.tipo_documento is not ("RE" or "NC" or "DP"))
            {
                return DbHelper.CreateErrorResponse("El tipo de documento no es v&aacute;lido.", -2, resultado);
            }

            if (string.IsNullOrWhiteSpace(request.num_documento)
                || request.num_documento.Length > 30)
            {
                return DbHelper.CreateErrorResponse("Debe indicar un n&uacute;mero de documento v&aacute;lido.", -2, resultado);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse("No se identific&oacute; el usuario de la sesi&oacute;n.", -2, resultado);
            }

            if (request.tipo_documento != "RE"
                && (string.IsNullOrWhiteSpace(request.cuenta_documento)
                    || string.IsNullOrWhiteSpace(request.detalle_documento)))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la cuenta contable y el detalle del documento.",
                    -2,
                    resultado);
            }

            if (request.detalle_documento.Length > 255)
            {
                return DbHelper.CreateErrorResponse(
                    "El detalle del documento no puede superar 255 caracteres.",
                    -2,
                    resultado);
            }

            return null;
        }

        /// <summary>Obtiene la operación activa dentro de la transacción actual.</summary>
        /// <param name="conn">Conexión abierta de la empresa.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="operacion">Número de operación.</param>
        /// <returns>Datos de la operación o <see langword="null"/>.</returns>
        private static CrAbonosComprobanteOperacionData? ObtenerOperacionInterna(
            SqlConnection conn,
            SqlTransaction tx,
            int operacion)
        {
            return conn.QueryFirstOrDefault<CrAbonosComprobanteOperacionData>(@"
                select top 1
                    isnull(R.id_solicitud, 0) as operacion,
                    rtrim(isnull(R.proceso, '')) as proceso,
                    rtrim(isnull(R.cedula, '')) as cedula,
                    rtrim(isnull(S.nombre, '')) as nombre,
                    rtrim(isnull(R.codigo, '')) as codigo,
                    rtrim(isnull(C.descripcion, '')) as descripcion,
                    isnull(R.opex, 0) as opex,
                    case when isnull(R.opex, 0) = 1 then 'OPEX' else '' end as opex_descripcion,
                    convert(bit, case
                        when isnull(C.retencion, 'N') = 'S' or isnull(C.poliza, 'N') = 'S'
                        then 1 else 0 end) as retencion
                from reg_creditos R
                inner join Catalogo C on R.codigo = C.codigo
                inner join Socios S on R.cedula = S.cedula
                where R.id_solicitud = @Operacion
                  and R.estado = 'A';",
                new { Operacion = operacion },
                tx);
        }

        /// <summary>Obtiene la cuenta configurada para el tipo de documento.</summary>
        /// <param name="conn">Conexión abierta de la empresa.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="sysDocVersion">Versión del sistema de documentos.</param>
        /// <param name="tipoDocumento">Tipo de documento SIF.</param>
        /// <returns>Cuenta contable configurada.</returns>
        private static string ObtenerCuentaDocumento(
            SqlConnection conn,
            SqlTransaction tx,
            int sysDocVersion,
            string tipoDocumento)
        {
            if (sysDocVersion != 1)
            {
                return conn.QueryFirstOrDefault<string>(@"
                    select rtrim(isnull(cod_cuenta, ''))
                    from sif_documentos
                    where tipo_documento = @TipoDocumento;",
                    new { TipoDocumento = tipoDocumento },
                    tx) ?? string.Empty;
            }

            var columna = tipoDocumento switch
            {
                "RE" => "CS_RE_CUENTA",
                "DP" => "CS_DP_CUENTA",
                "NC" => "CS_NC_CUENTA",
                _ => string.Empty
            };

            return string.IsNullOrEmpty(columna)
                ? string.Empty
                : conn.QueryFirstOrDefault<string>(
                    $"select rtrim(isnull({columna}, '')) from ase_consecutivos;",
                    transaction: tx) ?? string.Empty;
        }

        /// <summary>Resuelve y valida la cuenta contable que utilizará el comprobante.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="conn">Conexión abierta de la empresa.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="sysDocVersion">Versión del sistema de documentos.</param>
        /// <param name="request">Solicitud del comprobante.</param>
        /// <param name="resultado">Resultado vacío usado en una respuesta de error.</param>
        /// <param name="cuentaDocumento">Cuenta contable resuelta.</param>
        /// <returns>Error funcional o <see langword="null"/> cuando la cuenta es válida.</returns>
        private ErrorDto<CrAbonosComprobanteAplicarResultadoData>? ResolverCuentaDocumento(
            int codEmpresa,
            SqlConnection conn,
            SqlTransaction tx,
            int sysDocVersion,
            CrAbonosComprobanteAplicarRequest request,
            CrAbonosComprobanteAplicarResultadoData resultado,
            out string cuentaDocumento)
        {
            cuentaDocumento = ObtenerCuentaDocumento(
                conn,
                tx,
                sysDocVersion,
                request.tipo_documento);

            if (!string.IsNullOrWhiteSpace(request.cuenta_documento))
            {
                if (!_mCntLinkDb.fxgCntCuentaValida(codEmpresa, request.cuenta_documento))
                {
                    return DbHelper.CreateErrorResponse(
                        "La cuenta contable indicada no es válida o no acepta movimientos.",
                        -2,
                        resultado);
                }

                cuentaDocumento = request.cuenta_documento;
            }

            return string.IsNullOrWhiteSpace(cuentaDocumento)
                ? DbHelper.CreateErrorResponse(
                    "No se puede realizar el movimiento porque no se especificó una cuenta contable válida para esta operación.",
                    -2,
                    resultado)
                : null;
        }

        /// <summary>Completa la respuesta final con el resultado del comprobante y su impresión.</summary>
        /// <param name="resultado">Respuesta que se devolverá al cliente.</param>
        /// <param name="request">Solicitud del comprobante.</param>
        /// <param name="montoTotal">Monto total reconstruido.</param>
        /// <param name="impresion">Resultado producido por el generador de recibos.</param>
        private static void CompletarResultado(
            CrAbonosComprobanteAplicarResultadoData resultado,
            CrAbonosComprobanteAplicarRequest request,
            decimal montoTotal,
            ErrorDto<object> impresion)
        {
            resultado.tipo_documento = request.tipo_documento;
            resultado.num_documento = request.num_documento;
            resultado.monto_total = montoTotal;
            resultado.reporte_resultado = impresion.Code == -1
                ? null
                : impresion.Result?.ToString();
            resultado.mensaje = impresion.Code == -1
                ? $"Comprobante de abono realizado {request.tipo_documento} #{request.num_documento}, pero no fue posible generar el recibo: {impresion.Description}"
                : $"Comprobante de Abono Realizado {request.tipo_documento} #{request.num_documento}";
        }

        /// <summary>Convierte el tipo al número utilizado por el esquema documental anterior.</summary>
        /// <param name="tipoDocumento">Tipo de documento SIF.</param>
        /// <returns>Número equivalente del documento.</returns>
        private static string TipoDocumentoNumero(string tipoDocumento)
        {
            return tipoDocumento switch
            {
                "RE" => "2",
                "DP" => "6",
                "NC" => "7",
                _ => "1"
            };
        }

        /// <summary>Obtiene el concepto contable correspondiente al tipo de abono.</summary>
        /// <param name="tipoAbono">Índice del tipo de abono.</param>
        /// <returns>Código de concepto contable.</returns>
        private static string ObtenerConcepto(int tipoAbono)
        {
            return tipoAbono switch
            {
                0 => "CRD001",
                1 => "CRD002",
                2 => "CRD003",
                3 => "CRD004",
                _ => "CRD001"
            };
        }

        /// <summary>Agrupa los datos necesarios para registrar el documento SIF.</summary>
        private sealed class DocumentoRegistroContexto
        {
            public CrAbonosComprobanteAplicarRequest Request { get; init; } = null!;
            public CrAbonosComprobanteOperacionData Operacion { get; init; } = new();
            public CrAbonosComprobanteMovimientoData PrimerMovimiento { get; init; } = new();
            public CrAbonosComprobanteMovimientoData UltimoMovimiento { get; init; } = new();
            public CrAbonosComprobanteAfectacionData Afectacion { get; init; } = new();
            public CrAbonosComprobanteProximoPagoData ProximoPago { get; init; } = new();
            public string Oficina { get; init; } = string.Empty;
            public string Concepto { get; init; } = string.Empty;
            public decimal MontoTotal { get; init; }
        }

        /// <summary>Agrupa la conexión y los datos comunes de los asientos del comprobante.</summary>
        private sealed class AsientoRegistroContexto
        {
            public SqlConnection Conn { get; init; } = null!;
            public SqlTransaction Tx { get; init; } = null!;
            public CrAbonosComprobanteAplicarRequest Request { get; init; } = null!;
            public CrAbonosComprobanteOperacionCtasData Cuentas { get; init; } = new();
            public int Enlace { get; init; }
            public string Deposito { get; init; } = string.Empty;
        }

        /// <summary>Registra el encabezado y las líneas del comprobante SIF.</summary>
        /// <param name="conn">Conexión abierta de la empresa.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="contexto">Datos agrupados requeridos para registrar el documento.</param>
        private static void InsertarDocumento(
            SqlConnection conn,
            SqlTransaction tx,
            DocumentoRegistroContexto contexto)
        {
            var request = contexto.Request;
            var operacion = contexto.Operacion;
            var primero = contexto.PrimerMovimiento;
            var ultimo = contexto.UltimoMovimiento;
            var afectacion = contexto.Afectacion;
            var proximoPago = contexto.ProximoPago;

            var linea9 = proximoPago.fecha_pago.HasValue
                ? $"Prox.Pago..:{proximoPago.fecha_pago:dd/MM/yyyy} Cta.({proximoPago.num_cuota}) {proximoPago.cuota:N2}"
                : "Prox.Pago..: >> <<";

            conn.Execute(@"
                insert SIF_TRANSACCIONES
                (
                    COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
                    Cliente_IDENTIFICACION, CLIENTE_NOMBRE, cod_concepto, monto, estado,
                    Referencia_01, Referencia_02, Referencia_03, cod_oficina,
                    linea1, linea2, linea3, linea4, linea5, linea6, linea7, linea8,
                    linea9, linea10, linea11, detalle, documento
                )
                values
                (
                    @NumDocumento, @TipoDocumento, @Fecha, @Usuario,
                    @Cedula, @Nombre, @Concepto, @Monto, 'P',
                    @Operacion, @Codigo, @ReferenciaDocumento, @Oficina,
                    @Linea1, @Linea2, @Linea3, @Linea4, @Linea5, @Linea6, @Linea7,
                    @Linea8, @Linea9, @Linea10, @Linea11, @DetalleDocumento, @ReferenciaDocumento
                );",
                new
                {
                    NumDocumento = request.num_documento,
                    TipoDocumento = request.tipo_documento,
                    Fecha = primero.mov_fecha,
                    Usuario = primero.mov_usuario,
                    operacion.cedula,
                    operacion.nombre,
                    Concepto = string.IsNullOrWhiteSpace(primero.cod_concepto)
                        ? contexto.Concepto
                        : primero.cod_concepto,
                    Monto = contexto.MontoTotal,
                    Operacion = request.operacion.ToString(),
                    operacion.codigo,
                    Oficina = contexto.Oficina,
                    Linea1 = $"Saldo Anterior    {primero.saldo_anterior:N2}",
                    Linea2 = $"Interes Corriente {afectacion.IntCor:N2}",
                    Linea3 = $"Interes Atrasado  {afectacion.IntMor:N2}",
                    Linea4 = $"Amortización      {afectacion.Principal:N2}",
                    Linea5 = $"Cargos            {afectacion.Cargos:N2}",
                    Linea6 = $"Saldo Actual      {ultimo.saldo_actual:N2}",
                    Linea7 = $"Operacion/Línea   Op.:{request.operacion} Lí.:{operacion.codigo} Ret.:{(operacion.retencion ? "SI" : "NO")}",
                    Linea8 = operacion.descripcion,
                    Linea9 = linea9,
                    Linea10 = $"Notas: {proximoPago.notas}",
                    Linea11 = $"Póliza            {afectacion.Polizas:N2}",
                    ReferenciaDocumento = request.referencia_documento,
                    DetalleDocumento = request.detalle_documento
                },
                tx);
        }

        /// <summary>Registra una línea de asiento del comprobante.</summary>
        /// <param name="contexto">Conexión y datos comunes del asiento.</param>
        /// <param name="monto">Monto de la línea.</param>
        /// <param name="cuenta">Cuenta contable.</param>
        /// <param name="tipo">Naturaleza débito o crédito.</param>
        private static void RegistrarAsiento(
            AsientoRegistroContexto contexto,
            decimal monto,
            string cuenta,
            string tipo)
        {
            if (monto <= 0 || string.IsNullOrWhiteSpace(cuenta))
            {
                return;
            }

            contexto.Conn.Execute(@"
                exec spSIFDocsAsiento
                    @TipoDocumento, @NumDocumento, @Monto, @Tipo, @Divisa, 1,
                    @Enlace, @Unidad, @CentroCosto, @Cuenta, @Operacion, @Codigo, @Deposito;",
                new
                {
                    TipoDocumento = contexto.Request.tipo_documento,
                    NumDocumento = contexto.Request.num_documento,
                    Monto = monto,
                    Tipo = tipo,
                    Divisa = contexto.Cuentas.cod_Divisa,
                    Enlace = contexto.Enlace,
                    Unidad = contexto.Cuentas.cod_unidad,
                    CentroCosto = contexto.Cuentas.cod_centro_costo,
                    Cuenta = cuenta,
                    Operacion = contexto.Cuentas.id_solicitud,
                    Codigo = contexto.Cuentas.Codigo,
                    Deposito = contexto.Deposito
                },
                contexto.Tx);
        }

        /// <summary>Registra los asientos correspondientes a cargos del abono.</summary>
        /// <param name="contexto">Conexión y datos comunes de los asientos.</param>
        /// <param name="montoCargos">Monto total de cargos.</param>
        private static void RegistrarCargos(
            AsientoRegistroContexto contexto,
            decimal montoCargos)
        {
            if (montoCargos <= 0)
            {
                return;
            }

            var cargos = contexto.Conn.Query<CrAbonosComprobanteCargoData>(
                "exec spCrdDocumentoAfectacionCargos @TipoDocumento, @NumDocumento",
                new
                {
                    TipoDocumento = contexto.Request.tipo_documento,
                    NumDocumento = contexto.Request.num_documento
                },
                contexto.Tx);

            foreach (var cargo in cargos)
            {
                var monto = cargo.mov_monto ?? montoCargos;
                if (monto <= 0 || string.IsNullOrWhiteSpace(cargo.cod_cuenta))
                {
                    continue;
                }

                contexto.Conn.Execute(@"
                    exec spSIFDocsAsiento
                        @TipoDocumento, @NumDocumento, @Monto, 'C', @Divisa, 1,
                        @Enlace, @Unidad, @CentroCosto, @Cuenta, @Operacion, @Codigo, @Deposito;",
                    new
                    {
                        TipoDocumento = contexto.Request.tipo_documento,
                        NumDocumento = contexto.Request.num_documento,
                        Monto = monto,
                        Divisa = contexto.Cuentas.cod_Divisa,
                        Enlace = contexto.Enlace,
                        Unidad = cargo.cod_unidad,
                        CentroCosto = cargo.cod_centro_costo,
                        Cuenta = cargo.cod_cuenta,
                        Operacion = cargo.id_solicitud,
                        Codigo = cargo.codigo,
                        Deposito = contexto.Deposito
                    },
                    contexto.Tx);
            }
        }

        /// <summary>Registra el asiento correspondiente a la póliza del abono.</summary>
        /// <param name="contexto">Conexión y datos comunes de los asientos.</param>
        /// <param name="montoPoliza">Monto de póliza aplicado.</param>
        private static void RegistrarPoliza(
            AsientoRegistroContexto contexto,
            decimal montoPoliza)
        {
            if (montoPoliza <= 0)
            {
                return;
            }

            var cuentaPoliza = contexto.Conn.QueryFirstOrDefault<string>(
                "select rtrim(isnull(dbo.fxCrdOperacionCtaContaPolizas(@Operacion), ''));",
                new { Operacion = contexto.Cuentas.id_solicitud },
                contexto.Tx) ?? string.Empty;

            RegistrarAsiento(contexto, montoPoliza, cuentaPoliza, "C");
        }
    }
}
