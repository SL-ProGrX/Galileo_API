using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.DataBaseTier;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrAnulaAbonosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MRecibos _mRecibos;
        private readonly MProGrxMain _mProGrx;
        private readonly MSecurityMainDb _bitacora;
        private readonly MAfilicacionDB _mAfiliacion;
        private const int VModulo = 3;
        private const string TipoDocumento = "ND";
        private const string Concepto = "CRD008";

        public FrmCrAnulaAbonosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mRecibos = new MRecibos(config);
            _mProGrx = new MProGrxMain(config);
            _bitacora = new MSecurityMainDb(config);
            _mAfiliacion = new MAfilicacionDB(config);
        }

        /// <summary>
        /// Consulta los datos de la operación y sus movimientos anulables.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se consulta la operación.</param>
        /// <param name="idSolicitud">Número de operación de crédito.</param>
        /// <returns>Encabezado, movimientos registrados y últimas cuotas canceladas.</returns>
        public ErrorDto<CrAnulaAbonosConsultaResponse> CR_AnulaAbonos_ConsultarOperacion(int codEmpresa, int idSolicitud)
        {
            var response = new CrAnulaAbonosConsultaResponse();

            if (idSolicitud <= 0)
                return DbHelper.CreateErrorResponse("Debe indicar una operación válida.", -2, response);

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();

            var operacion = conn.QueryFirstOrDefault<CrAnulaAbonosOperacionData>(@"
select
    R.id_solicitud,
    isnull(R.saldo,0) as saldo,
    rtrim(isnull(R.proceso,'')) as proceso,
    case rtrim(isnull(R.proceso,''))
        when 'N' then 'Normal'
        when 'T' then 'Traspaso Deuda'
        when 'J' then 'Cobro Judicial'
        when 'I' then 'Incobrable'
        else rtrim(isnull(R.proceso,''))
    end as proceso_desc,
    isnull(isnull(R.interesv,R.int),0) as interes,
    isnull(R.plazo,0) as plazo,
    isnull(R.prideduc,0) as prideduc,
    isnull(R.fecult,0) as fecult,
    convert(bit, case when isnull(R.opex,0) = 1 then 1 else 0 end) as opex,
    case when isnull(R.opex,0) = 1 then 'Op.Ex.' else 'Interno' end as opex_desc,
    rtrim(isnull(R.codigo,'')) as codigo,
    rtrim(isnull(R.cedula,'')) as cedula,
    rtrim(isnull(S.nombre,'')) as nombre,
    rtrim(isnull(C.descripcion,'')) as descripcion,
    convert(bit, case when isnull(C.retencion,'N') = 'S' or isnull(C.poliza,'N') = 'S' then 1 else 0 end) as retencion,
    rtrim(isnull(R.base_calculo,'')) as base_calculo
from reg_creditos R
inner join Catalogo C on R.codigo = C.codigo
inner join Socios S on R.cedula = S.cedula
where R.estado in('A','C')
  and R.ID_SOLICITUD = @idSolicitud;", new { idSolicitud });

            if (operacion == null)
                return DbHelper.CreateErrorResponse("No se encontró la operación.", -2, response);

            response.operacion = operacion;
            response.movimientos = conn.Query<CrAnulaAbonosMovimientoData>(@"
select
    isnull(Id_seq,0) as id_seq,
    isnull(Num_Cuota,0) as num_cuota,
    isnull(Fecha_Proceso,0) as fecha_proceso,
    isnull(Cuota,0) as cuota,
    case when isnull(Mora_Dias,0) > 0 then 'En Mora' else 'Al Día' end as estado,
    isnull(Mov_IntCor,0) as mov_intcor,
    isnull(Mov_IntMor,0) as mov_intmor,
    isnull(Mov_Principal,0) as mov_principal,
    isnull(Mov_Cargos,0) as mov_cargos,
    isnull(Mov_Poliza,0) as mov_poliza,
    isnull(Dias_calculo,0) as dias_calculo,
    isnull(Mora_Dias,0) as mora_dias,
    rtrim(isnull(Tipo_Documento,'')) as tipo_documento,
    rtrim(isnull(Num_Comprobante,'')) as num_comprobante,
    Mov_fecha as mov_fecha,
    rtrim(isnull(Mov_usuario,'')) as mov_usuario
from CRD_OPERACION_TRANSAC
where estado = 'C'
  and id_solicitud = @idSolicitud
  and Tipo_Documento not in('AJ')
  and Mov_Monto > 0
order by id_seq desc;", new { idSolicitud }).ToList();

            response.ultimas_cuotas = ConstruirUltimasCuotas(operacion.fecult, operacion.prideduc);
            return DbHelper.CreateOkResponse(response);
        }

        /// <summary>
        /// Obtiene la cuenta recomendada para la reversión de principal.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se calcula la cuenta.</param>
        /// <param name="request">Operación y monto de amortización.</param>
        /// <returns>Cuenta recomendada o puntos suspensivos si no aplica.</returns>
        public ErrorDto<string> CR_AnulaAbonos_CuentaRecomendada(int codEmpresa, CrAnulaAbonosCuentaRecomendadaRequest request)
        {
            if (request.id_solicitud <= 0 || request.monto_amortizacion <= 0)
                return DbHelper.CreateOkResponse("...");

            const string sql = @"
select isnull(dbo.fxCrd_Operacion_Anula_Cta_Recomendada(@idSolicitud, @montoAmortizacion),'...') as cuenta;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                "...",
                new { idSolicitud = request.id_solicitud, montoAmortizacion = request.monto_amortizacion });
        }

        /// <summary>
        /// Procesa la anulación de abonos generando nota débito, asientos, saldo a favor y movimiento de plan de pagos.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se registra el proceso.</param>
        /// <param name="request">Datos de anulación capturados en frmCR_AnulaAbonos.</param>
        /// <returns>Documento generado y resultado de impresión.</returns>
        public ErrorDto<CrAnulaAbonosProcesarResponse> CR_AnulaAbonos_Procesar(int codEmpresa, CrAnulaAbonosProcesarRequest request)
        {
            var response = new CrAnulaAbonosProcesarResponse();
            var validacion = ValidarRequestProcesar(request, response);
            if (validacion != null)
                return validacion;

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                var operacion = ConsultarOperacion(conn, tx, request.id_solicitud);
                if (operacion == null)
                    return DbHelper.CreateErrorResponse("No se encontró la operación.", -2, response);

                if (PersonaCongelada(codEmpresa, operacion.cedula))
                    return DbHelper.CreateErrorResponse("Esta persona se encuentra congelada, verifique.", -2, response);

                if (!OperacionPermiteAnulacion(conn, tx, operacion.codigo))
                    return DbHelper.CreateErrorResponse("No se pueden realizar este tipo de movimientos a recaudos de ahorros extraordinarios, debe aplicarlos directamente al plan de ahorros de la persona.", -2, response);

                var destino = ResolverCuentaDestino(conn, tx, codEmpresa, request.accion);
                if (!destino.es_valido)
                    return DbHelper.CreateErrorResponse(destino.mensaje_error, -2, response);

                var ctas = conn.QueryFirstOrDefault<CrAnulaAbonosOperacionCtasData>(
                    "exec spCrdOperacionCtas @idSolicitud",
                    new { idSolicitud = request.id_solicitud },
                    tx);

                if (ctas == null)
                    return DbHelper.CreateErrorResponse("No se pudieron resolver las cuentas de la operación.", -2, response);

                var globales = _mProGrx.sbSifParametrosInicializa(codEmpresa, request.usuario).Result;
                if (globales == null)
                    return DbHelper.CreateErrorResponse("No se pudieron obtener los parámetros globales del usuario.", -2, response);

                string numDocumento = _mRecibos.FxDocumentoConsecutivo(codEmpresa, TipoDocumento).ToString();
                DateTime fechaServidor = conn.QueryFirstOrDefault<DateTime>("select dbo.MyGetdate()", transaction: tx);
                decimal montoTotal = ObtenerMontoTotal(request);

                var contexto = new AnulacionContext
                {
                    Conn = conn,
                    Tx = tx,
                    Request = request,
                    Operacion = operacion,
                    Ctas = ctas,
                    OficinaTitular = globales.GOficinaTitular,
                    Enlace = globales.GEnlace,
                    NumDocumento = numDocumento,
                    MontoTotal = montoTotal
                };

                InsertarDocumento(contexto);
                RegistrarAsientoSiMonto(contexto, request.int_corriente, ctas.ctaintc, "D");
                RegistrarAsientoSiMonto(contexto, request.int_morosidad, ctas.ctaintm, "D");
                RegistrarAsientoSiMonto(contexto, request.cargos, ctas.CtaCargos, "D");
                RegistrarAsientoSiMonto(contexto, request.poliza, ObtenerCuentaPoliza(conn, tx, ctas.ID_SOLICITUD), "D");
                RegistrarAsientoSiMonto(contexto, request.amortizacion, ctas.ctaamortiza, "D");
                RegistrarAsientoSiMonto(contexto, montoTotal, destino.cuenta, "C");
                RegistrarSaldoFavorSiAplica(contexto, destino);
                EjecutarAnulacionPlanPago(conn, tx, request, fechaServidor, numDocumento);

                tx.Commit();

                _bitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = request.usuario,
                    Movimiento = "Anula",
                    Modulo = VModulo,
                    DetalleMovimiento = $"OP: {request.id_solicitud} Doc.:{numDocumento} Total: {montoTotal:N2} Rec.Cuota.:{request.recalcular_cuota}"
                });

                var usuario = request.usuario ?? string.Empty;
                var trazabilidad = _mProGrx.sbTrazabilidad_Inserta(
                    codEmpresa,
                    "06",
                    numDocumento,
                    numDocumento,
                    usuario,
                    nuevo: true);

                if (trazabilidad.Code.HasValue && trazabilidad.Code != 0)
                    return DbHelper.CreateErrorResponse(
                        trazabilidad.Description ?? "No se pudo registrar la trazabilidad.",
                        trazabilidad.Code.Value,
                        response);

                var impresion = _mRecibos.sbImprimeRecibo(codEmpresa, numDocumento, TipoDocumento, usuario);

                response.tipo_documento = TipoDocumento;
                response.num_documento = numDocumento;
                response.monto_total = montoTotal;
                response.reporte_resultado = impresion.Code == -1 ? null : impresion.Result?.ToString();
                response.mensaje = impresion.Code == -1
                    ? $"Anulación realizada con Nota Débito {numDocumento}, pero no se pudo generar el recibo: {impresion.Description}"
                    : $"Anulación realizada ... Con Nota Débito: {numDocumento}";

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                try
                {
                    tx.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    return DbHelper.CreateErrorResponse(
                        $"Error al procesar la anulación: {ex.Message}. Además, no se pudo revertir la transacción: {rollbackEx.Message}",
                        -1,
                        response);
                }

                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        private static ErrorDto<CrAnulaAbonosProcesarResponse>? ValidarRequestProcesar(
            CrAnulaAbonosProcesarRequest request,
            CrAnulaAbonosProcesarResponse response)
        {
            if (request == null || request.id_solicitud <= 0 || string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.CreateErrorResponse("La solicitud de anulación es inválida.", -2, response);

            if (ObtenerMontoTotal(request) <= 0)
                return DbHelper.CreateErrorResponse("Debe indicar un monto de anulación mayor que cero.", -2, response);

            if (request.ultima_cuota_cancelada <= 0)
                return DbHelper.CreateErrorResponse("Debe seleccionar la última cuota cancelada.", -2, response);

            return null;
        }

        private static decimal ObtenerMontoTotal(CrAnulaAbonosProcesarRequest request)
        {
            return request.int_corriente + request.int_morosidad + request.amortizacion + request.cargos + request.poliza;
        }

        private static List<CrAnulaAbonosUltimaCuotaData> ConstruirUltimasCuotas(int fecUlt, int priDeduc)
        {
            var cuotas = new List<CrAnulaAbonosUltimaCuotaData>();
            var cursor = FechaProcesoSiguiente(fecUlt);

            for (var i = 0; i < 6; i++)
            {
                cursor = FechaProcesoAnterior(cursor);
                if (cursor >= priDeduc)
                    cuotas.Add(new CrAnulaAbonosUltimaCuotaData { fecha_proceso = cursor, descripcion = FormatearProceso(cursor) });
            }

            return cuotas;
        }

        private static int FechaProcesoSiguiente(int proceso)
        {
            var anio = proceso / 100;
            var mes = proceso % 100;
            mes++;
            if (mes <= 12) return anio * 100 + mes;
            return (anio + 1) * 100 + 1;
        }

        private static int FechaProcesoAnterior(int proceso)
        {
            var anio = proceso / 100;
            var mes = proceso % 100;
            mes--;
            if (mes >= 1) return anio * 100 + mes;
            return (anio - 1) * 100 + 12;
        }

        private static string FormatearProceso(int proceso)
        {
            return proceso > 0 ? $"{proceso / 100:0000}-{proceso % 100:00}" : string.Empty;
        }

        private static CrAnulaAbonosOperacionData? ConsultarOperacion(SqlConnection conn, SqlTransaction tx, int idSolicitud)
        {
            return conn.QueryFirstOrDefault<CrAnulaAbonosOperacionData>(@"
select
    R.id_solicitud,
    isnull(R.saldo,0) as saldo,
    rtrim(isnull(R.proceso,'')) as proceso,
    case rtrim(isnull(R.proceso,''))
        when 'N' then 'Normal'
        when 'T' then 'Traspaso Deuda'
        when 'J' then 'Cobro Judicial'
        when 'I' then 'Incobrable'
        else rtrim(isnull(R.proceso,''))
    end as proceso_desc,
    isnull(isnull(R.interesv,R.int),0) as interes,
    isnull(R.plazo,0) as plazo,
    isnull(R.prideduc,0) as prideduc,
    isnull(R.fecult,0) as fecult,
    convert(bit, case when isnull(R.opex,0) = 1 then 1 else 0 end) as opex,
    case when isnull(R.opex,0) = 1 then 'Op.Ex.' else 'Interno' end as opex_desc,
    rtrim(isnull(R.codigo,'')) as codigo,
    rtrim(isnull(R.cedula,'')) as cedula,
    rtrim(isnull(S.nombre,'')) as nombre,
    rtrim(isnull(C.descripcion,'')) as descripcion,
    convert(bit, case when isnull(C.retencion,'N') = 'S' or isnull(C.poliza,'N') = 'S' then 1 else 0 end) as retencion,
    rtrim(isnull(R.base_calculo,'')) as base_calculo
from reg_creditos R
inner join Catalogo C on R.codigo = C.codigo
inner join Socios S on R.cedula = S.cedula
where R.estado in('A','C')
  and R.ID_SOLICITUD = @idSolicitud;", new { idSolicitud }, tx);
        }

        private bool PersonaCongelada(int codEmpresa, string cedula)
        {
            return _mAfiliacion.fxgCongelamiento_Obtener(codEmpresa, cedula, "per_abono_cajas");
        }

        private static bool OperacionPermiteAnulacion(SqlConnection conn, SqlTransaction tx, string codigo)
        {
            return conn.QueryFirstOrDefault<int>(
                "select dbo.fxCrd_Operacion_Recaudo_Ahorro(@codigo)",
                new { codigo },
                tx) != 0;
        }

        private CuentaDestino ResolverCuentaDestino(SqlConnection conn, SqlTransaction tx, int codEmpresa, string accion)
        {
            if (string.Equals((accion ?? "S").Trim(), "S", StringComparison.OrdinalIgnoreCase))
            {
                var saldoFavor = conn.QueryFirstOrDefault<(string cod_forma_pago, string cod_cuenta)>(@"
select top 1
    rtrim(COD_FORMA_PAGO) as cod_forma_pago,
    rtrim(COD_CUENTA) as cod_cuenta
from SIF_FORMAS_PAGO
where TIPO = 'S' and Activa = 1;", transaction: tx);

                if (string.IsNullOrWhiteSpace(saldoFavor.cod_cuenta))
                    return CuentaDestino.Invalida("No existe una forma de pago activa para saldo a favor.");

                return CuentaDestino.SaldoFavor(saldoFavor.cod_cuenta, saldoFavor.cod_forma_pago);
            }

            string cuenta = _mRecibos.FxDocumentoCuenta(codEmpresa, TipoDocumento).Trim();
            return string.IsNullOrWhiteSpace(cuenta)
                ? CuentaDestino.Invalida("No se puede realizar movimiento porque no se especificó una cuenta contable válida para esta operación.")
                : CuentaDestino.Contable(cuenta);
        }

        private static string ObtenerCuentaPoliza(SqlConnection conn, SqlTransaction tx, int idSolicitud)
        {
            return conn.QueryFirstOrDefault<string>(
                "select isnull(dbo.fxCrdOperacionCtaContaPolizas(@idSolicitud),'')",
                new { idSolicitud },
                tx) ?? string.Empty;
        }

        private static void InsertarDocumento(AnulacionContext contexto)
        {
            var request = contexto.Request;
            var operacion = contexto.Operacion;
            var ctas = contexto.Ctas;
            var detalle = (request.notas ?? string.Empty).Trim();
            contexto.Conn.Execute(@"
insert SIF_TRANSACCIONES(
    COD_TRANSACCION,TIPO_DOCUMENTO,REGISTRO_FECHA,REGISTRO_USUARIO,Cliente_IDENTIFICACION,CLIENTE_NOMBRE,
    cod_concepto,monto,estado,Referencia_01,Referencia_02,Referencia_03,cod_oficina,
    linea1,linea2,linea3,linea4,linea5,linea6,linea7,linea8,linea9,linea10,linea11,detalle)
values(
    @numDocumento,@tipoDocumento,dbo.MyGetdate(),@usuario,@cedula,@nombre,
    @concepto,@montoTotal,'P',@idSolicitud,@codigo,'',@oficina,
    @linea1,@linea2,@linea3,@linea4,@linea5,@linea6,@linea7,@linea8,@linea9,@linea10,@linea11,@detalle);", new
            {
                numDocumento = contexto.NumDocumento,
                tipoDocumento = TipoDocumento,
                usuario = request.usuario,
                cedula = operacion.cedula,
                nombre = operacion.nombre,
                concepto = Concepto,
                montoTotal = contexto.MontoTotal,
                idSolicitud = request.id_solicitud.ToString(),
                operacion.codigo,
                oficina = contexto.OficinaTitular,
                linea1 = $"Saldo Actual      {ctas.Saldo:N2}",
                linea2 = $"Interes Corriente {request.int_corriente * -1:N2}",
                linea3 = $"Interes Moratorio {request.int_morosidad * -1:N2}",
                linea4 = $"Amortización      {request.amortizacion * -1:N2}",
                linea5 = $"Cargos            {request.cargos * -1:N2}",
                linea6 = $"Póliza            {request.poliza:N2}",
                linea7 = $"Nuevo Saldo       {ctas.Saldo + request.amortizacion:N2}",
                linea8 = $"Operación /Linea  {request.id_solicitud}_{operacion.codigo}_{operacion.opex_desc.ToUpperInvariant()}",
                linea9 = $"Proc.Retencion    {(operacion.retencion ? "SI" : "NO")}",
                linea10 = $"Usuario           {request.usuario}",
                linea11 = $"Fecha Ult. Cta    {FormatearProceso(request.ultima_cuota_cancelada)}",
                detalle
            }, contexto.Tx);
        }

        private static void RegistrarAsientoSiMonto(
            AnulacionContext contexto,
            decimal monto,
            string cuenta,
            string dc)
        {
            if (monto <= 0 || string.IsNullOrWhiteSpace(cuenta))
                return;

            var ctas = contexto.Ctas;
            contexto.Conn.Execute(@"
exec spSIFDocsAsiento
    @tipoDocumento,
    @numDocumento,
    @monto,
    @dc,
    @codDivisa,
    1,
    @enlace,
    @codUnidad,
    @codCentroCosto,
    @cuenta,
    @idSolicitud,
    @codigo,
    '';", new
            {
                tipoDocumento = TipoDocumento,
                numDocumento = contexto.NumDocumento,
                monto,
                dc,
                codDivisa = ctas.cod_Divisa,
                enlace = contexto.Enlace,
                codUnidad = ctas.Cod_Unidad,
                codCentroCosto = ctas.Cod_Centro_Costo,
                cuenta,
                idSolicitud = ctas.ID_SOLICITUD,
                codigo = ctas.Codigo
            }, contexto.Tx);
        }

        private static void RegistrarSaldoFavorSiAplica(
            AnulacionContext contexto,
            CuentaDestino destino)
        {
            if (!destino.requiere_saldo_favor)
                return;

            var request = contexto.Request;
            var ctas = contexto.Ctas;
            var sfId = contexto.Conn.QueryFirstOrDefault<int>(@"
exec spCajas_SaldoFavor_Registra
    @formaPago,
    @referencia,
    @monto,
    @cedula,
    @nombre,
    @usuario,
    @divisa;", new
            {
                formaPago = destino.forma_pago,
                referencia = $"{TipoDocumento}-{contexto.NumDocumento}",
                monto = contexto.MontoTotal,
                cedula = contexto.Operacion.cedula,
                nombre = contexto.Operacion.nombre,
                usuario = request.usuario,
                divisa = ctas.cod_Divisa
            }, contexto.Tx);

            contexto.Conn.Execute(@"
exec spSYS_Anulacion_Saldo_Favor
    @tipoDocumento,
    @numDocumento,
    @usuario,
    @formaPago,
    @divisa,
    @monto,
    @unidad,
    @cuenta,
    @referencia,
    @sfId;", new
            {
                tipoDocumento = TipoDocumento,
                numDocumento = contexto.NumDocumento,
                usuario = request.usuario,
                formaPago = destino.forma_pago,
                divisa = ctas.cod_Divisa,
                monto = contexto.MontoTotal,
                unidad = ctas.Cod_Unidad,
                cuenta = destino.cuenta,
                referencia = $"{TipoDocumento}-{contexto.NumDocumento}",
                sfId
            }, contexto.Tx);
        }

        private static void EjecutarAnulacionPlanPago(
            SqlConnection conn,
            SqlTransaction tx,
            CrAnulaAbonosProcesarRequest request,
            DateTime fechaServidor,
            string numDocumento)
        {
            conn.Execute(@"
exec spCrdPlanPagoAnulaAbono
    @idSolicitud,
    @concepto,
    @usuario,
    @tipoDocumento,
    @numDocumento,
    1,
    @intCorriente,
    @intMorosidad,
    @amortizacion,
    @cargos,
    @poliza,
    @fecha,
    '',
    1,
    @recalcularCuota,
    @ultimaCuotaCancelada,
    @notas;", new
            {
                idSolicitud = request.id_solicitud,
                concepto = Concepto,
                usuario = request.usuario,
                tipoDocumento = TipoDocumento,
                numDocumento,
                intCorriente = request.int_corriente,
                intMorosidad = request.int_morosidad,
                amortizacion = request.amortizacion,
                cargos = request.cargos,
                poliza = request.poliza,
                fecha = fechaServidor,
                recalcularCuota = request.recalcular_cuota,
                ultimaCuotaCancelada = request.ultima_cuota_cancelada,
                notas = (request.notas ?? string.Empty).Trim()
            }, tx);
        }

        private sealed class AnulacionContext
        {
            public SqlConnection Conn { get; init; } = null;
            public SqlTransaction Tx { get; init; } = null;
            public CrAnulaAbonosProcesarRequest Request { get; init; } = null;
            public CrAnulaAbonosOperacionData Operacion { get; init; } = null;
            public CrAnulaAbonosOperacionCtasData Ctas { get; init; } = null;
            public string OficinaTitular { get; init; } = string.Empty;
            public int Enlace { get; init; }
            public string NumDocumento { get; init; } = string.Empty;
            public decimal MontoTotal { get; init; }
        }

        private sealed class CuentaDestino
        {
            public bool es_valido { get; private set; }
            public bool requiere_saldo_favor { get; private set; }
            public string cuenta { get; private set; } = string.Empty;
            public string forma_pago { get; private set; } = string.Empty;
            public string mensaje_error { get; private set; } = string.Empty;

            public static CuentaDestino Contable(string cuenta) =>
                new() { es_valido = true, cuenta = cuenta };

            public static CuentaDestino SaldoFavor(string cuenta, string formaPago) =>
                new() { es_valido = true, requiere_saldo_favor = true, cuenta = cuenta, forma_pago = formaPago };

            public static CuentaDestino Invalida(string mensaje) =>
                new() { es_valido = false, mensaje_error = mensaje };
        }
    }
}
