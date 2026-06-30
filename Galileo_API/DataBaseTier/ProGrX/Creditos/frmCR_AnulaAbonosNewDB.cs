using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrAnulaAbonosNewDb
    {
        private readonly PortalDB _portalDb;
        private readonly MRecibos _mRecibos;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MAfilicacionDB _mAfiliacionDb;

        private const int ModuloCredito = 3;
        private const string TipoDocumentoNotaDebito = "ND";
        private const string ConceptoAnulacion = "CRD008";
        private const string MensajeOperacionRequerida = "Debe indicar la operaci&oacute;n.";
        private const string MensajeOperacionNoEncontrada = "No se encontr&oacute; la operaci&oacute;n.";

        public FrmCrAnulaAbonosNewDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mRecibos = new MRecibos(config);
            _mProGrxMain = new MProGrxMain(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mAfiliacionDb = new MAfilicacionDB(config);
        }

        /// <summary>
        /// Obtiene la operacion, sus movimientos anulables y el listado de ultimas cuotas canceladas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CrAnulaAbonosNewConsultaData> CrAnulaAbonosNew_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            var resultado = new CrAnulaAbonosNewConsultaData();

            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(MensajeOperacionRequerida, -2, resultado);
            }

            const string sqlOperacion = @"
            select top 1
                isnull(R.id_solicitud, 0) as operacion,
                rtrim(isnull(R.cedula, '')) as cedula,
                rtrim(isnull(S.nombre, '')) as nombre,
                rtrim(isnull(R.codigo, '')) as codigo,
                rtrim(isnull(C.descripcion, '')) as descripcion,
                rtrim(isnull(R.proceso, '')) as proceso,
                case rtrim(isnull(R.proceso, ''))
                    when 'N' then 'Normal'
                    when 'T' then 'Traspaso Deuda'
                    when 'J' then 'Cobro Judicial'
                    when 'I' then 'Incobrable'
                    else rtrim(isnull(R.proceso, ''))
                end as proceso_descripcion,
                isnull(R.opex, 0) as opex,
                case when isnull(R.opex, 0) = 1 then 'Op.Ex.' else 'Interno' end as opex_descripcion,
                isnull(R.saldo, 0) as saldo,
                isnull(isnull(R.interesv, R.int), 0) as interes,
                isnull(R.plazo, 0) as plazo,
                isnull(R.prideduc, 0) as prideduc,
                isnull(R.fecult, 0) as fecult,
                convert(bit, case when isnull(C.retencion, 'N') = 'S' or isnull(C.poliza, 'N') = 'S' then 1 else 0 end) as retencion,
                rtrim(isnull(R.base_calculo, '')) as base_calculo
            from reg_creditos R
            inner join Catalogo C
                on R.codigo = C.codigo
            inner join Socios S
                on R.cedula = S.cedula
            where R.estado in ('A', 'C')
              and R.id_solicitud = @Operacion;";

            var operacionResp = DbHelper.ExecuteSingleQuery<CrAnulaAbonosNewOperacionData>(
                _portalDb,
                codEmpresa,
                sqlOperacion,
                null,
                new { Operacion = operacion });

            if (operacionResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    operacionResp.Description ?? "No fue posible consultar la operaci&oacute;n.",
                    operacionResp.Code.GetValueOrDefault(-1),
                    resultado);
            }

            if (operacionResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(MensajeOperacionNoEncontrada, -2, resultado);
            }

            const string sqlMovimientos = @"
            select
                isnull(T.id_seq, 0) as id_seq,
                convert(varchar(7), T.fecha_proceso, 126) as fecha_proceso,
                isnull(T.num_cuota, 0) as num_cuota,
                isnull(T.cuota, 0) as cuota,
                case when isnull(T.mora_dias, 0) > 0 then 'En Mora' else 'Al Dia' end as estado,
                isnull(T.mov_intcor, 0) as mov_int_cor,
                isnull(T.mov_intmor, 0) as mov_int_mor,
                isnull(T.mov_principal, 0) as mov_principal,
                isnull(T.mov_cargos, 0) as mov_cargos,
                isnull(T.mov_poliza, 0) as mov_poliza,
                isnull(T.dias_calculo, 0) as dias_cor,
                isnull(T.mora_dias, 0) as dias_mor,
                rtrim(isnull(T.tipo_documento, '')) as tipo_documento,
                rtrim(isnull(T.num_comprobante, '')) as num_comprobante,
                T.mov_fecha,
                rtrim(isnull(T.mov_usuario, '')) as mov_usuario
            from CRD_OPERACION_TRANSAC T
            where T.estado = 'C'
              and T.id_solicitud = @Operacion
              and T.tipo_documento not in ('AJ')
              and T.mov_monto > 0
            order by T.id_seq desc;";

            var movimientosResp = DbHelper.ExecuteListQuery<CrAnulaAbonosNewMovimientoData>(
                _portalDb,
                codEmpresa,
                sqlMovimientos,
                new { Operacion = operacion });

            if (movimientosResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    movimientosResp.Description ?? "No fue posible consultar los movimientos.",
                    movimientosResp.Code.GetValueOrDefault(-1),
                    resultado);
            }

            resultado.operacion = operacionResp.Result;
            resultado.movimientos = movimientosResp.Result ?? [];
            resultado.ultimas_cuotas = CrAnulaAbonosNew_UltimasCuotas_Construir(
                operacionResp.Result.fecult,
                operacionResp.Result.prideduc);

            return DbHelper.CreateOkResponse(resultado);
        }

        /// <summary>
        /// Obtiene la cuenta contable recomendada para la anulacion del principal.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<string> CrAnulaAbonosNew_CuentaRecomendada_Obtener(
            int codEmpresa,
            CrAnulaAbonosNewCuentaRecomendadaRequest request)
        {
            if (request.operacion <= 0 || request.amortizacion <= 0)
            {
                return DbHelper.CreateOkResponse("...");
            }

            const string sql = @"
            select isnull(dbo.fxCrd_Operacion_Anula_Cta_Recomendada(@Operacion, @Amortizacion), '...') as cuenta;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                "...",
                new
                {
                    Operacion = request.operacion,
                    Amortizacion = request.amortizacion
                })!;
        }

        /// <summary>
        /// Ejecuta la anulacion de los movimientos seleccionados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrAnulaAbonosNewAplicarResultadoData> CrAnulaAbonosNew_Aplicar(
            int codEmpresa,
            CrAnulaAbonosNewAplicarRequest request)
        {
            var resultado = new CrAnulaAbonosNewAplicarResultadoData();

            request.usuario = CrAnulaAbonosNew_NormalizarTexto(request.usuario);
            request.accion = CrAnulaAbonosNew_NormalizarTexto(request.accion);
            request.notas = CrAnulaAbonosNew_NormalizarTexto(request.notas);

            var validacion = CrAnulaAbonosNew_Aplicar_Validar(request, resultado);
            if (validacion is not null)
            {
                return validacion;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var tx = conn.BeginTransaction();

                var operacion = CrAnulaAbonosNew_Operacion_Interna_Obtener(conn, tx, request.operacion);
                if (operacion is null)
                {
                    return DbHelper.CreateErrorResponse(MensajeOperacionNoEncontrada, -2, resultado);
                }

                if (_mAfiliacionDb.fxgCongelamiento(codEmpresa, operacion.cedula, "per_abono_cajas"))
                {
                    return DbHelper.CreateErrorResponse(
                        "Esta persona se encuentra congelada, verifique.",
                        -2,
                        resultado);
                }

                if (!CrAnulaAbonosNew_OperacionPermiteAnulacion(conn, tx, operacion.codigo))
                {
                    return DbHelper.CreateErrorResponse(
                        "No se pueden realizar este tipo de movimientos a recaudos de ahorros extraordinarios, debe aplicarlos directamente al plan de ahorros de la persona.",
                        -2,
                        resultado);
                }

                var cuentaDestino = CrAnulaAbonosNew_CuentaDestino_Resolver(conn, tx, codEmpresa, request.accion);
                if (!cuentaDestino.es_valida)
                {
                    return DbHelper.CreateErrorResponse(cuentaDestino.mensaje_error, -2, resultado);
                }

                var cuentasOperacion = conn.QueryFirstOrDefault<CrAnulaAbonosNewOperacionCtasData>(
                    "exec spCrdOperacionCtas @Operacion",
                    new { Operacion = request.operacion },
                    tx);

                if (cuentasOperacion is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No fue posible obtener las cuentas de la operaci&oacute;n.",
                        -2,
                        resultado);
                }

                var globalesResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, request.usuario);
                if (globalesResp.Code != 0 || globalesResp.Result is null)
                {
                    return DbHelper.CreateErrorResponse(
                        globalesResp.Description ?? "No fue posible obtener los par&aacute;metros globales.",
                        globalesResp.Code.GetValueOrDefault(-1),
                        resultado);
                }

                var numeroDocumento = _mRecibos.FxDocumentoConsecutivo(codEmpresa, TipoDocumentoNotaDebito).ToString();
                var fechaServidor = conn.QueryFirstOrDefault<DateTime>("select Getdate();", transaction: tx);
                var montoTotal = CrAnulaAbonosNew_Total_Calcular(request);
                var ultimaCuotaCancelada = CrAnulaAbonosNew_Proceso_Obtener(request.ult_cta_cancelada);

                var contexto = new CrAnulaAbonosNewAplicarContext
                {
                    conn = conn,
                    tx = tx,
                    request = request,
                    operacion = operacion,
                    cuentas = cuentasOperacion,
                    oficina_titular = globalesResp.Result.GOficinaTitular,
                    enlace = globalesResp.Result.GEnlace,
                    numero_documento = numeroDocumento,
                    fecha_servidor = fechaServidor,
                    monto_total = montoTotal,
                    ultima_cuota_cancelada = ultimaCuotaCancelada
                };

                CrAnulaAbonosNew_Documento_Insertar(contexto);
                CrAnulaAbonosNew_Asiento_Registrar(contexto, request.int_cor, cuentasOperacion.ctaintc, "D");
                CrAnulaAbonosNew_Asiento_Registrar(contexto, request.int_mor, cuentasOperacion.ctaintm, "D");
                CrAnulaAbonosNew_Asiento_Registrar(contexto, request.cargos, cuentasOperacion.CtaCargos, "D");
                CrAnulaAbonosNew_Asiento_Registrar(
                    contexto,
                    request.poliza,
                    CrAnulaAbonosNew_CuentaPoliza_Obtener(conn, tx, cuentasOperacion.id_solicitud),
                    "D");
                CrAnulaAbonosNew_Asiento_Registrar(contexto, request.amortizacion, cuentasOperacion.ctaamortiza, "D");
                CrAnulaAbonosNew_Asiento_Registrar(contexto, montoTotal, cuentaDestino.cuenta, "C");
                CrAnulaAbonosNew_SaldoFavor_Registrar(contexto, cuentaDestino);
                CrAnulaAbonosNew_PlanPago_Anular(contexto);

                tx.Commit();

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = request.usuario.ToUpperInvariant(),
                    Movimiento = "Anula",
                    Modulo = ModuloCredito,
                    DetalleMovimiento = $"OP: {request.operacion} Doc.:{numeroDocumento} Total: {montoTotal:N2} Rec.Cuota.:{request.recalcula_cuota}"
                });

                var trazabilidadResp = _mProGrxMain.sbTrazabilidad_Inserta(
                    codEmpresa,
                    "06",
                    numeroDocumento,
                    numeroDocumento,
                    request.usuario,
                    nuevo: true);

                if (trazabilidadResp.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        trazabilidadResp.Description ?? "No fue posible registrar la trazabilidad.",
                        trazabilidadResp.Code.GetValueOrDefault(-1),
                        resultado);
                }

                var impresionResp = _mRecibos.sbImprimeRecibo(
                    codEmpresa,
                    numeroDocumento,
                    TipoDocumentoNotaDebito,
                    request.usuario);

                resultado.tipo_documento = TipoDocumentoNotaDebito;
                resultado.num_documento = numeroDocumento;
                resultado.monto_total = montoTotal;
                resultado.reporte_resultado = impresionResp.Code == -1
                    ? null
                    : impresionResp.Result?.ToString();

                resultado.mensaje = impresionResp.Code == -1
                    ? $"Anulaci&oacute;n realizada con Nota D&eacute;bito {numeroDocumento}, pero no se pudo generar el recibo: {impresionResp.Description}"
                    : $"Anulaci&oacute;n realizada ... Con Nota D&eacute;bito: {numeroDocumento}";

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, resultado);
            }
        }

        private static ErrorDto<CrAnulaAbonosNewAplicarResultadoData>? CrAnulaAbonosNew_Aplicar_Validar(
            CrAnulaAbonosNewAplicarRequest request,
            CrAnulaAbonosNewAplicarResultadoData resultado)
        {
            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(MensajeOperacionRequerida, -2, resultado);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, resultado);
            }

            if (CrAnulaAbonosNew_Total_Calcular(request) <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No se ha especificado ning&uacute;n monto v&aacute;lido para anular.",
                    -2,
                    resultado);
            }

            if (CrAnulaAbonosNew_Proceso_Obtener(request.ult_cta_cancelada) <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la &uacute;ltima cuota cancelada.",
                    -2,
                    resultado);
            }

            return null;
        }

        private static decimal CrAnulaAbonosNew_Total_Calcular(CrAnulaAbonosNewAplicarRequest request)
        {
            return request.int_cor
                 + request.int_mor
                 + request.amortizacion
                 + request.cargos
                 + request.poliza;
        }

        private static CrAnulaAbonosNewOperacionData? CrAnulaAbonosNew_Operacion_Interna_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            int operacion)
        {
            const string sql = @"
            select top 1
                isnull(R.id_solicitud, 0) as operacion,
                rtrim(isnull(R.cedula, '')) as cedula,
                rtrim(isnull(S.nombre, '')) as nombre,
                rtrim(isnull(R.codigo, '')) as codigo,
                rtrim(isnull(C.descripcion, '')) as descripcion,
                rtrim(isnull(R.proceso, '')) as proceso,
                case rtrim(isnull(R.proceso, ''))
                    when 'N' then 'Normal'
                    when 'T' then 'Traspaso Deuda'
                    when 'J' then 'Cobro Judicial'
                    when 'I' then 'Incobrable'
                    else rtrim(isnull(R.proceso, ''))
                end as proceso_descripcion,
                isnull(R.opex, 0) as opex,
                case when isnull(R.opex, 0) = 1 then 'Op.Ex.' else 'Interno' end as opex_descripcion,
                isnull(R.saldo, 0) as saldo,
                isnull(isnull(R.interesv, R.int), 0) as interes,
                isnull(R.plazo, 0) as plazo,
                isnull(R.prideduc, 0) as prideduc,
                isnull(R.fecult, 0) as fecult,
                convert(bit, case when isnull(C.retencion, 'N') = 'S' or isnull(C.poliza, 'N') = 'S' then 1 else 0 end) as retencion,
                rtrim(isnull(R.base_calculo, '')) as base_calculo
            from reg_creditos R
            inner join Catalogo C
                on R.codigo = C.codigo
            inner join Socios S
                on R.cedula = S.cedula
            where R.estado in ('A', 'C')
              and R.id_solicitud = @Operacion;";

            return conn.QueryFirstOrDefault<CrAnulaAbonosNewOperacionData>(
                sql,
                new { Operacion = operacion },
                tx);
        }

        private static bool CrAnulaAbonosNew_OperacionPermiteAnulacion(
            SqlConnection conn,
            SqlTransaction tx,
            string codigo)
        {
            return conn.QueryFirstOrDefault<int>(
                "select dbo.fxCrd_Operacion_Recaudo_Ahorro(@Codigo);",
                new { Codigo = codigo },
                tx) != 0;
        }

        private CrAnulaAbonosNewCuentaDestinoData CrAnulaAbonosNew_CuentaDestino_Resolver(
            SqlConnection conn,
            SqlTransaction tx,
            int codEmpresa,
            string accion)
        {
            if (string.Equals(accion, "S", StringComparison.OrdinalIgnoreCase))
            {
                const string sql = @"
                select top 1
                    rtrim(isnull(COD_FORMA_PAGO, '')) as forma_pago,
                    rtrim(isnull(COD_CUENTA, '')) as cuenta
                from SIF_FORMAS_PAGO
                where TIPO = 'S'
                  and Activa = 1;";

                var saldoFavor = conn.QueryFirstOrDefault<CrAnulaAbonosNewSaldoFavorData>(sql, transaction: tx);

                if (saldoFavor is null || string.IsNullOrWhiteSpace(saldoFavor.cuenta))
                {
                    return CrAnulaAbonosNewCuentaDestinoData.Invalida(
                        "No existe una forma de pago activa para saldo a favor.");
                }

                return CrAnulaAbonosNewCuentaDestinoData.SaldoFavor(
                    saldoFavor.cuenta,
                    saldoFavor.forma_pago);
            }

            var cuenta = CrAnulaAbonosNew_NormalizarTexto(
                _mRecibos.FxDocumentoCuenta(codEmpresa, TipoDocumentoNotaDebito));

            return string.IsNullOrWhiteSpace(cuenta)
                ? CrAnulaAbonosNewCuentaDestinoData.Invalida(
                    "No se puede realizar movimiento porque no se especific&oacute; una cuenta contable v&aacute;lida para esta operaci&oacute;n.")
                : CrAnulaAbonosNewCuentaDestinoData.Contable(cuenta);
        }

        private static string CrAnulaAbonosNew_CuentaPoliza_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            int operacion)
        {
            return conn.QueryFirstOrDefault<string>(
                "select isnull(dbo.fxCrdOperacionCtaContaPolizas(@Operacion), '');",
                new { Operacion = operacion },
                tx) ?? string.Empty;
        }

        private static void CrAnulaAbonosNew_Documento_Insertar(CrAnulaAbonosNewAplicarContext context)
        {
            var request = context.request;
            var operacion = context.operacion;
            var cuentas = context.cuentas;

            context.conn.Execute(@"
            insert SIF_TRANSACCIONES
            (
                COD_TRANSACCION,
                TIPO_DOCUMENTO,
                REGISTRO_FECHA,
                REGISTRO_USUARIO,
                Cliente_IDENTIFICACION,
                CLIENTE_NOMBRE,
                cod_concepto,
                monto,
                estado,
                Referencia_01,
                Referencia_02,
                Referencia_03,
                cod_oficina,
                linea1,
                linea2,
                linea3,
                linea4,
                linea5,
                linea6,
                linea7,
                linea8,
                linea9,
                linea10,
                linea11,
                detalle
            )
            values
            (
                @NumDocumento,
                @TipoDocumento,
                Getdate(),
                @Usuario,
                @Cedula,
                @Nombre,
                @Concepto,
                @MontoTotal,
                'P',
                @Operacion,
                @Codigo,
                '',
                @Oficina,
                @Linea1,
                @Linea2,
                @Linea3,
                @Linea4,
                @Linea5,
                @Linea6,
                @Linea7,
                @Linea8,
                @Linea9,
                @Linea10,
                @Linea11,
                @Detalle
            );",
            new
            {
                NumDocumento = context.numero_documento,
                TipoDocumento = TipoDocumentoNotaDebito,
                Usuario = request.usuario,
                Cedula = operacion.cedula,
                Nombre = operacion.nombre,
                Concepto = ConceptoAnulacion,
                MontoTotal = context.monto_total,
                Operacion = request.operacion.ToString(),
                Codigo = operacion.codigo,
                Oficina = context.oficina_titular,
                Linea1 = $"Saldo Actual      {cuentas.Saldo:N2}",
                Linea2 = $"Interes Corriente {request.int_cor * -1:N2}",
                Linea3 = $"Interes Moratorio {request.int_mor * -1:N2}",
                Linea4 = $"Amortizacion      {request.amortizacion * -1:N2}",
                Linea5 = $"Cargos            {request.cargos * -1:N2}",
                Linea6 = $"Poliza            {request.poliza:N2}",
                Linea7 = $"Nuevo Saldo       {cuentas.Saldo + request.amortizacion:N2}",
                Linea8 = $"Operacion /Linea  {request.operacion}_{operacion.codigo}_{operacion.opex_descripcion.ToUpperInvariant()}",
                Linea9 = $"Proc.Retencion    {(operacion.retencion ? "SI" : "NO")}",
                Linea10 = $"Usuario           {request.usuario}",
                Linea11 = $"Fecha Ult. Cta    {CrAnulaAbonosNew_Proceso_Formatear(context.ultima_cuota_cancelada)}",
                Detalle = request.notas
            },
            context.tx);
        }

        private static void CrAnulaAbonosNew_Asiento_Registrar(
            CrAnulaAbonosNewAplicarContext context,
            decimal monto,
            string cuenta,
            string tipo)
        {
            if (monto <= 0 || string.IsNullOrWhiteSpace(cuenta))
            {
                return;
            }

            var cuentas = context.cuentas;

            context.conn.Execute(@"
            exec spSIFDocsAsiento
                @TipoDocumento,
                @NumDocumento,
                @Monto,
                @Tipo,
                @Divisa,
                1,
                @Enlace,
                @Unidad,
                @CentroCosto,
                @Cuenta,
                @Operacion,
                @Codigo,
                '';",
            new
            {
                TipoDocumento = TipoDocumentoNotaDebito,
                NumDocumento = context.numero_documento,
                Monto = monto,
                Tipo = tipo,
                Divisa = cuentas.cod_Divisa,
                Enlace = context.enlace,
                Unidad = cuentas.Cod_Unidad,
                CentroCosto = cuentas.Cod_Centro_Costo,
                Cuenta = cuenta,
                Operacion = cuentas.id_solicitud,
                Codigo = cuentas.Codigo
            },
            context.tx);
        }

        private static void CrAnulaAbonosNew_SaldoFavor_Registrar(
            CrAnulaAbonosNewAplicarContext context,
            CrAnulaAbonosNewCuentaDestinoData cuentaDestino)
        {
            if (!cuentaDestino.requiere_saldo_favor)
            {
                return;
            }

            var cuentas = context.cuentas;
            var sfId = context.conn.QueryFirstOrDefault<int>(@"
            exec spCajas_SaldoFavor_Registra
                @FormaPago,
                @Referencia,
                @Monto,
                @Cedula,
                @Nombre,
                @Usuario,
                @Divisa;",
            new
            {
                FormaPago = cuentaDestino.forma_pago,
                Referencia = $"{TipoDocumentoNotaDebito}-{context.numero_documento}",
                Monto = context.monto_total,
                Cedula = context.operacion.cedula,
                Nombre = context.operacion.nombre,
                Usuario = context.request.usuario,
                Divisa = cuentas.cod_Divisa
            },
            context.tx);

            context.conn.Execute(@"
            exec spSYS_Anulacion_Saldo_Favor
                @TipoDocumento,
                @NumDocumento,
                @Usuario,
                @FormaPago,
                @Divisa,
                @Monto,
                @Unidad,
                @Cuenta,
                @Referencia,
                @SfId;",
            new
            {
                TipoDocumento = TipoDocumentoNotaDebito,
                NumDocumento = context.numero_documento,
                Usuario = context.request.usuario,
                FormaPago = cuentaDestino.forma_pago,
                Divisa = cuentas.cod_Divisa,
                Monto = context.monto_total,
                Unidad = cuentas.Cod_Unidad,
                Cuenta = cuentaDestino.cuenta,
                Referencia = $"{TipoDocumentoNotaDebito}-{context.numero_documento}",
                SfId = sfId
            },
            context.tx);
        }

        private static void CrAnulaAbonosNew_PlanPago_Anular(CrAnulaAbonosNewAplicarContext context)
        {
            var request = context.request;

            context.conn.Execute(@"
            exec spCrdPlanPagoAnulaAbono
                @Operacion,
                @Concepto,
                @Usuario,
                @TipoDocumento,
                @NumDocumento,
                1,
                @IntCor,
                @IntMor,
                @Amortizacion,
                @Cargos,
                @Poliza,
                @Fecha,
                '',
                1,
                @RecalculaCuota,
                @UltimaCuotaCancelada,
                @Notas;",
            new
            {
                Operacion = request.operacion,
                Concepto = ConceptoAnulacion,
                Usuario = request.usuario,
                TipoDocumento = TipoDocumentoNotaDebito,
                NumDocumento = context.numero_documento,
                IntCor = request.int_cor,
                IntMor = request.int_mor,
                Amortizacion = request.amortizacion,
                Cargos = request.cargos,
                Poliza = request.poliza,
                Fecha = context.fecha_servidor,
                RecalculaCuota = request.recalcula_cuota,
                UltimaCuotaCancelada = context.ultima_cuota_cancelada,
                Notas = request.notas
            },
            context.tx);
        }

        private static List<DropDownListaGenericaModel> CrAnulaAbonosNew_UltimasCuotas_Construir(
            int fecUlt,
            int priDeduc)
        {
            var lista = new List<DropDownListaGenericaModel>();
            var proceso = CrAnulaAbonosNew_Proceso_Siguiente(fecUlt);

            for (var i = 0; i < 6; i++)
            {
                proceso = CrAnulaAbonosNew_Proceso_Anterior(proceso);

                if (proceso >= priDeduc)
                {
                    lista.Add(new DropDownListaGenericaModel
                    {
                        item = proceso.ToString(),
                        descripcion = CrAnulaAbonosNew_Proceso_Formatear(proceso)
                    });
                }
            }

            return lista;
        }

        private static int CrAnulaAbonosNew_Proceso_Siguiente(int proceso)
        {
            var anio = proceso / 100;
            var mes = proceso % 100;

            mes++;
            if (mes <= 12)
            {
                return (anio * 100) + mes;
            }

            return ((anio + 1) * 100) + 1;
        }

        private static int CrAnulaAbonosNew_Proceso_Anterior(int proceso)
        {
            var anio = proceso / 100;
            var mes = proceso % 100;

            mes--;
            if (mes >= 1)
            {
                return (anio * 100) + mes;
            }

            return ((anio - 1) * 100) + 12;
        }

        private static int CrAnulaAbonosNew_Proceso_Obtener(string valor)
        {
            var soloDigitos = new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
            return int.TryParse(soloDigitos, out var proceso) ? proceso : 0;
        }

        private static string CrAnulaAbonosNew_Proceso_Formatear(int proceso)
        {
            return proceso > 0
                ? $"{proceso / 100:0000}-{proceso % 100:00}"
                : string.Empty;
        }

        private static string CrAnulaAbonosNew_NormalizarTexto(string valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private sealed class CrAnulaAbonosNewAplicarContext
        {
            public SqlConnection conn { get; set; } = null!;
            public SqlTransaction tx { get; set; } = null!;
            public CrAnulaAbonosNewAplicarRequest request { get; set; } = null!;
            public CrAnulaAbonosNewOperacionData operacion { get; set; } = null!;
            public CrAnulaAbonosNewOperacionCtasData cuentas { get; set; } = null!;
            public string oficina_titular { get; set; } = string.Empty;
            public int enlace { get; set; }
            public string numero_documento { get; set; } = string.Empty;
            public DateTime fecha_servidor { get; set; }
            public decimal monto_total { get; set; }
            public int ultima_cuota_cancelada { get; set; }
        }

        private sealed class CrAnulaAbonosNewCuentaDestinoData
        {
            public bool es_valida { get; private set; }
            public bool requiere_saldo_favor { get; private set; }
            public string cuenta { get; private set; } = string.Empty;
            public string forma_pago { get; private set; } = string.Empty;
            public string mensaje_error { get; private set; } = string.Empty;

            public static CrAnulaAbonosNewCuentaDestinoData Contable(string cuenta)
            {
                return new CrAnulaAbonosNewCuentaDestinoData
                {
                    es_valida = true,
                    cuenta = cuenta
                };
            }

            public static CrAnulaAbonosNewCuentaDestinoData SaldoFavor(string cuenta, string formaPago)
            {
                return new CrAnulaAbonosNewCuentaDestinoData
                {
                    es_valida = true,
                    requiere_saldo_favor = true,
                    cuenta = cuenta,
                    forma_pago = formaPago
                };
            }

            public static CrAnulaAbonosNewCuentaDestinoData Invalida(string mensaje)
            {
                return new CrAnulaAbonosNewCuentaDestinoData
                {
                    es_valida = false,
                    mensaje_error = mensaje
                };
            }
        }

        private sealed class CrAnulaAbonosNewSaldoFavorData
        {
            public string forma_pago { get; set; } = string.Empty;
            public string cuenta { get; set; } = string.Empty;
        }
    }
}