using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    /// <summary>
    /// Traslado de deuda (cobro a fiadores): listado de operaciones trasladadas y reversión.
    /// Origen VB6: frmCO_Principal (OperacionesGeneradas / cmdReversaTraspasoDeudas_Click).
    /// </summary>
    public partial class FrmCOPrincipalDB
    {
        private const int ModuloCobro = 4;
        private const string FormatoFechaHora = "yyyy/MM/dd HH:mm:ss";
        private const string ConceptoReversion = "CBR003";
        private const string TipoDocumentoTraslado = "TRA";
        private const string TipoDocumentoV1 = "4";
        private const string NumeroDocumentoV1 = "8888";
        private const string ObservacionPorDefecto = "NADA";
        private const string ParametroCuentaCargos = "23";

        /// <summary>
        /// Obtiene las operaciones trasladadas (spCbr_Operaciones_Trasladadas).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<COTrasladoDeudaRowDto>> TrasladoDeuda_Listar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<List<COTrasladoDeudaRowDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var filas = ObtenerOperacionesTrasladadas(cn, operacion);
                var estados = ObtenerEstadosOperaciones(cn, filas.Select(x => x.Id_Solicitud).ToList());

                response.Result = filas.Select(x => new COTrasladoDeudaRowDto
                {
                    operacion = x.Id_Solicitud,
                    linea = (x.Codigo ?? string.Empty).Trim(),
                    cedula = (x.Cedula ?? string.Empty).Trim(),
                    monto = x.Monto,
                    saldo = x.Saldo,
                    cuota = x.Cuota,
                    tasa = x.Tasa,
                    plazo = x.Plazo,
                    interesPendiente = x.C_Intereses,
                    cargos = x.C_Cargos + x.C_Iva,
                    polizas = x.C_Polizas,
                    estado = estados.GetValueOrDefault(x.Id_Solicitud, string.Empty),
                    nombre = (x.Nombre ?? string.Empty).Trim()
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Revierte el traspaso de deudas de los fiadores marcados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<string> TrasladoDeuda_Revertir(int codEmpresa, COTrasladoDeudaRevertirRequestDto request)
        {
            var response = new ErrorDto<string>();
            SqlTransaction? tx = null;

            try
            {
                var errorRequest = ValidarSolicitudReversion(request);
                if (errorRequest != null)
                    return DbHelper.CreateErrorResponse<string>(errorRequest);

                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));
                cn.Open();

                var (ctx, errorContexto) = CrearContextoReversion(cn, codEmpresa, request);
                if (ctx == null)
                    return DbHelper.CreateErrorResponse<string>(errorContexto ?? "No fue posible preparar la reversión.");

                RegistrarDocumentoReversion(cn, ctx);

                tx = cn.BeginTransaction();
                foreach (var fiador in ctx.Fiadores)
                {
                    ReversarFiador(cn, tx, ctx, fiador);
                }
                tx.Commit();
                tx.Dispose();
                tx = null;

                RegistrarAsientosReversion(cn, ctx);
                FinalizarReversion(cn, ctx);

                response.Result =
                    $"Reversión de Traspaso realizada satisfactoriamente. Revisar nota de Traspaso No.: {ctx.NumeroDocumento}";
            }
            catch (Exception ex)
            {
                tx?.Rollback();
                tx?.Dispose();
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #region Reversión - preparación

        private static string? ValidarSolicitudReversion(COTrasladoDeudaRevertirRequestDto? request)
        {
            if (request == null || request.operacion <= 0)
                return "Debe indicar la operación.";

            if (string.IsNullOrWhiteSpace(request.usuario))
                return "Debe indicar el usuario.";

            if (request.nuevomonto <= 0m)
                return "El monto de la reversión debe ser mayor que cero.";

            if (request.operacionesseleccionadas == null || request.operacionesseleccionadas.Count == 0)
                return "No se ha marcado ninguna (deuda) de fiador, para reversión verifique...";

            return null;
        }

        private (ReversionContexto? Contexto, string? Error) CrearContextoReversion(
            SqlConnection cn, int codEmpresa, COTrasladoDeudaRevertirRequestDto request)
        {
            var cabecera = cn.QueryFirstOrDefault<CabeceraReversionRow>(@"
                select rtrim(r.cedula) as cedula, rtrim(r.codigo) as codigo,
                       isnull(r.saldo, 0) as saldo, rtrim(isnull(s.nombre, '')) as nombre,
                       isnull(r.cod_divisa, 'COL') as divisa
                  from reg_creditos r
                  left join socios s on r.cedula = s.cedula
                 where r.id_solicitud = @operacion;",
                new { operacion = request.operacion }, commandTimeout: 0);

            if (cabecera == null)
                return (null, "La operación no existe.");

            var congelado = cn.ExecuteScalar<int>(
                ObtenerSqlValidarCongelamiento("per_reversiones"),
                new { cedula = cabecera.Cedula }, commandTimeout: 0);
            if (congelado > 0)
                return (null, "Esta persona se encuentra CONGELADA, verifique...");

            var fiadores = ObtenerOperacionesTrasladadas(cn, request.operacion)
                .Where(x => x.Id_Solicitud != request.operacion
                         && request.operacionesseleccionadas.Contains(x.Id_Solicitud))
                .ToList();
            if (fiadores.Count == 0)
                return (null, "No se ha marcado ninguna (deuda) de fiador, para reversión verifique...");

            var cuentas = cn.QueryFirstOrDefault<CuentasOperacionRow>(
                "exec spCrdOperacionCtas @operacion", new { operacion = request.operacion }, commandTimeout: 0)
                ?? new CuentasOperacionRow();

            var observacion = (request.observacion ?? string.Empty).Trim();
            if (observacion.Length == 0)
                observacion = ObservacionPorDefecto;

            var ctx = new ReversionContexto
            {
                CodEmpresa = codEmpresa,
                Request = request,
                Cabecera = cabecera,
                Cuentas = cuentas,
                Fiadores = fiadores,
                Observacion = observacion,
                Usuario = request.usuario.Trim().ToUpperInvariant(),
                Fecha = FrmCOTrasladoDeudaDB.ObtenerFechaServidor(cn),
                Contabilidad = FrmCOTrasladoDeudaDB.ObtenerContabilidadEnlace(cn),
                SysPlanPagos = FrmCOTrasladoDeudaDB.ObtenerSysPlanPagos(cn),
                OficinaTitular = FrmCOTrasladoDeudaDB.ObtenerOficinaTitular(cn),
                FechaProceso = cn.QueryFirstOrDefault<decimal>(
                    "select dbo.fxSIFDateTimeToProceso(dbo.MyGetdate());", commandTimeout: 0),
                SysDocVersion = cn.QueryFirstOrDefault<int>(
                    "select isnull(SysDocVersion, 0) from SIF_EMPRESA where PORTAL_ID = @codEmpresa;",
                    new { codEmpresa }, commandTimeout: 0)
            };

            return (ctx, null);
        }

        private static List<TrasladadaRow> ObtenerOperacionesTrasladadas(SqlConnection cn, int operacion)
        {
            return cn.Query<TrasladadaRow>(
                "exec spCbr_Operaciones_Trasladadas @operacion",
                new { operacion }, commandTimeout: 0).ToList();
        }

        private static Dictionary<int, string> ObtenerEstadosOperaciones(SqlConnection cn, List<int> ids)
        {
            if (ids.Count == 0)
                return new Dictionary<int, string>();

            const string sql = @"
                select rc.id_solicitud as Id,
                       case
                           when rc.estado = 'C' then 'CANCELADO'
                           when rc.proceso = 'J' then 'COBRO JUDICIAL'
                           when rc.proceso = 'T' then 'TRASPASO DEUDAS'
                           when rc.estadosol = 'F' then 'NORMAL'
                           else isnull(rtrim(rc.estado), '')
                       end as Estado
                  from reg_creditos rc
                 where rc.id_solicitud in @ids;";

            return cn.Query<EstadoOperacionRow>(sql, new { ids }, commandTimeout: 0)
                     .ToDictionary(x => x.Id, x => x.Estado);
        }

        private void RegistrarDocumentoReversion(SqlConnection cn, ReversionContexto ctx)
        {
            if (ctx.SysDocVersion == 1)
            {
                // Control de Documentos v1: no se registra en SIF_TRANSACCIONES.
                ctx.TipoDocumento = TipoDocumentoV1;
                ctx.NumeroDocumento = NumeroDocumentoV1;
                return;
            }

            var consecutivo = new MRecibos(_config).FxDocumentoConsecutivo(ctx.CodEmpresa, TipoDocumentoTraslado);
            if (consecutivo <= 0)
                throw new InvalidOperationException("No se pudo obtener el consecutivo del documento.");

            ctx.TipoDocumento = TipoDocumentoTraslado;
            ctx.NumeroDocumento = consecutivo.ToString(CultureInfo.InvariantCulture);

            var saldo = ctx.Cabecera.Saldo.ToString("N2", CultureInfo.InvariantCulture);
            var cero = 0m.ToString("N2", CultureInfo.InvariantCulture);

            cn.Execute(@"
                insert into SIF_TRANSACCIONES
                (
                    COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
                    CLIENTE_IDENTIFICACION, CLIENTE_NOMBRE, COD_CONCEPTO, MONTO, ESTADO,
                    REFERENCIA_01, REFERENCIA_02, REFERENCIA_03, COD_OFICINA,
                    LINEA1, LINEA2, LINEA3, LINEA4, LINEA5, LINEA6,
                    LINEA7, LINEA8, LINEA9, LINEA10, DETALLE, DOCUMENTO, LINEA11
                )
                values
                (
                    @numero, @tipo, dbo.MyGetdate(), @usuario,
                    @cedula, @nombre, @concepto, @monto, 'P',
                    @operacion, @linea, '', @oficina,
                    @linea1, @linea2, @linea3, @linea4, @linea5, @linea6,
                    @linea7, @linea8, @linea9, @linea10, @detalle, '', @linea11
                );",
                new
                {
                    numero = ctx.NumeroDocumento,
                    tipo = ctx.TipoDocumento,
                    usuario = ctx.Usuario,
                    cedula = ctx.Cabecera.Cedula,
                    nombre = ctx.Cabecera.Nombre,
                    concepto = ConceptoReversion,
                    monto = ctx.Request.nuevomonto,
                    operacion = ctx.Request.operacion.ToString(CultureInfo.InvariantCulture),
                    linea = ctx.Cabecera.Codigo,
                    oficina = ctx.OficinaTitular,
                    linea1 = $"Saldo Anterior    {saldo}",
                    linea2 = $"Interes Corriente {cero}",
                    linea3 = $"Interes Moratorio {cero}",
                    linea4 = $"Cargos            {cero}",
                    linea5 = $"Amortizacion      {saldo}",
                    linea6 = $"Saldo Actual      {cero}",
                    linea7 = $"Operación         {ctx.Request.operacion}",
                    linea8 = $"Línea             {ctx.Cabecera.Codigo}",
                    linea9 = "Proc.Retencion    NO",
                    linea10 = $"Usuario           {ctx.Usuario}",
                    linea11 = $"Póliza            {cero}",
                    detalle = ctx.Observacion
                }, commandTimeout: 0);
        }

        #endregion

        #region Reversión - fiadores (dentro de la transacción)

        private static void ReversarFiador(SqlConnection cn, SqlTransaction tx, ReversionContexto ctx, TrasladadaRow fiador)
        {
            if (fiador.Saldo + fiador.C_Intereses <= 0m)
                return;

            if (ctx.SysPlanPagos == 1)
                ReversarFiadorConPlanPagos(cn, tx, ctx, fiador);
            else
                ReversarFiadorSinPlanPagos(cn, tx, ctx, fiador);
        }

        private static void ReversarFiadorConPlanPagos(SqlConnection cn, SqlTransaction tx, ReversionContexto ctx, TrasladadaRow fiador)
        {
            var fecha = ctx.Fecha.ToString(FormatoFechaHora, CultureInfo.InvariantCulture);

            cn.Execute("exec spCrdPlanPagosMoraActualizaOp @operacion, @fecha",
                new { operacion = fiador.Id_Solicitud, fecha }, tx, commandTimeout: 0);

            var info = cn.QueryFirst<CancelacionInfoRow>(
                "exec spCrdPlanPagosInfoCancelacion @operacion, @fecha",
                new { operacion = fiador.Id_Solicitud, fecha }, tx, commandTimeout: 0);
            var total = info.IntCor + info.IntMor + info.Cargos + info.Poliza + info.Principal;

            // Abono de cancelación de la operación del fiador
            cn.Execute(
                "exec spCrdPlanPagoAbonoCancelacion @operacion, @concepto, @usuario, @tipo, @numero, @total, @fecha, ''",
                new
                {
                    operacion = fiador.Id_Solicitud,
                    concepto = ConceptoReversion,
                    usuario = ctx.Usuario,
                    tipo = ctx.TipoDocumento,
                    numero = ctx.NumeroDocumento,
                    total,
                    fecha
                }, tx, commandTimeout: 0);

            // Ajuste de la operación del deudor (original)
            cn.Execute(
                "exec spCrdPlanPagoAnulaAbono @operacion, @concepto, @usuario, @tipo, @numero, 1, 0, 0, @total, 0, 0, @fecha, '', 1, 1",
                new
                {
                    operacion = ctx.Request.operacion,
                    concepto = ConceptoReversion,
                    usuario = ctx.Usuario,
                    tipo = ctx.TipoDocumento,
                    numero = ctx.NumeroDocumento,
                    total,
                    fecha
                }, tx, commandTimeout: 0);
        }

        private static void ReversarFiadorSinPlanPagos(SqlConnection cn, SqlTransaction tx, ReversionContexto ctx, TrasladadaRow fiador)
        {
            var mora = cn.QueryFirst<MorosidadSumaRow>(@"
                select isnull(sum(intc), 0) as IntC, isnull(sum(intm), 0) as IntM,
                       isnull(sum(amortiza), 0) as Amortiza, isnull(sum(cargo), 0) as Cargo
                  from morosidad
                 where estado = 'A' and id_solicitud = @operacion;",
                new { operacion = fiador.Id_Solicitud }, tx, commandTimeout: 0);

            var intPendiente = fiador.C_Intereses - (mora.IntC + mora.IntM);
            var amortiza = fiador.Saldo - mora.Amortiza;
            var totalFiador = fiador.Saldo + fiador.C_Intereses;

            var p = new
            {
                operacionFiador = fiador.Id_Solicitud,
                operacion = ctx.Request.operacion,
                codigoFiador = fiador.Codigo,
                codigoDeudor = ctx.Cabecera.Codigo,
                usuario = ctx.Usuario,
                concepto = ConceptoReversion,
                tipo = ctx.TipoDocumento,
                numero = ctx.NumeroDocumento,
                saldo = fiador.Saldo,
                intereses = fiador.C_Intereses,
                intPendiente,
                amortiza,
                abono = amortiza + intPendiente,
                totalFiador,
                totalFiadorNegativo = totalFiador * -1,
                fechaProceso = ctx.FechaProceso,
                observacion = ctx.Observacion.Length > 255 ? ctx.Observacion[..255] : ctx.Observacion
            };

            // Cancela la mora y la operación del fiador
            cn.Execute(@"
                update morosidad
                   set abintc = intc, abintm = intm, abamortiza = amortiza, abcargo = cargo,
                       usuario = @usuario, cod_concepto = @concepto, cod_caja = '',
                       estado = 'C', tcon = @tipo, ncon = @numero, fecult = dbo.MyGetdate()
                 where estado = 'A' and id_solicitud = @operacionFiador;", p, tx, commandTimeout: 0);

            cn.Execute(@"
                update reg_creditos
                   set saldo = saldo - @saldo, amortiza = amortiza + @saldo,
                       interesc = interesc + @intereses, estado = 'C', proceso = 'N',
                       fecha_enviaproceso = dbo.MyGetdate(), observacion_proceso = @observacion
                 where id_solicitud = @operacionFiador;", p, tx, commandTimeout: 0);

            cn.Execute(@"
                insert creditos_dt(CODIGO, ID_SOLICITUD, CUOTA, ABONO, INTCP, AMORTIZA, FECHAS,
                                   FECHAP, TCON, NCON, ESTADO, ESTADO_ASIENTO, usuario, cod_concepto, cod_caja)
                values(@codigoFiador, @operacionFiador, 0, @abono, @intPendiente, @amortiza, dbo.MyGetdate(),
                       @fechaProceso, @tipo, @numero, 'A', 'G', @usuario, @concepto, '');", p, tx, commandTimeout: 0);

            // Ajuste de la operación del deudor (original)
            cn.Execute(@"
                update reg_creditos
                   set proceso = 'N', estado = 'A', saldo = saldo + @totalFiador
                 where id_solicitud = @operacion;", p, tx, commandTimeout: 0);

            cn.Execute(@"
                insert creditos_dt(CODIGO, ID_SOLICITUD, CUOTA, ABONO, INTCP, AMORTIZA, FECHAS,
                                   FECHAP, TCON, NCON, ESTADO, ESTADO_ASIENTO, usuario, cod_concepto, cod_caja)
                values(@codigoDeudor, @operacion, 0, @totalFiadorNegativo, 0, @totalFiadorNegativo, dbo.MyGetdate(),
                       @fechaProceso, @tipo, @numero, 'A', 'G', @usuario, @concepto, '');", p, tx, commandTimeout: 0);
        }

        #endregion

        #region Reversión - asientos y cierre (posterior al commit)

        private void RegistrarAsientosReversion(SqlConnection cn, ReversionContexto ctx)
        {
            var filas = cn.Query<AsientoReversionRow>(@"
                select V.id_solicitud as Id_Solicitud, V.codigo as Codigo,
                       Ofi.COD_UNIDAD as Cod_Unidad, Ofi.COD_CENTRO_COSTO as Cod_Centro_Costo,
                       SUM(V.IntCor) as IntCor, SUM(V.IntMor) as IntMor, SUM(V.Cargo) as Cargo,
                       SUM(V.Poliza) as Poliza, SUM(V.Principal) as Principal,
                       SUM(V.IntCor + V.IntMor + V.Cargo + V.Poliza + V.Principal) as Total,
                       case when Reg.PROCESO = 'J' then Cat.CtaCIntC else Cat.CTANINTC end as CtaIntC,
                       case when Reg.PROCESO = 'J' then Cat.CtaCIntM else Cat.CTANINTM end as CtaIntM,
                       case when Reg.PROCESO = 'J' then Cat.CtaCAmort else Cat.CTANAMORT end as CtaAmortiza
                  from vCRDsReportesMov V
                 inner join SIF_OFICINAS Ofi on ISNULL(V.cod_Oficina_R, @oficinaTitular) = Ofi.COD_OFICINA
                 inner join REG_CREDITOS Reg on V.id_solicitud = Reg.ID_SOLICITUD
                 inner join CATALOGO Cat on Reg.CODIGO = Cat.CODIGO
                 where V.Tcon = @tipo and V.Ncon = @numero and Reg.Id_Solicitud <> @operacion
                 group by V.id_solicitud, V.codigo, V.cedula, V.COD_CONCEPTO, Ofi.COD_UNIDAD, Ofi.COD_CENTRO_COSTO,
                          Reg.PROCESO, Cat.CtaCIntC, Cat.CTANINTC, Cat.CtaCIntM, Cat.CTANINTM, Cat.CtaCAmort, Cat.CTANAMORT;",
                new
                {
                    oficinaTitular = ctx.OficinaTitular,
                    tipo = ctx.TipoDocumento,
                    numero = ctx.NumeroDocumento,
                    operacion = ctx.Request.operacion
                }, commandTimeout: 0).ToList();

            if (filas.Count == 0)
                return;

            var cuentaCargos = new MCobroDb(_config).fxCBRParametro(ctx.CodEmpresa, ParametroCuentaCargos);

            foreach (var fila in filas)
            {
                RegistrarAsientosFiador(cn, ctx, fila, cuentaCargos);
            }
        }

        private static void RegistrarAsientosFiador(SqlConnection cn, ReversionContexto ctx, AsientoReversionRow fila, string cuentaCargos)
        {
            var operacion = ctx.Request.operacion.ToString(CultureInfo.InvariantCulture);
            var fiador = fila.Id_Solicitud.ToString(CultureInfo.InvariantCulture);
            var centro = fila.Cod_Centro_Costo ?? string.Empty;

            // Débito: operación del deudor
            if (fila.Total > 0m)
                EjecutarAsiento(cn, ctx, new AsientoArgs(fila.Total, "D", ctx.Cuentas.Cod_Unidad, "", ctx.Cuentas.CtaAmortiza, operacion, ctx.Cabecera.Codigo));

            // Créditos: operación del fiador
            if (fila.Principal > 0m)
                EjecutarAsiento(cn, ctx, new AsientoArgs(fila.Principal, "C", fila.Cod_Unidad, "", fila.CtaAmortiza, fiador, fila.Codigo));

            if (fila.Cargo > 0m)
                RegistrarAsientosCargos(cn, ctx, fila, cuentaCargos);

            if (fila.Poliza > 0m)
                RegistrarAsientoPoliza(cn, ctx, fila);

            if (fila.IntCor > 0m)
                EjecutarAsiento(cn, ctx, new AsientoArgs(fila.IntCor, "C", fila.Cod_Unidad, centro, fila.CtaIntC, fiador, fila.Codigo));

            if (fila.IntMor > 0m)
                EjecutarAsiento(cn, ctx, new AsientoArgs(fila.IntMor, "C", fila.Cod_Unidad, centro, fila.CtaIntM, fiador, fila.Codigo));
        }

        private static void RegistrarAsientosCargos(SqlConnection cn, ReversionContexto ctx, AsientoReversionRow fila, string cuentaCargos)
        {
            var fiador = fila.Id_Solicitud.ToString(CultureInfo.InvariantCulture);

            if (ctx.SysPlanPagos != 1)
            {
                EjecutarAsiento(cn, ctx, new AsientoArgs(fila.Cargo, "C", fila.Cod_Unidad, fila.Cod_Centro_Costo ?? "", cuentaCargos, fiador, fila.Codigo));
                return;
            }

            // Con plan de pagos se detallan los cargos de la afectación del documento (como el VB6, sin filtrar por fiador)
            var cargos = cn.Query<AfectacionCargoRow>(
                "exec spCrdDocumentoAfectacionCargos @tipo, @numero",
                new { tipo = ctx.TipoDocumento, numero = ctx.NumeroDocumento }, commandTimeout: 0);

            foreach (var cargo in cargos)
            {
                EjecutarAsiento(cn, ctx, new AsientoArgs(cargo.Mov_Monto, "C", cargo.Cod_Unidad, cargo.Cod_Centro_Costo, cargo.Cod_Cuenta,
                    cargo.Id_Solicitud.ToString(CultureInfo.InvariantCulture), cargo.Codigo));
            }
        }

        private static void RegistrarAsientoPoliza(SqlConnection cn, ReversionContexto ctx, AsientoReversionRow fila)
        {
            var cuentaPoliza = (cn.QueryFirstOrDefault<string>(
                "select dbo.fxCrdOperacionCtaContaPolizas(@operacion) as Cuenta;",
                new { operacion = fila.Id_Solicitud }, commandTimeout: 0) ?? string.Empty).Trim();

            // Paridad VB6: la transacción del asiento de póliza es la cuenta contable de la póliza.
            EjecutarAsiento(cn, ctx, new AsientoArgs(fila.Poliza, "C", fila.Cod_Unidad, fila.Cod_Centro_Costo ?? "", cuentaPoliza,
                fila.Id_Solicitud.ToString(CultureInfo.InvariantCulture), fila.Codigo)
            { Transaccion = cuentaPoliza });
        }

        private static void EjecutarAsiento(SqlConnection cn, ReversionContexto ctx, AsientoArgs a)
        {
            cn.Execute(
                "exec spSIFDocsAsiento @Tipo, @Transaccion, @Monto, @Movimiento, @Divisa, 1, @Contabilidad, @Unidad, @CentroCosto, @Cuenta, @Referencia1, @Referencia2, ''",
                new
                {
                    Tipo = ctx.TipoDocumento,
                    Transaccion = a.Transaccion ?? ctx.NumeroDocumento,
                    a.Monto,
                    a.Movimiento,
                    Divisa = ctx.Cabecera.Divisa,
                    Contabilidad = ctx.Contabilidad,
                    Unidad = a.Unidad,
                    CentroCosto = a.CentroCosto,
                    a.Cuenta,
                    Referencia1 = a.Referencia1,
                    Referencia2 = a.Referencia2
                }, commandTimeout: 0);
        }

        private void FinalizarReversion(SqlConnection cn, ReversionContexto ctx)
        {
            // Las operaciones trasladadas que ya no tienen saldo vuelven a proceso normal
            cn.Execute(@"
                update reg_creditos set PROCESO = 'N'
                 where proceso = 'T' and estado = 'A'
                   and id_solicitud in (select referencia from reg_creditos
                                         where referencia = @operacion
                                         group by referencia having sum(saldo) <= 0);",
                new { operacion = ctx.Request.operacion }, commandTimeout: 0);

            new MSecurityMainDb(_config).Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = ctx.CodEmpresa,
                Usuario = ctx.Usuario,
                DetalleMovimiento = $"Traspaso de Deudas de la Operación: {ctx.Request.operacion}",
                Movimiento = "Reversa - WEB",
                Modulo = ModuloCobro
            });

            // Registro en historial y expediente (tipo '05' = reversión de traspaso)
            cn.Execute(
                "exec spCBRRegTransac @Tipo, @Cedula, @Operacion, @Notas, @Saldo, 0, 0, 0, 0, 0, @TipoDoc, @Documento, @Usuario",
                new
                {
                    Tipo = "05",
                    Cedula = ctx.Cabecera.Cedula,
                    Operacion = ctx.Request.operacion,
                    Notas = ctx.Observacion,
                    Saldo = ctx.Request.nuevomonto,
                    TipoDoc = ctx.TipoDocumento,
                    Documento = ctx.NumeroDocumento,
                    Usuario = ctx.Usuario
                }, commandTimeout: 0);
        }

        #endregion

        #region Reversión - tipos de apoyo

        private sealed class ReversionContexto
        {
            public int CodEmpresa { get; set; }
            public COTrasladoDeudaRevertirRequestDto Request { get; set; } = null!;
            public CabeceraReversionRow Cabecera { get; set; } = null!;
            public CuentasOperacionRow Cuentas { get; set; } = new();
            public List<TrasladadaRow> Fiadores { get; set; } = new();
            public string Observacion { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
            public decimal FechaProceso { get; set; }
            public int Contabilidad { get; set; }
            public int SysPlanPagos { get; set; }
            public int SysDocVersion { get; set; }
            public string OficinaTitular { get; set; } = string.Empty;
            public string TipoDocumento { get; set; } = string.Empty;
            public string NumeroDocumento { get; set; } = string.Empty;
        }

        private sealed record AsientoArgs(
            decimal Monto, string Movimiento, string Unidad, string CentroCosto,
            string Cuenta, string Referencia1, string Referencia2)
        {
            public string? Transaccion { get; init; }
        }

        private sealed class CabeceraReversionRow
        {
            public string Cedula { get; set; } = string.Empty;
            public string Codigo { get; set; } = string.Empty;
            public decimal Saldo { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Divisa { get; set; } = "COL";
        }

        private sealed class CuentasOperacionRow
        {
            public string Cod_Unidad { get; set; } = string.Empty;
            public string CtaAmortiza { get; set; } = string.Empty;
        }

        private sealed class TrasladadaRow
        {
            public int Id_Solicitud { get; set; }
            public string? Codigo { get; set; }
            public string? Cedula { get; set; }
            public decimal Monto { get; set; }
            public decimal Saldo { get; set; }
            public decimal Cuota { get; set; }
            public decimal Tasa { get; set; }
            public int Plazo { get; set; }
            public decimal C_Intereses { get; set; }
            public decimal C_Cargos { get; set; }
            public decimal C_Iva { get; set; }
            public decimal C_Polizas { get; set; }
            public string? Nombre { get; set; }
        }

        private sealed class EstadoOperacionRow
        {
            public int Id { get; set; }
            public string Estado { get; set; } = string.Empty;
        }

        private sealed class CancelacionInfoRow
        {
            public decimal IntCor { get; set; }
            public decimal IntMor { get; set; }
            public decimal Cargos { get; set; }
            public decimal Poliza { get; set; }
            public decimal Principal { get; set; }
        }

        private sealed class MorosidadSumaRow
        {
            public decimal IntC { get; set; }
            public decimal IntM { get; set; }
            public decimal Amortiza { get; set; }
            public decimal Cargo { get; set; }
        }

        private sealed class AsientoReversionRow
        {
            public int Id_Solicitud { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string Cod_Unidad { get; set; } = string.Empty;
            public string? Cod_Centro_Costo { get; set; }
            public decimal IntCor { get; set; }
            public decimal IntMor { get; set; }
            public decimal Cargo { get; set; }
            public decimal Poliza { get; set; }
            public decimal Principal { get; set; }
            public decimal Total { get; set; }
            public string CtaIntC { get; set; } = string.Empty;
            public string CtaIntM { get; set; } = string.Empty;
            public string CtaAmortiza { get; set; } = string.Empty;
        }

        private sealed class AfectacionCargoRow
        {
            public int Id_Solicitud { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public decimal Mov_Monto { get; set; }
            public string Cod_Unidad { get; set; } = string.Empty;
            public string Cod_Centro_Costo { get; set; } = string.Empty;
            public string Cod_Cuenta { get; set; } = string.Empty;
        }

        #endregion
    }
}
