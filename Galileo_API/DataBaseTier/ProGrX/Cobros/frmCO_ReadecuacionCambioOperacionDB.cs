using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOReadecuacionCambioOperacionDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _security_MainDB;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MSeguimientoDB _mSeguimientoDB;
        private const string IDENTIFICACION_CONGELADA = "Esta Persona se encuentra CONGELADA, verifique...";
        private const string OPERACION_INVALIDA = "No se encontró el número de operación [Activa]";
        private const string NOTA_INVALIDA = "La nota para realizar la transacción no es válida...";
        private const int MODULO = 4;
        private const string CARGOS = "Cargos";
        private const string FORMATO_FECHA = "yyyy/MM/dd";
        private const string IntMor = "IntMor";
        private const string IntCor = "IntCor";
        private const string TIPO = "@tipo";
        private const string TIPO_DOC = "@TipoDoc";
        private const string NUM_DOC = "@NumDoc";
        public FrmCOReadecuacionCambioOperacionDB(IConfiguration config)
            : this(
                new PortalDB(config),
                new MSecurityMainDb(config),
                new MProGrxMain(config),
                new MSeguimientoDB(config))
        {
        }

        public FrmCOReadecuacionCambioOperacionDB(
            PortalDB portalDB,
            MSecurityMainDb securityMainDb,
            MProGrxMain mProGrxMain,
            MSeguimientoDB mSeguimientoDB)
        {
            _portalDB = portalDB;
            _security_MainDB = securityMainDb;
            _mProGrxMain = mProGrxMain;
            _mSeguimientoDB = mSeguimientoDB;
        }
        /// <summary>
        /// Obtiene la informacion por codigo de tramite de una operacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idTramite"></param>
        /// <returns></returns>
        public ErrorDto<CoReadecuacionCambioOperacionObtenerResponse> CO_ReadecuacionCambioOperacion_Obtener(int CodEmpresa, int idTramite)
        {
            if (idTramite <= 0)
                return DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionObtenerResponse>(OPERACION_INVALIDA, -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                const string sql = @"
                SELECT
                    R.*,
                    S.nombre,
                    C.descripcion,
                    ISNULL(V.intC,0) AS IntMorCor,
                    ISNULL(V.intM,0) AS IntMorMor,
                    ISNULL(V.cargos,0) AS Cargos,
                    ISNULL(R.liqTasa,0) AS LiqTasaX,
                    dbo.fxCRDCalculoIntCorte(R.id_solicitud, dbo.MyGetdate()) AS InteresTotal,
                    CAST(0 AS decimal(18,2)) AS Poliza,
                    O.descripcion AS OficinaDesc,
                    R.cod_oficina_r AS Oficina,
                    R.cod_grupo,
                    Pre.descripcion AS RecursoDesc,
                    dbo.MyGetdate() AS FechaServer
                FROM reg_creditos R
                INNER JOIN Socios S ON R.cedula = S.cedula
                INNER JOIN catalogo C ON R.codigo = C.codigo
                LEFT JOIN sif_oficinas O ON R.cod_oficina_R = O.cod_Oficina
                LEFT JOIN Vista_Morosidad V ON R.id_solicitud = V.id_solicitud
                LEFT JOIN CATALOGO_GRUPOS Pre ON R.cod_grupo = Pre.cod_grupo
                WHERE R.id_solicitud = @Id AND R.estado = 'A';";

                var row = conn.QueryFirstOrDefault(sql, new { Id = idTramite });
                if (row == null)
                    return DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionObtenerResponse>(OPERACION_INVALIDA, -2);

                var d = (IDictionary<string, object?>)row;

                var interesTotal = ToDec(V(d, "InteresTotal"));
                var intMorCor = ToDec(V(d, "IntMorCor"));
                var intMorMor = ToDec(V(d, "IntMorMor"));
                var cargos = ToDec(V(d, CARGOS));
                var poliza = ToDec(V(d, "Poliza"));
                var saldo = ToDec(V(d, "saldo"));
                var montoApr = ToDec(V(d, "montoapr"));

                var intCorVenc = interesTotal - (intMorCor + intMorMor);
                if (intCorVenc < 0) intCorVenc = 0;

                var totalDeuda = saldo + interesTotal + cargos + poliza;

                var plazo = ToInt(V(d, "plazo"));
                var tasa = ToDec(V(d, "interesv"));

                decimal? tbpAdd = TryParseNullableDecimal(V(d, "TBP_PuntosAdd"));
                var liqTasaX = ToInt(V(d, "LiqTasaX"));

                var tasaLabel = "Tasa %";
                if (tbpAdd.HasValue)
                    tasaLabel = $"Tasa (TBP + {tbpAdd.Value})";
                if (liqTasaX == 1)
                    tasaLabel = tasaLabel + " + PtsLiq";

                var init = _mProGrxMain.sbSifParametrosInicializa(CodEmpresa, string.Empty);

                var sysPlanPagos = (init.Code == 0 && init.Result != null)
                    ? init.Result.SysPlanPagos
                    : GetSysPlanPagos(conn, tx: null, CodEmpresa);

                if (sysPlanPagos == 1)
                {
                    var fechaServer = ToDateTime(V(d, "FechaServer"));
                    var fechaStr = fechaServer.ToString(FORMATO_FECHA, CultureInfo.InvariantCulture);

                    const string sp = "exec spCrdPlanPagosInfoCancelacion @Id, @Fecha";
                    var r2 = conn.QueryFirstOrDefault(sp, new { Id = idTramite, Fecha = fechaStr });
                    if (r2 != null)
                    {
                        var d2 = (IDictionary<string, object?>)r2;

                        intCorVenc = 0m;
                        intMorMor = ToDec(V(d2, IntMor));
                        intMorCor = ToDec(V(d2, IntCor));
                        cargos = ToDec(V(d2, CARGOS));
                        poliza = ToDec(V(d2, "Poliza"));

                        var principal = ToDec(V(d2, "Principal"));
                        totalDeuda = principal + intMorCor + intMorMor + cargos + poliza;
                    }
                }

                var noMonto = totalDeuda;
                var noPlazo = plazo;
                var noTasa = tasa;
                var noCuota = MCobroDb.fxCalcula_Cuota(noMonto, noPlazo, noTasa, "M");

                var fechaServerFinal = ToDateTime(V(d, "FechaServer")).ToString(FORMATO_FECHA, CultureInfo.InvariantCulture);

                var dto = new CoReadecuacionCambioOperacionConsultaDto
                {
                    cedula = S(V(d, "cedula")),
                    codigo = S(V(d, "codigo")),
                    descripcion = S(V(d, "descripcion")),
                    nombre = S(V(d, "nombre")),

                    monto = montoApr,
                    int_cor_atrasado = intMorCor,
                    int_cor_venc = intCorVenc,
                    int_moratorio = intMorMor,
                    saldo = saldo,
                    cargos = cargos,
                    polizas = poliza,
                    interes_total = interesTotal,
                    total_deuda = totalDeuda,

                    plazo = plazo,
                    tasa = tasa,
                    tasa_label = tasaLabel,

                    liq_tasa_x = liqTasaX,
                    tbp_puntos_add = tbpAdd,

                    oficina_desc = S(V(d, "OficinaDesc")),
                    oficina = S(V(d, "Oficina")),
                    recurso_desc = S(V(d, "RecursoDesc")),

                    no_monto = noMonto,
                    no_plazo = noPlazo,
                    no_tasa = noTasa,
                    no_cuota = noCuota,

                    fecha_server = fechaServerFinal,
                };

                return DbHelper.CreateOkResponse(new CoReadecuacionCambioOperacionObtenerResponse
                {
                    id_tramite = idTramite,
                    datos = dto
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionObtenerResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Aplica la readecuacion de deuda.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<CoReadecuacionCambioOperacionAplicarResponse> CO_ReadecuacionCambioOperacion_Aplicar(int CodEmpresa,CoReadecuacionCambioOperacionAplicarRequest req)
        {
            var valid = ValidarAplicar(req);
            if (valid.Error != null)
                return valid.Error;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                var op = ObtenerOperacionActiva(conn, req.id_tramite ?? 0);
                if (op.Error != null || op.Dop == null)
                    return op.Error ?? DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>(OPERACION_INVALIDA, -2);

                if (PersonaCongelada(conn, tx: null, op.Cedula, "per_readecuaciones"))
                    return DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>(IDENTIFICACION_CONGELADA, -2);

                var init = _mProGrxMain.sbSifParametrosInicializa(CodEmpresa, valid.Usuario);
                if (init.Code != 0 || init.Result == null)
                    return DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>(
                        "No se pudo inicializar parámetros del sistema.", -2);

                var g = init.Result;

                var vTipoDoc = "REA";
                var vConcepto = "CBR001";
                var vFecha = MyGetDate(conn);

                using var tx = conn.BeginTransaction();

                var doc = ResolverDocumento(conn, tx, g.SysDocVersion, vTipoDoc, valid.Usuario);
                if (doc.Error != null)
                    return doc.Error;

                var ctx = new AplicarCtx(
                    CodEmpresa: CodEmpresa,
                    Req: req,
                    Valid: valid,
                    Op: op,
                    Globales: g,
                    FechaServidor: vFecha,
                    TipoDoc: vTipoDoc,
                    Concepto: vConcepto,
                    Doc: doc
                );

                EjecutarCancelacion(conn, tx, ctx);

                var vDiaPago = CalcularDiaPago(req.chk_dia_pago, op.DiaPagoOld, vFecha, op.IndDeduc);

                var primerDeduccion = _mSeguimientoDB.fxPrimerDeduccion(
                    CodEmpresa,
                    op.CodigoLinea,
                    0,
                    0,
                    vDiaPago
                );

                var nuevaOperacion = InsertarNuevaOperacion(conn, tx, ctx, Convert.ToInt32(primerDeduccion), vDiaPago);

                CopiarFiadores(conn, tx, nuevaOperacion, req.id_tramite ?? 0);

                if (g.SysPlanPagos == 1)
                {
                    conn.Execute("exec spCrdPlanPagos @Id;", new { Id = nuevaOperacion }, tx);
                    conn.Execute("exec spCrdPlanPagosActivaCuota @Id;", new { Id = nuevaOperacion }, tx);
                }

                var pantalla = CalcularPantallaParaRegTransac(
                    conn,
                    tx,
                    g.SysPlanPagos,
                    req.id_tramite ?? 0,
                    vFecha,
                    op.Dop
                );

                var docCtx = new DocumentoContext(
                    codEmpresa: CodEmpresa,
                    sysDocVersion: g.SysDocVersion,
                    sysPlanPagos: g.SysPlanPagos,

                    operacionId: req.id_tramite ?? 0,
                    cedula: op.Cedula,
                    nombre: op.Nombre,
                    descripcion: op.Descripcion,
                    codigoLinea: op.CodigoLinea,

                    usuario: valid.Usuario,
                    notas: valid.Notas,
                    fechaServidor: vFecha,

                    tipoDoc: vTipoDoc,
                    numDoc: doc.NumDocStr,
                    concepto: vConcepto,
                    tipoMov: doc.TipoMov,

                    gEnlace: g.GEnlace,
                    docDeposito: (req.gstr_mascara ?? string.Empty).Trim(),

                    saldoPantalla: pantalla.Saldo,
                    intCorAtrasado: pantalla.IntCorAtrasado,
                    intCorVenc: pantalla.IntCorVenc,
                    intMoratorio: pantalla.IntMoratorio,
                    cargos: pantalla.Cargos,
                    polizas: pantalla.Polizas
                );

                SbDocumento(conn, tx, op.Dop, docCtx);

                tx.Commit();

                _security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = valid.Usuario,
                    Modulo = MODULO,
                    Movimiento = "Modifica - WEB",
                    DetalleMovimiento = $"Readecuacion de Operacion de {req.id_tramite} A {nuevaOperacion}"
                });

                var msg =
                    $"- La operación No. {req.id_tramite} fue cancelada y se registró nueva operación No. {nuevaOperacion}\n\n - Readecuación No.{doc.NumDocStr}";

                return DbHelper.CreateOkResponse(new CoReadecuacionCambioOperacionAplicarResponse
                {
                    operacion_original = req.id_tramite ?? 0,
                    operacion_nueva = nuevaOperacion,
                    tipo_documento = vTipoDoc,
                    num_documento = doc.NumDocStr,
                    mensaje = msg
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene el id de la nueva operacion para el reporte.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<CoReadecuacionReporteOperacionNuevaDto> CO_Readecuacion_ReporteOperacionNueva_Obtener(int CodEmpresa,CoReadecuacionReporteOperacionNuevaRequest req)
        {
            if (req == null || req.id_solicitud <= 0)
                return DbHelper.CreateErrorResponse<CoReadecuacionReporteOperacionNuevaDto>("Solicitud inválida.", -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sqlRef = @"
                    SELECT TOP 1
                        ISNULL(id_solicitud, 0)
                    FROM reg_creditos
                    WHERE referencia = @idOriginal
                      AND estadosol = 'F'
                    ORDER BY id_solicitud DESC;";

                var opRef = conn.ExecuteScalar<long>(sqlRef, new { idOriginal = req.id_solicitud });
                if (opRef > 0)
                {
                    return DbHelper.CreateOkResponse(new CoReadecuacionReporteOperacionNuevaDto
                    {
                        operacion_nueva = opRef
                    });
                }

                const string sqlBase = @"
                    SELECT TOP 1
                        RTRIM(cedula) AS cedula,
                        RTRIM(codigo) AS codigo
                    FROM reg_creditos
                    WHERE id_solicitud = @id;";

                var baseData = conn.QueryFirstOrDefault(sqlBase, new { id = req.id_solicitud });
                if (baseData == null)
                    return DbHelper.CreateOkResponse(new CoReadecuacionReporteOperacionNuevaDto { operacion_nueva = 0 });

                const string sqlVop = @"
                    SELECT ISNULL(MIN(id_solicitud), 0)
                    FROM reg_creditos
                    WHERE cedula = @cedula
                      AND codigo = @codigo
                      AND estadosol = 'F'
                      AND id_solicitud > @idOriginal;";

                var vop = conn.ExecuteScalar<long>(sqlVop, new
                {
                    cedula = (string)baseData.cedula,
                    codigo = (string)baseData.codigo,
                    idOriginal = req.id_solicitud
                });

                return DbHelper.CreateOkResponse(new CoReadecuacionReporteOperacionNuevaDto
                {
                    operacion_nueva = vop
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoReadecuacionReporteOperacionNuevaDto>(ex.Message);
            }
        }
        private sealed record PantallaValores(
            decimal Saldo,
            decimal IntCorAtrasado,
            decimal IntCorVenc,
            decimal IntMoratorio,
            decimal Cargos,
            decimal Polizas
        );
        private sealed record DocumentoContext(
            int codEmpresa,
            int sysDocVersion,
            int sysPlanPagos,

            int operacionId,
            string cedula,
            string nombre,
            string descripcion,
            string codigoLinea,

            string usuario,
            string notas,
            DateTime fechaServidor,

            string tipoDoc,
            string numDoc,
            string concepto,
            string tipoMov,

            int gEnlace,
            string docDeposito,

            decimal saldoPantalla,
            decimal intCorAtrasado,
            decimal intCorVenc,
            decimal intMoratorio,
            decimal cargos,
            decimal polizas
        );
        private static PantallaValores CalcularPantallaParaRegTransac( SqlConnection conn,SqlTransaction tx,int sysPlanPagos,int idTramite,DateTime fechaServidor,IDictionary<string, object?> dop)
        {
            var saldo = ToDec(V(dop, "saldo"));

            decimal intCorAtrasado = 0m;
            decimal intCorVenc = 0m;
            decimal intMor = 0m;
            decimal cargos = 0m;
            decimal polizas = 0m;

            if (sysPlanPagos == 1)
            {
                var fechaStr = fechaServidor.ToString(FORMATO_FECHA, CultureInfo.InvariantCulture);
                const string sp = "exec spCrdPlanPagosInfoCancelacion @Id, @Fecha";

                var r2 = conn.QueryFirstOrDefault(sp, new { Id = idTramite, Fecha = fechaStr }, tx);
                if (r2 != null)
                {
                    var d2 = (IDictionary<string, object?>)r2;
                    intCorVenc = 0m;
                    intMor = ToDec(V(d2, IntMor));
                    intCorAtrasado = ToDec(V(d2, IntCor));
                    cargos = ToDec(V(d2, CARGOS));
                    polizas = ToDec(V(d2, "Poliza"));
                }

                return new PantallaValores(
                    Saldo: saldo,
                    IntCorAtrasado: intCorAtrasado,
                    IntCorVenc: intCorVenc,
                    IntMoratorio: intMor,
                    Cargos: cargos,
                    Polizas: polizas
                );
            }
            const string sql = @"
                SELECT
                    ISNULL(V.intC,0) AS IntMorCor,
                    ISNULL(V.intM,0) AS IntMorMor,
                    ISNULL(V.cargos,0) AS Cargos,
                    dbo.fxCRDCalculoIntCorte(R.id_solicitud, dbo.MyGetdate()) AS InteresTotal
                FROM reg_creditos R
                LEFT JOIN Vista_Morosidad V ON R.id_solicitud = V.id_solicitud
                WHERE R.id_solicitud = @Id;";

            var r = conn.QueryFirstOrDefault(sql, new { Id = idTramite }, tx);
            if (r != null)
            {
                var d = (IDictionary<string, object?>)r;
                var interesTotal = ToDec(V(d, "InteresTotal"));
                var intMorCor = ToDec(V(d, "IntMorCor"));
                var intMorMor = ToDec(V(d, "IntMorMor"));

                cargos = ToDec(V(d, CARGOS));
                polizas = 0m;
                var tmp = interesTotal - (intMorCor + intMorMor);
                intCorVenc = tmp < 0 ? 0m : tmp;
                intCorAtrasado = intMorCor;
                intMor = intMorMor;
            }

            return new PantallaValores(
                Saldo: saldo,
                IntCorAtrasado: intCorAtrasado,
                IntCorVenc: intCorVenc,
                IntMoratorio: intMor,
                Cargos: cargos,
                Polizas: polizas
            );
        }
        private sealed record MovMontos(
    decimal Cargo,
    decimal IntC,
    decimal IntM,
    decimal Amortiza,
    decimal Poliza
)
        {
            public decimal MontoTotal => Cargo + IntC + IntM + Amortiza + Poliza;
        }
        private sealed record CtasOperacion(
            string CodDivisa,
            string CodUnidad,
            string CodCentroCosto,
            string CtaAmortiza,
            string CtaIntC,
            string CtaIntM,
            string CtaCargos
        );
        private sealed record LineasTransaccion(
            string Linea1,
            string Linea2,
            string Linea3,
            string Linea4,
            string Linea5,
            string Linea6,
            string Linea7,
            string Linea8,
            string Linea9,
            string Linea10
        );
        private static void SbDocumento(SqlConnection conn,SqlTransaction tx,IDictionary<string, object?> dop,DocumentoContext ctx)
        {
            var mov = ObtenerMontosDocumento(conn, tx, ctx);

            if (!ObjectExists(conn, tx, "dbo.spCrdOperacionCtas", "P"))
                return;

            var ctas = ObtenerCuentasOperacion(conn, tx, ctx);
            if (ctas == null)
                return;
            var lineas = ConstruirLineas(ctx, mov);
            if (ctx.sysDocVersion != 2)
            {
                InsertarAsientosTmpSiAplica(conn, tx, ctx, ctas, mov);
                return;
            }
            if (!ObjectExists(conn, tx, "dbo.SIF_TRANSACCIONES", "U"))
                return;

            InsertarSifTransacciones(conn, tx, dop, ctx, lineas, mov.MontoTotal);

            if (!ObjectExists(conn, tx, "dbo.spSIFDocsAsiento", "P"))
                return;

            const string spAs =
                "exec spSIFDocsAsiento @TipoDocumento,@NumDocumento,@Monto,@DebeHaber,@CodDivisa,@Factor,@Enlace,@CodUnidad,@CodCentroCosto,@CodCuenta,@IdSolicitud,@Codigo,@Deposito;";

            if (!ValidarFirmaSpSifDocsAsiento(conn, tx))
                return;

            EjecutarAsientosBase(conn, tx, ctx, ctas, mov, spAs);
            EjecutarAsientosCargosPlanPagosSiAplica(conn, tx, ctx, ctas, spAs);
            EjecutarAsientoPolizaSiAplica(conn, tx, ctx, ctas, mov, spAs);

            SbCBRRegTransacSiExiste(conn, tx, ctx);
        }
        private static MovMontos ObtenerMontosDocumento(SqlConnection conn, SqlTransaction tx, DocumentoContext ctx)
        {
            decimal curCargo = 0m;
            decimal curIntC = 0m;
            decimal curIntM = 0m;
            decimal curAmortiza = 0m;
            decimal curPoliza = 0m;

            if (ctx.sysPlanPagos == 1)
            {
                if (!ObjectExists(conn, tx, "dbo.spCrdOperacionCtas", "P"))
                {
                    var ok = ProcedureHasParams(conn, tx, "dbo.spCrdDocumentoAfectacion", TIPO_DOC, NUM_DOC, TIPO);
                    if (ok)
                    {
                        var r = conn.QueryFirstOrDefault(
                            "exec spCrdDocumentoAfectacion @TipoDoc,@NumDoc,@Tipo",
                            new { TipoDoc = ctx.tipoDoc, NumDoc = ctx.numDoc, Tipo = "R" },
                            tx);

                        if (r != null)
                        {
                            var d = (IDictionary<string, object?>)r;
                            curCargo = ToDec(V(d, CARGOS));
                            curIntC = ToDec(V(d, IntCor));
                            curIntM = ToDec(V(d, IntMor));
                            curAmortiza = ToDec(V(d, "Principal"));
                            curPoliza = ToDec(V(d, "Polizas"));
                        }
                    }
                }

                return new MovMontos(curCargo, curIntC, curIntM, curAmortiza, curPoliza);
            }
            if (!ObjectExists(conn, tx, "dbo.spCrdOperacionCtas", "V"))
            {
                const string sql = @"
            SELECT
                ISNULL(SUM(intCor),0) AS IntCor,
                ISNULL(SUM(intMor),0) AS IntMor,
                ISNULL(SUM(Cargo),0)  AS Cargos,
                ISNULL(SUM(Poliza),0) AS Polizas,
                ISNULL(SUM(Principal),0) AS Principal
            FROM dbo.vCRDsReportesMov
            WHERE Tcon = @Tcon AND Ncon = @Ncon AND id_solicitud = @Id;";

                var r = conn.QueryFirstOrDefault(sql, new
                {
                    Tcon = ctx.tipoMov,
                    Ncon = ctx.numDoc,
                    Id = ctx.operacionId
                }, tx);

                if (r != null)
                {
                    var d = (IDictionary<string, object?>)r;
                    curCargo = ToDec(V(d, CARGOS));
                    curIntC = ToDec(V(d, IntCor));
                    curIntM = ToDec(V(d, IntMor));
                    curAmortiza = ToDec(V(d, "Principal"));
                    curPoliza = ToDec(V(d, "Polizas"));
                }
            }

            return new MovMontos(curCargo, curIntC, curIntM, curAmortiza, curPoliza);
        }
        private static CtasOperacion? ObtenerCuentasOperacion(SqlConnection conn, SqlTransaction tx, DocumentoContext ctx)
        {
            var cuentas = conn.QueryFirstOrDefault("exec spCrdOperacionCtas @Id", new { Id = ctx.operacionId }, tx);
            if (cuentas == null)
                return null;

            var ctas = (IDictionary<string, object?>)cuentas;

            var codDivisa = S(V(ctas, "cod_divisa"));
            var codUnidad = S(V(ctas, "Cod_Unidad", "cod_unidad"));
            var codCentroCosto = S(V(ctas, "Cod_Centro_Costo", "cod_centro_costo"));
            var ctaAmortiza = S(V(ctas, "ctaamortiza"));
            var ctaIntC = S(V(ctas, "ctaintc"));
            var ctaIntM = S(V(ctas, "ctaintm"));
            var ctaCargos = S(V(ctas, "CtaCargos", "ctacargos"));

            return new CtasOperacion(
                CodDivisa: codDivisa,
                CodUnidad: codUnidad,
                CodCentroCosto: codCentroCosto,
                CtaAmortiza: ctaAmortiza,
                CtaIntC: ctaIntC,
                CtaIntM: ctaIntM,
                CtaCargos: ctaCargos
            );
        }
        private static LineasTransaccion ConstruirLineas(DocumentoContext ctx, MovMontos mov)
        {
            var saldoAnterior = ctx.saldoPantalla;
            var saldoActual = saldoAnterior - mov.Amortiza;

            var l1 = "Saldo Anterior    " + saldoAnterior.ToString("0.00", CultureInfo.InvariantCulture);
            var l2 = "Interes Corriente " + mov.IntC.ToString("0.00", CultureInfo.InvariantCulture);
            var l3 = "Interes Moratorio " + mov.IntM.ToString("0.00", CultureInfo.InvariantCulture);
            var l4 = "Amortización      " + mov.Amortiza.ToString("0.00", CultureInfo.InvariantCulture);
            var l5 = "Saldo Actual      " + saldoActual.ToString("0.00", CultureInfo.InvariantCulture);
            var l6 = "Pólizas           " + mov.Poliza.ToString("0.00", CultureInfo.InvariantCulture);
            var l7 = "Cargos [General]  " + mov.Cargo.ToString("0.00", CultureInfo.InvariantCulture);
            var l8 = "Operacion/Línea   " + "Op.:" + ctx.operacionId.ToString(CultureInfo.InvariantCulture) + " L.:" + ctx.codigoLinea;
            var l9 = "Descripción       " + ctx.descripcion.Trim();
            var l10 = " ";

            return new LineasTransaccion(l1, l2, l3, l4, l5, l6, l7, l8, l9, l10);
        }
        private static void InsertarAsientosTmpSiAplica(SqlConnection conn,SqlTransaction tx,DocumentoContext ctx,CtasOperacion ctas,MovMontos mov)
        {
            if (!ObjectExists(conn, tx, "dbo.asientos_tmp", "U"))
                return;

            var caso = "RA" + ctx.operacionId.ToString(CultureInfo.InvariantCulture);
            var f = ctx.fechaServidor.ToString(FORMATO_FECHA, CultureInfo.InvariantCulture);

            const string insTmp = @"
            INSERT asientos_tmp(TMP_TIPO,TMP_USUARIO,TMP_CASO,TMP_CUENTA,TMP_MONTO,TMP_DEBEHABER,TMP_FECHA,TMP_ESTADO_ASIENTO)
            VALUES('TRA',@Usuario,@Caso,@Cuenta,@Monto,@DH,@Fecha,'P');";

            var curMonto = mov.MontoTotal;

            if (curMonto > 0 && !string.IsNullOrWhiteSpace(ctas.CtaAmortiza))
                conn.Execute(insTmp, new { Usuario = ctx.usuario, Caso = caso, Cuenta = ctas.CtaAmortiza, Monto = curMonto, DH = "D", Fecha = f }, tx);

            if (mov.Amortiza > 0 && !string.IsNullOrWhiteSpace(ctas.CtaAmortiza))
                conn.Execute(insTmp, new { Usuario = ctx.usuario, Caso = caso, Cuenta = ctas.CtaAmortiza, Monto = mov.Amortiza, DH = "H", Fecha = f }, tx);

            if (mov.Cargo > 0 && !string.IsNullOrWhiteSpace(ctas.CtaCargos))
                conn.Execute(insTmp, new { Usuario = ctx.usuario, Caso = caso, Cuenta = ctas.CtaCargos, Monto = mov.Cargo, DH = "H", Fecha = f }, tx);

            if (mov.IntC > 0 && !string.IsNullOrWhiteSpace(ctas.CtaIntC))
                conn.Execute(insTmp, new { Usuario = ctx.usuario, Caso = caso, Cuenta = ctas.CtaIntC, Monto = mov.IntC, DH = "H", Fecha = f }, tx);

            if (mov.IntM > 0 && !string.IsNullOrWhiteSpace(ctas.CtaIntM))
                conn.Execute(insTmp, new { Usuario = ctx.usuario, Caso = caso, Cuenta = ctas.CtaIntM, Monto = mov.IntM, DH = "H", Fecha = f }, tx);
        }
        private static void InsertarSifTransacciones(SqlConnection conn,SqlTransaction tx,IDictionary<string, object?> dop,DocumentoContext ctx,LineasTransaccion lineas,decimal monto)
        {
            var codOficina = S(V(dop, "cod_oficina_r"));

            const string insTr = @"
                INSERT SIF_TRANSACCIONES(
                    COD_TRANSACCION,TIPO_DOCUMENTO,REGISTRO_FECHA,REGISTRO_USUARIO,Cliente_IDENTIFICACION,CLIENTE_NOMBRE,
                    cod_concepto,monto,estado,Referencia_01,Referencia_02,Referencia_03,cod_oficina,
                    linea1,linea2,linea3,linea4,linea5,linea6,linea7,linea8,linea9,linea10,detalle,documento
                )
                VALUES(
                    @NumDoc,@TipoDoc,dbo.MyGetdate(),@Usuario,@Cedula,@Nombre,
                    @Concepto,@Monto,'P',@Ref1,@Ref2,@Ref3,@CodOficina,
                    @L1,@L2,@L3,@L4,@L5,@L6,@L7,@L8,@L9,@L10,@Detalle,@Documento
                );";

            conn.Execute(insTr, new
            {
                NumDoc = ctx.numDoc,
                TipoDoc = ctx.tipoDoc,
                Usuario = ctx.usuario,
                Cedula = ctx.cedula,
                Nombre = ctx.nombre,
                Concepto = ctx.concepto,
                Monto = monto,
                Ref1 = ctx.operacionId.ToString(CultureInfo.InvariantCulture),
                Ref2 = ctx.codigoLinea,
                Ref3 = ctx.docDeposito,
                CodOficina = codOficina,
                L1 = lineas.Linea1,
                L2 = lineas.Linea2,
                L3 = lineas.Linea3,
                L4 = lineas.Linea4,
                L5 = lineas.Linea5,
                L6 = lineas.Linea6,
                L7 = lineas.Linea7,
                L8 = lineas.Linea8,
                L9 = lineas.Linea9,
                L10 = lineas.Linea10,
                Detalle = ctx.notas,
                Documento = ctx.docDeposito
            }, tx);
        }
        private static bool ValidarFirmaSpSifDocsAsiento(SqlConnection conn, SqlTransaction tx)
        {
            return ProcedureHasParams(conn, tx, "dbo.spSIFDocsAsiento",
                "@TipoDocumento", "@NumDocumento", "@Monto", "@DebeHaber", "@CodDivisa",
                "@Factor", "@Enlace", "@CodUnidad", "@CodCentroCosto", "@CodCuenta",
                "@IdSolicitud", "@Codigo", "@Deposito");
        }
        private static void EjecutarAsientosBase(SqlConnection conn,SqlTransaction tx,DocumentoContext ctx,CtasOperacion ctas,MovMontos mov,string spAs)
        {
            var idSol = ctx.operacionId;
            var codigo = ctx.codigoLinea;

            if (mov.MontoTotal > 0 && !string.IsNullOrWhiteSpace(ctas.CtaAmortiza))
                conn.Execute(spAs, new { TipoDocumento = ctx.tipoDoc, NumDocumento = ctx.numDoc, Monto = mov.MontoTotal, DebeHaber = "D", CodDivisa = ctas.CodDivisa, Factor = 1, Enlace = ctx.gEnlace, CodUnidad = ctas.CodUnidad, CodCentroCosto = ctas.CodCentroCosto, CodCuenta = ctas.CtaAmortiza, IdSolicitud = idSol, Codigo = codigo, Deposito = ctx.docDeposito }, tx);

            if (mov.Amortiza > 0 && !string.IsNullOrWhiteSpace(ctas.CtaAmortiza))
                conn.Execute(spAs, new { TipoDocumento = ctx.tipoDoc, NumDocumento = ctx.numDoc, Monto = mov.Amortiza, DebeHaber = "C", CodDivisa = ctas.CodDivisa, Factor = 1, Enlace = ctx.gEnlace, CodUnidad = ctas.CodUnidad, CodCentroCosto = ctas.CodCentroCosto, CodCuenta = ctas.CtaAmortiza, IdSolicitud = idSol, Codigo = codigo, Deposito = ctx.docDeposito }, tx);

            if (mov.IntC > 0 && !string.IsNullOrWhiteSpace(ctas.CtaIntC))
                conn.Execute(spAs, new { TipoDocumento = ctx.tipoDoc, NumDocumento = ctx.numDoc, Monto = mov.IntC, DebeHaber = "C", CodDivisa = ctas.CodDivisa, Factor = 1, Enlace = ctx.gEnlace, CodUnidad = ctas.CodUnidad, CodCentroCosto = ctas.CodCentroCosto, CodCuenta = ctas.CtaIntC, IdSolicitud = idSol, Codigo = codigo, Deposito = ctx.docDeposito }, tx);

            if (mov.IntM > 0 && !string.IsNullOrWhiteSpace(ctas.CtaIntM))
                conn.Execute(spAs, new { TipoDocumento = ctx.tipoDoc, NumDocumento = ctx.numDoc, Monto = mov.IntM, DebeHaber = "C", CodDivisa = ctas.CodDivisa, Factor = 1, Enlace = ctx.gEnlace, CodUnidad = ctas.CodUnidad, CodCentroCosto = ctas.CodCentroCosto, CodCuenta = ctas.CtaIntM, IdSolicitud = idSol, Codigo = codigo, Deposito = ctx.docDeposito }, tx);
            if (mov.Cargo > 0 && ctx.sysPlanPagos == 0 && !string.IsNullOrWhiteSpace(ctas.CtaCargos))
                conn.Execute(spAs, new { TipoDocumento = ctx.tipoDoc, NumDocumento = ctx.numDoc, Monto = mov.Cargo, DebeHaber = "C", CodDivisa = ctas.CodDivisa, Factor = 1, Enlace = ctx.gEnlace, CodUnidad = ctas.CodUnidad, CodCentroCosto = ctas.CodCentroCosto, CodCuenta = ctas.CtaCargos, IdSolicitud = idSol, Codigo = codigo, Deposito = ctx.docDeposito }, tx);
        }
        private static void EjecutarAsientosCargosPlanPagosSiAplica(SqlConnection conn,SqlTransaction tx,DocumentoContext ctx,CtasOperacion ctas,string spAs)
        {
            if (ctx.sysPlanPagos != 1)
                return;

            if (!ObjectExists(conn, tx, "dbo.spCrdDocumentoAfectacionCargos", "P"))
                return;

            var okCargos = ProcedureHasParams(conn, tx, "dbo.spCrdDocumentoAfectacionCargos", TIPO_DOC, NUM_DOC);
            if (!okCargos)
                return;

            var rows = conn.Query(
                "exec spCrdDocumentoAfectacionCargos @TipoDoc,@NumDoc",
                new { TipoDoc = ctx.tipoDoc, NumDoc = ctx.numDoc },
                tx);

            var idSol = ctx.operacionId;
            var codigo = ctx.codigoLinea;

            foreach (var r in rows.Cast<IDictionary<string, object?>>())
            {
                var movMonto = ToDec(V(r, "Mov_Monto", "monto", "Monto"));
                if (movMonto <= 0) continue;

                var codCuenta = S(V(r, "cod_cuenta", "Cod_Cuenta", "Cuenta"));
                if (string.IsNullOrWhiteSpace(codCuenta)) continue;

                var u = S(V(r, "Cod_Unidad", "cod_unidad"));
                var cc = S(V(r, "Cod_Centro_Costo", "cod_centro_costo"));
                var id = ToInt(V(r, "Id_Solicitud", "id_solicitud"));
                var cod = S(V(r, "Codigo", "codigo"));

                conn.Execute(spAs, new
                {
                    TipoDocumento = ctx.tipoDoc,
                    NumDocumento = ctx.numDoc,
                    Monto = movMonto,
                    DebeHaber = "C",
                    CodDivisa = ctas.CodDivisa,
                    Factor = 1,
                    Enlace = ctx.gEnlace,
                    CodUnidad = u,
                    CodCentroCosto = cc,
                    CodCuenta = codCuenta,
                    IdSolicitud = id > 0 ? id : idSol,
                    Codigo = string.IsNullOrWhiteSpace(cod) ? codigo : cod,
                    Deposito = ctx.docDeposito
                }, tx);
            }
        }
        private static void EjecutarAsientoPolizaSiAplica(SqlConnection conn,SqlTransaction tx,DocumentoContext ctx,CtasOperacion ctas,MovMontos mov,string spAs)
        {
            if (mov.Poliza <= 0 || ctx.sysPlanPagos != 1)
                return;

            if (!ObjectExists(conn, tx, "dbo.fxCrdOperacionCtaContaPolizas", "FN"))
                return;

            var cuentaPoliza = conn.QueryFirstOrDefault<string>(
                "select dbo.fxCrdOperacionCtaContaPolizas(@Id) as Cuenta;",
                new { Id = ctx.operacionId },
                tx
            ) ?? string.Empty;

            cuentaPoliza = cuentaPoliza.Trim();
            if (string.IsNullOrWhiteSpace(cuentaPoliza))
                return;

            conn.Execute(spAs, new
            {
                TipoDocumento = ctx.tipoDoc,
                NumDocumento = ctx.numDoc,
                Monto = mov.Poliza,
                DebeHaber = "C",
                CodDivisa = ctas.CodDivisa,
                Factor = 1,
                Enlace = ctx.gEnlace,
                CodUnidad = ctas.CodUnidad,
                CodCentroCosto = ctas.CodCentroCosto,
                CodCuenta = cuentaPoliza,
                IdSolicitud = ctx.operacionId,
                Codigo = ctx.codigoLinea,
                Deposito = ctx.docDeposito
            }, tx);
        }
        private static void SbCBRRegTransacSiExiste(SqlConnection conn, SqlTransaction tx, DocumentoContext ctx)
        {
            if (!ObjectExists(conn, tx, "dbo.spCBRRegTransac", "P"))
                return;

            var hasFull = ProcedureHasParams(conn, tx, "dbo.spCBRRegTransac",
                TIPO, "@Cedula", "@Operacion", "@Notas", "@Saldo", "@IntCor",
                "@IntMor", "@Cargos", "@Polizas", "@SaldoAnt", TIPO_DOC, NUM_DOC);

            if (hasFull)
            {
                const string sp = "exec spCBRRegTransac @Tipo,@Cedula,@Operacion,@Notas,@Saldo,@IntCor,@IntMor,@Cargos,@Polizas,@SaldoAnt,@TipoDoc,@NumDoc;";
                conn.Execute(sp, new
                {
                    Tipo = "03",
                    Cedula = ctx.cedula,
                    Operacion = ctx.operacionId,
                    Notas = ctx.notas,
                    Saldo = ctx.saldoPantalla,
                    IntCor = ctx.intCorAtrasado + ctx.intCorVenc,
                    IntMor = ctx.intMoratorio,
                    Cargos = ctx.cargos,
                    Polizas = ctx.polizas,
                    SaldoAnt = ctx.saldoPantalla,
                    TipoDoc = ctx.tipoDoc,
                    NumDoc = ctx.numDoc
                }, tx);
                return;
            }

            var hasMini = ProcedureHasParams(conn, tx, "dbo.spCBRRegTransac",
                TIPO, "@Cedula", "@Operacion", "@Notas", "@Usuario", TIPO_DOC, NUM_DOC);

            if (hasMini)
            {
                const string sp = "exec spCBRRegTransac @Tipo,@Cedula,@Operacion,@Notas,@Usuario,@TipoDoc,@NumDoc;";
                conn.Execute(sp, new
                {
                    Tipo = "03",
                    Cedula = ctx.cedula,
                    Operacion = ctx.operacionId,
                    Notas = ctx.notas,
                    Usuario = ctx.usuario,
                    TipoDoc = ctx.tipoDoc,
                    NumDoc = ctx.numDoc
                }, tx);
            }
        }
        private static string S(object? v) => (Convert.ToString(v, CultureInfo.InvariantCulture) ?? string.Empty).Trim();
        private static object? V(IDictionary<string, object?> d, params string[] keys)
        {
            return keys
                .Select(k => d
                    .Where(x => string.Equals(x.Key, k, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Value)
                    .FirstOrDefault())
                .FirstOrDefault(value => value != null);
        }
        private static int ToInt(object? v)
        {
            if (v == null) return 0;
            if (int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var n)) return n;
            if (decimal.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return (int)d;
            return 0;
        }
        private static decimal ToDec(object? v)
        {
            if (v == null) return 0m;
            if (decimal.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var n)) return n;
            return 0m;
        }
        private static DateTime ToDateTime(object? v)
        {
            if (v is DateTime dt) return dt;

            var s = Convert.ToString(v, CultureInfo.InvariantCulture) ?? string.Empty;
            var formats = new[] { "yyyy/MM/dd HH:mm:ss", FORMATO_FECHA, "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy" };

            if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var r))
                return r;

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out r))
                return r;

            return DateTime.Now;
        }
        private static decimal? TryParseNullableDecimal(object? v)
        {
            if (v == null) return null;
            if (decimal.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var n))
                return n;
            return null;
        }
        private static DateTime MyGetDate(SqlConnection conn)
            => conn.QueryFirstOrDefault<DateTime>("SELECT dbo.MyGetdate()");
        private static bool CuotaCoincide(decimal cuotaBe, decimal cuotaFe)
        {
            var diff = Math.Abs(cuotaBe - cuotaFe);
            return diff <= 0.50m;
        }
        private static bool ObjectExists(SqlConnection conn, SqlTransaction? tx, string objectName, string objectType)
        {
            const string sql = "SELECT CASE WHEN OBJECT_ID(@obj, @type) IS NOT NULL THEN 1 ELSE 0 END;";
            return conn.QueryFirst<int>(sql, new { obj = objectName, type = objectType }, transaction: tx) == 1;
        }
        private static bool ColumnExists(SqlConnection conn, SqlTransaction? tx, string tableName, string columnName)
        {
            const string sql = @"
            SELECT CASE WHEN EXISTS(
                SELECT 1
                FROM sys.columns c
                INNER JOIN sys.objects o ON c.object_id = o.object_id
                WHERE o.object_id = OBJECT_ID(@tbl, 'U') AND c.name = @col
            ) THEN 1 ELSE 0 END;";

            return conn.QueryFirst<int>(sql, new { tbl = tableName, col = columnName }, transaction: tx) == 1;
        }
        private static bool ProcedureHasParams(SqlConnection conn, SqlTransaction? tx, string procName, params string[] paramNames)
        {
            const string sql = @"
                SELECT p.name
                FROM sys.parameters p
                WHERE p.object_id = OBJECT_ID(@proc, 'P');";

            var existing = conn.Query<string>(sql, new { proc = procName }, transaction: tx)
                .Select(s => s.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var p in paramNames)
            {
                var name = p.StartsWith("@", StringComparison.Ordinal) ? p : "@" + p;
                if (!existing.Contains(name))
                    return false;
            }

            return true;
        }
        private static int GetSysPlanPagos(SqlConnection conn, SqlTransaction? tx, int codEmpresa)
        {
            if (ObjectExists(conn, tx, "dbo.SIF_EMPRESA", "U") && ColumnExists(conn, tx, "dbo.SIF_EMPRESA", "SysPlanPagos"))
            {
                const string sql = "SELECT ISNULL(SysPlanPagos,0) FROM SIF_EMPRESA WHERE PORTAL_ID = @CodEmpresa;";
                return conn.QueryFirstOrDefault<int>(sql, new { CodEmpresa = codEmpresa }, transaction: tx);
            }

            if (ObjectExists(conn, tx, "dbo.fxSysPlanPagos", "FN"))
            {
                const string sql = "SELECT dbo.fxSysPlanPagos(@CodEmpresa) AS Val;";
                return conn.QueryFirstOrDefault<int>(sql, new { CodEmpresa = codEmpresa }, transaction: tx);
            }

            return 0;
        }
        private static bool PersonaCongelada(SqlConnection conn, SqlTransaction? tx, string cedula, string tipo)
        {
            if (ObjectExists(conn, tx, "dbo.fxgCongelamiento", "FN"))
            {
                const string sql = "SELECT dbo.fxgCongelamiento(@Cedula, @Tipo) AS Val;";
                var val = conn.QueryFirstOrDefault<object>(sql, new { Cedula = cedula, Tipo = tipo }, transaction: tx);
                var s = (Convert.ToString(val, CultureInfo.InvariantCulture) ?? string.Empty).Trim();
                return s == "1" || s.Equals("S", StringComparison.OrdinalIgnoreCase) || s.Equals("SI", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
        private static int GetFechaCR(SqlConnection conn, SqlTransaction tx)
        {
            if (ObjectExists(conn,tx, "dbo.fxFechaCR", "FN"))
            {
                const string sql = "SELECT dbo.fxFechaCR() AS Val;";
                return conn.QueryFirstOrDefault<int>(sql, transaction: tx);
            }
            return 0;
        }
        private static int UltimaOperacion(SqlConnection conn, SqlTransaction tx, string cedula)
        {
            return conn.QueryFirst<int>(
                "SELECT ISNULL(MAX(id_solicitud),0) FROM reg_creditos WHERE cedula=@Cedula;",
                new { Cedula = cedula },
                tx
            );
        }
        private sealed record AplicarValidacion(
    string Usuario,
    string Notas,
    decimal CuotaBe,
    ErrorDto<CoReadecuacionCambioOperacionAplicarResponse>? Error);
        private sealed record AplicarCtx(
    int CodEmpresa,
    CoReadecuacionCambioOperacionAplicarRequest Req,
    AplicarValidacion Valid,
    OperacionActivaInfo Op,
    Globales Globales,
    DateTime FechaServidor,
    string TipoDoc,
    string Concepto,
    DocumentoInfo Doc
);
        private sealed record OperacionActivaInfo(
            IDictionary<string, object?>? Dop,
            string Cedula,
            string Nombre,
            string Descripcion,
            string CodigoLinea,
            int DiaPagoOld,
            string IndDeduc,
            int Opex,
            ErrorDto<CoReadecuacionCambioOperacionAplicarResponse>? Error);
        private sealed record DocumentoInfo(
            string TipoMov,
            string NumDocStr,
            ErrorDto<CoReadecuacionCambioOperacionAplicarResponse>? Error);
        private static AplicarValidacion ValidarAplicar(CoReadecuacionCambioOperacionAplicarRequest req)
        {
            if (req.id_tramite <= 0)
            {
                return new AplicarValidacion(
                    Usuario: string.Empty,
                    Notas: string.Empty,
                    CuotaBe: 0m,
                    Error: DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>(OPERACION_INVALIDA, -2));
            }

            var pNotas = (req.notas ?? string.Empty).Trim();
            if (pNotas.Length < 10)
            {
                return new AplicarValidacion(
                    Usuario: string.Empty,
                    Notas: pNotas,
                    CuotaBe: 0m,
                    Error: DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>(NOTA_INVALIDA, -2));
            }

            var pUsuario = (req.usuario_sesion ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pUsuario))
            {
                return new AplicarValidacion(
                    Usuario: pUsuario,
                    Notas: pNotas,
                    CuotaBe: 0m,
                    Error: DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>("Usuario sesión inválido.", -2));
            }

            if (req.no_monto <= 0 || req.no_plazo <= 0 || req.no_tasa < 0)
            {
                return new AplicarValidacion(
                    Usuario: pUsuario,
                    Notas: pNotas,
                    CuotaBe: 0m,
                    Error: DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>("Monto/Tasa/Plazo no son válidos.", -2));
            }

            var cuotaBe = MCobroDb.fxCalcula_Cuota(req.no_monto ?? 0m, req.no_plazo ?? 0, req.no_tasa ?? 0m, "M");
            if (!CuotaCoincide(cuotaBe, req.no_cuota ?? 0m))
            {
                return new AplicarValidacion(
                    Usuario: pUsuario,
                    Notas: pNotas,
                    CuotaBe: cuotaBe,
                    Error: DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>(
                        "Cuota no coincide con el cálculo del sistema.", -2));
            }

            return new AplicarValidacion(
                Usuario: pUsuario,
                Notas: pNotas,
                CuotaBe: cuotaBe,
                Error: null);
        }
        private static OperacionActivaInfo ObtenerOperacionActiva(SqlConnection conn, int idTramite)
        {
            const string sqlOp = @"
        SELECT
            R.id_solicitud,
            R.cedula,
            R.codigo,
            C.descripcion,
            S.nombre,
            R.plazo,
            R.interesv,
            R.saldo,
            R.montoapr,
            R.opex,
            R.dia_pago,
            R.ind_deduce_planilla,
            R.id_comite,
            R.acta,
            R.garantia,
            R.pagare,
            R.premio,
            R.fecult,
            R.TBP_PuntosAdd,
            R.LiqTasa,
            R.cod_oficina_r,
            R.cod_oficina_f,
            R.cod_oficina_comision,
            R.cod_grupo
        FROM reg_creditos R
        INNER JOIN Socios S ON R.cedula = S.cedula
        INNER JOIN catalogo C ON R.codigo = C.codigo
        WHERE R.id_solicitud = @Id AND R.estado = 'A';";

            var op = conn.QueryFirstOrDefault(sqlOp, new { Id = idTramite });
            if (op == null)
            {
                return new OperacionActivaInfo(
                    Dop: null,
                    Cedula: string.Empty,
                    Nombre: string.Empty,
                    Descripcion: string.Empty,
                    CodigoLinea: string.Empty,
                    DiaPagoOld: 0,
                    IndDeduc: string.Empty,
                    Opex: 0,
                    Error: DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>(OPERACION_INVALIDA, -2));
            }

            var dop = (IDictionary<string, object?>)op;

            return new OperacionActivaInfo(
                Dop: dop,
                Cedula: S(V(dop, "cedula")),
                Nombre: S(V(dop, "nombre")),
                Descripcion: S(V(dop, "descripcion")),
                CodigoLinea: S(V(dop, "codigo")),
                DiaPagoOld: ToInt(V(dop, "dia_pago")),
                IndDeduc: S(V(dop, "ind_deduce_planilla")).Trim().ToUpperInvariant(),
                Opex: ToInt(V(dop, "opex")),
                Error: null);
        }
        private static DocumentoInfo ResolverDocumento(SqlConnection conn,SqlTransaction tx,int sysDocVersion,string tipoDoc,string usuario)
        {
            if (sysDocVersion == 2)
                return ResolverDocumentoV2(conn, tx, tipoDoc, usuario);

            return new DocumentoInfo(
                TipoMov: "4",
                NumDocStr: "8889",
                Error: null);
        }
        private static DocumentoInfo ResolverDocumentoV2(SqlConnection conn,SqlTransaction tx,string tipoDoc,string usuario)
        {
            if (!ObjectExists(conn, tx, "dbo.spSIFDocsConsecutivo", "P"))
            {
                return new DocumentoInfo(
                    TipoMov: string.Empty,
                    NumDocStr: string.Empty,
                    Error: DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>(
                        "No existe dbo.spSIFDocsConsecutivo para obtener consecutivo de documento.", -2));
            }

            var hasUsuario = ProcedureHasParams(conn, tx, "dbo.spSIFDocsConsecutivo", TIPO, "@Usuario");

            object? consObj = hasUsuario
                ? conn.QueryFirstOrDefault<object>(
                    "exec dbo.spSIFDocsConsecutivo @Tipo, @Usuario;",
                    new { Tipo = tipoDoc, Usuario = usuario },
                    tx)
                : conn.QueryFirstOrDefault<object>(
                    "exec dbo.spSIFDocsConsecutivo @Tipo;",
                    new { Tipo = tipoDoc },
                    tx);

            var cons = ToInt(consObj);
            if (cons <= 0)
            {
                return new DocumentoInfo(
                    TipoMov: string.Empty,
                    NumDocStr: string.Empty,
                    Error: DbHelper.CreateErrorResponse<CoReadecuacionCambioOperacionAplicarResponse>(
                        $"No fue posible obtener consecutivo para documento {tipoDoc}.", -2));
            }

            return new DocumentoInfo(
                TipoMov: "REA",
                NumDocStr: cons.ToString(CultureInfo.InvariantCulture),
                Error: null);
        }
        private static void EjecutarCancelacion(SqlConnection conn, SqlTransaction tx, AplicarCtx ctx)
        {
            var req = ctx.Req;
            var op = ctx.Op;
            var pUsuario = ctx.Valid.Usuario;

            if (ctx.Globales.SysPlanPagos == 1)
            {
                const string sp = "exec spCrdPlanPagoAbonoCancelacion @Id, @Concepto, @Usuario, @TipoDoc, @NumDoc, @Monto, @Fecha, @Empty;";
                conn.Execute(sp, new
                {
                    Id = req.id_tramite,
                    Concepto = ctx.Concepto,
                    Usuario = pUsuario,
                    TipoDoc = ctx.TipoDoc,
                    NumDoc = ctx.Doc.NumDocStr,
                    Monto = req.no_monto,
                    Fecha = ctx.FechaServidor.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                    Empty = ""
                }, tx);

                return;
            }

            const string updMor = @"
            UPDATE morosidad
            SET estado = 'C',
                abintc = intc,
                abintm = intm,
                abamortiza = amortiza,
                abCargo = cargo,
                tcon = @Tcon,
                ncon = @Ncon,
                fecult = dbo.MyGetdate(),
                usuario = @Usuario,
                cod_concepto = @Concepto,
                cod_caja = ''
            WHERE estado = 'A' AND id_solicitud = @Id;";

            conn.Execute(updMor, new
            {
                Id = req.id_tramite,
                Tcon = ctx.Doc.TipoMov,
                Ncon = ctx.Doc.NumDocStr,
                Usuario = pUsuario,
                Concepto = ctx.Concepto
            }, tx);

            const string sqlAm = @"
                SELECT ISNULL(SUM(abamortiza),0)
                FROM morosidad
                WHERE tcon=@Tcon AND ncon=@Ncon AND id_solicitud=@Id;";

            var amortiza = conn.QueryFirstOrDefault<decimal>(
                sqlAm,
                new { Tcon = ctx.Doc.TipoMov, Ncon = ctx.Doc.NumDocStr, Id = req.id_tramite },
                tx);

            var interesTotal = conn.QueryFirstOrDefault<decimal>(
                "SELECT dbo.fxCRDCalculoIntCorte(@Id, dbo.MyGetdate());",
                new { Id = req.id_tramite },
                tx);

            var saldoActual = ToDec(V(op.Dop!, "saldo"));
            var intCorVenc = interesTotal;

            var fechaP = GetFechaCR(conn, tx);

            const string insDt = @"
                INSERT creditos_dt(
                    CODIGO,ID_SOLICITUD,CUOTA,ABONO,INTCP,AMORTIZA,FECHAS,FECHAP,TCON,NCON,ESTADO,ESTADO_ASIENTO,Usuario,Cod_Concepto,Cod_Caja
                )
                VALUES(
                    @Codigo,@Id,@Cuota,@Abono,@Intcp,@Amortiza,dbo.MyGetdate(),@FechaP,@Tcon,@Ncon,'A','G',@Usuario,@Concepto,''
                );";

            conn.Execute(insDt, new
            {
                Codigo = op.CodigoLinea,
                Id = req.id_tramite,
                Cuota = (intCorVenc + saldoActual - amortiza),
                Abono = (saldoActual - amortiza),
                Intcp = intCorVenc,
                Amortiza = (saldoActual - amortiza),
                FechaP = fechaP,
                Tcon = ctx.Doc.TipoMov,
                Ncon = ctx.Doc.NumDocStr,
                Usuario = pUsuario,
                Concepto = ctx.Concepto
            }, tx);

            const string updOp = @"
                UPDATE reg_creditos
                SET saldo = 0,
                    amortiza = montoapr,
                    saldo_mes = 0,
                    estado = 'C',
                    FECHA_ENVIAPROCESO = dbo.MyGetdate(),
                    OBSERVACION_PROCESO = 'Readecuación de Deuda'
                WHERE id_solicitud = @Id;";

            conn.Execute(updOp, new { Id = req.id_tramite }, tx);
        }
        private static int CalcularDiaPago(bool? chkDiaPago, int diaPagoOld, DateTime fechaServidor, string indDeduc)
        {
            var vDiaPago = chkDiaPago == true ? diaPagoOld : fechaServidor.Day;
            if (indDeduc == "S" && vDiaPago < 32) vDiaPago = 32;
            return vDiaPago;
        }
        private static int InsertarNuevaOperacion(SqlConnection conn,SqlTransaction tx,AplicarCtx ctx,int primerDeduccion,int diaPago)
        {
            var req = ctx.Req;
            var op = ctx.Op;
            var valid = ctx.Valid;

            const string insOp = @"
            INSERT reg_creditos(
                codigo,id_comite,cedula,montosol,estadosol,fechasol,fechares,plazo,int,montoapr,prideduc,fechaforp,fechaforf,acta,saldo,amortiza,interesc,
                cuota,estado,opex,proceso,userrec,userres,userfor,garantia,observacion,firma_deudor,monto_girado,interesv,tesoreria,usertesoreria,primer_cuota,
                tdocumento,ndocumento,pagare,fecha_calculo_int,premio,cuotas_planilla,cuotas_directas,cuotas_anuladas,FECULT,TBP_PuntosAdd,
                LiqTasa,cod_oficina_r,cod_oficina_f,cod_oficina_comision,referencia,fecha_registro,DIA_PAGO, IND_DEDUCE_PLANILLA
            )
            VALUES(
                @Codigo,@IdComite,@Cedula,@Monto,'F',@F,@F,@Plazo,@Tasa,@Monto,@PriDeduc,@F,@F,@Acta,@Monto,0,0,
                @Cuota,'A',@Opex,'N',@Usuario,@Usuario,@Usuario,@Garantia,@Obs,
                1,0,@Tasa,@F,@Usuario,'N','ND',
                @OldId,@Pagare,@F,@Premio,
                0,0,0,@FecUlt,@TbpAdd,
                @LiqTasa,@OfR,@OfF,@OfC,@Referencia,dbo.MyGetdate(),@DiaPago,@IndDeduc
            );
            SELECT CAST(SCOPE_IDENTITY() AS int) AS NewId;";

            var newId = conn.QueryFirstOrDefault<int>(insOp, new
            {
                Codigo = op.CodigoLinea,
                IdComite = ToInt(V(op.Dop!, "id_comite")),
                Cedula = op.Cedula,
                Monto = req.no_monto,
                F = ctx.FechaServidor.ToString(FORMATO_FECHA, CultureInfo.InvariantCulture),
                Plazo = req.no_plazo,
                Tasa = req.no_tasa,
                PriDeduc = primerDeduccion,
                Acta = ToInt(V(op.Dop!, "acta")),
                Cuota = valid.CuotaBe,
                Opex = op.Opex,
                Usuario = valid.Usuario,
                Garantia = S(V(op.Dop!, "garantia")),
                Obs = valid.Notas,
                OldId = req.id_tramite,
                Pagare = ToInt(V(op.Dop!, "pagare")),
                Premio = ToDec(V(op.Dop!, "premio")),
                FecUlt = V(op.Dop!, "fecult"),
                TbpAdd = V(op.Dop!, "TBP_PuntosAdd"),
                LiqTasa = V(op.Dop!, "LiqTasa"),
                OfR = V(op.Dop!, "cod_oficina_r"),
                OfF = V(op.Dop!, "cod_oficina_f"),
                OfC = V(op.Dop!, "cod_oficina_comision"),
                Referencia = req.id_tramite,
                DiaPago = diaPago,
                IndDeduc = op.IndDeduc
            }, tx);

            return newId > 0 ? newId : UltimaOperacion(conn, tx, op.Cedula);
        }
        private static void CopiarFiadores(SqlConnection conn, SqlTransaction tx, int nuevaOperacion, int oldOperacion)
        {
            const string insF = @"
            INSERT INTO fiadores(id_solicitud,codigo,cedulaf,nombre,firma,estado,interno)
            SELECT @NewId,codigo,cedulaf,nombre,firma,estado,interno
            FROM fiadores
            WHERE id_solicitud = @OldId;";

            conn.Execute(insF, new { NewId = nuevaOperacion, OldId = oldOperacion }, tx);
        }
    }
}