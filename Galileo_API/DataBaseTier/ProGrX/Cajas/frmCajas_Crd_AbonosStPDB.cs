using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
#pragma warning restore S6680 // Deshabilita regla Sonar S6680 (DoS: limita recursos asignados por input del usuario)
    /// <summary>
    /// Migración VB6: frmCajas_Crd_AbonosStP -> Backend .NET (Dapper)
    /// Nota: Este archivo intenta ser "drop-in" con el resto del proyecto (DbHelper, modelos y helpers existentes).
    /// </summary>
    public class FrmCajasCrdAbonosStpDB
    {
        private readonly PortalDB _portalDB;
        private readonly MRecibos _mRecibos;
        private readonly MProGrxMain _mProGrx;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly MCajas _mCajas;
        private readonly MCobroDb _mCobro;
        private readonly MAfilicacionDB _mAfilicacion;

        public FrmCajasCrdAbonosStpDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mRecibos = new MRecibos(config);
            _mProGrx = new MProGrxMain(config);
            _mSecurityMainDb = new MSecurityMainDb(config);
            _mCajas = new MCajas(config);
            _mCobro = new MCobroDb(config);
            _mAfilicacion = new MAfilicacionDB(config);
        }

        #region Consultas base

        /// <summary>
        /// Obtiene los documentos (tipoDoc) disponibles para una caja.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CajasCrdAbonosSt_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT 
                        RTRIM(C.tipo_documento) AS item,
                        RTRIM(D.Descripcion) AS descripcion
                    FROM SIF_DOCUMENTOS D
                    INNER JOIN CAJAS_DOCUMENTOS C 
                        ON D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO
                    WHERE 
                        C.cod_caja = @codCaja
                        AND D.Tipo_Movimiento IN ('A', 'C')
                    ORDER BY C.tipo_documento";

                return conn.Query<DropDownListaGenericaModel>(query, new { codCaja }).ToList();
            });
        }

        /// <summary>
        /// Lista operaciones (créditos) activas.
        /// </summary>
        public ErrorDto<List<CajasCrdAbonosStPDData>> CajasCrdAbonosSt_Operaciones_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT 
                        R.id_solicitud ,
                        R.codigo,
                        S.cedula,
                        S.nombre,
                        C.descripcion
                    FROM REG_CREDITOS R
                    INNER JOIN SOCIOS S 
                        ON R.cedula = S.cedula
                    INNER JOIN CATALOGO C 
                        ON R.codigo = C.codigo
                    WHERE 
                        R.estado = 'A'
                    ORDER BY R.cedula";

                return conn.Query<CajasCrdAbonosStPDData>(query).ToList();
            });
        }

        /// <summary>
        /// Wrapper del parámetro de crédito (VB: fxCrdParametro).
        /// </summary>
        public ErrorDto<int> CajasCrdAbonosSt_fxCrdParametro(int codEmpresa, string parametro)
        {
            var iDias = MCredito.fxCrdParametro(DbHelper.OpenConnection(_portalDB, codEmpresa), parametro);
            return DbHelper.CreateOkResponse(iDias != null ? int.Parse(iDias) : 0);
        }

        /// <summary>
        /// Carga operación por id_solicitud y devuelve datos para pantalla.
        /// </summary>
        public ErrorDto<CajasCrdAbonosStPDData> CajasCrdAbonosSt_ConsultaOperacion_Obtener(int codEmpresa, string codCaja, int operacionId)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT
                        R.id_solicitud,
                        R.saldo,
                        R.saldo - ISNULL(V.amortiza, 0) AS Saldo_mes,
                        R.proceso,
                        ISNULL(R.cod_Divisa, 'COL') AS Divisa,
                        R.interesv,
                        R.int,
                        R.plazo,
                        R.interesc,
                        R.amortiza,
                        R.fecult,
                        R.Prideduc,
                        R.opex,
                        R.cuota,
                        R.codigo,
                        R.cedula,
                        R.cuotas_planilla,
                        R.cuotas_directas,
                        R.montoApr,
                        S.nombre,
                        C.descripcion,
                        C.retencion,
                        C.poliza,
                        R.fechaforp,
                        C.PORC_CARGO_CANCELACION,
                        R.Base_Calculo,
                        dbo.fxCajas_Valida_Auxiliar(@CodCaja, 'CRD', R.Codigo) AS Caja_Valida_Concepto
                    FROM reg_creditos R
                    INNER JOIN Catalogo C ON R.codigo = C.codigo
                    INNER JOIN Socios S ON R.cedula = S.cedula
                    LEFT JOIN vista_morosidad V ON R.id_solicitud = V.id_solicitud
                    WHERE R.estado = 'A'
                      AND R.saldo > 0
                      AND R.ID_SOLICITUD = @OperacionId";

                var op = conn.QueryFirstOrDefault<CajasCrdAbonosStPDData>(query, new { CodCaja = codCaja, OperacionId = operacionId })
                         ?? new CajasCrdAbonosStPDData();

                // Ajustes de compatibilidad con VB6 (saldo_mes nunca negativo)
                op.saldo_mes = op.saldo_mes < 0 ? 0 : op.saldo_mes;
                if (op.saldo_mes == 0)
                {
                    const string updateSql = "UPDATE reg_creditos SET saldo_mes = saldo WHERE id_solicitud = @id_solicitud";
                    conn.Execute(updateSql, new { id_solicitud = op.id_solicitud });
                    op.saldo_mes = op.saldo;
                }

                // glngFechaCR en formato yyyymm
                string vFecha = $"{DateTime.Now:yyyyMM}";
                op.glngFechaCR = Convert.ToInt32(vFecha);

                // VB: lblFecUltMov = IIf(IsNull(rs!FecUlt), fxFechaProcesoAnterior(glngFechaCR), rs!FecUlt)
                op.fecult = op.fecult ?? _mCobro.fxFechaProcesoAnterior(codEmpresa, (int)op.glngFechaCR);

                // Si fecult < proceso actual, forzar a proceso anterior (misma lógica del código que traías)
                if (op.fecult.HasValue && op.fecult.Value < op.glngFechaCR)
                    op.fecult = _mCobro.fxFechaProcesoAnterior(codEmpresa, (int)op.glngFechaCR);

                return op;
            });
        }

        /// <summary>
        /// Carga operación por cédula + código (para búsqueda directa).
        /// </summary>
        public ErrorDto<CajasCrdAbonoCargaOperacionData> CajasCrdAbonosSt_CargaOperacionCodCed(int codEmpresa, string cedula, string codigo)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT 
                        R.id_solicitud,
                        R.saldo,
                        R.saldo_mes,
                        R.interesv,
                        R.int,
                        R.plazo,
                        R.interesc, 
                        R.amortiza,
                        R.fecult,
                        R.opex,
                        R.cuota,
                        R.codigo,
                        R.cedula,
                        R.cuotas_planilla,
                        R.cuotas_directas,
                        C.retencion,
                        C.poliza
                    FROM reg_creditos R
                    INNER JOIN catalogo C 
                        ON R.codigo = C.codigo
                    WHERE 
                        R.estado = 'A'
                        AND R.proceso <> 'N'
                        AND R.saldo > 0
                        AND R.cedula = @cedula
                        AND R.codigo = @codigo";

                return conn.QueryFirstOrDefault<CajasCrdAbonoCargaOperacionData>(query, new { cedula, codigo })
                       ?? new CajasCrdAbonoCargaOperacionData();
            });
        }

        #endregion

        #region Mora (spCajas_Crd_MoraConsulta)

        /// <summary>
        /// Consulta morosidad (lista).
        /// </summary>
        public ErrorDto<List<CajasCrdAbonoMorosidadData>> CajasCrdAbonosSt_MoraConsulta(int codEmpresa, int operacion, DateTime fechaPago)
        {
            string sql = "exec spCajas_Crd_MoraConsulta @Operacion, @FechaPago";
            var parameters = new { Operacion = operacion, FechaPago = fechaPago };
            return DbHelper.ExecuteListQuery<CajasCrdAbonoMorosidadData>(_portalDB, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Consulta morosidad (resumen con totales) - recomendado para la UI.
        /// </summary>
        public ErrorDto<MoraConsultaResponse> CajasCrdAbonosSt_MoraConsultaResumen(int codEmpresa, long operacion, DateTime fechaPago)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = "exec spCajas_Crd_MoraConsulta @Operacion, @FechaPago";
                var items = conn.Query<CajasCrdAbonoMorosidadData>(sql, new { Operacion = operacion, FechaPago = fechaPago }).ToList();

                var cuotas = items.Count;
                var amort = items.Sum(x => x.amortiza);
                var interes = items.Sum(x => x.intc + x.intm);
                var cargos = items.Sum(x => x.cargo);
                var total = items.Sum(x => x.total);

                var resp = new MoraConsultaResponse
                {
                    Items = items,
                    Cuotas = cuotas,
                    Amortiza = amort,
                    Interes = interes,
                    Cargos = cargos,
                    Total = total,
                    PermiteExtraordinario = cuotas <= 1
                };

                return resp;
            });
        }

        #endregion

        /// <summary>
        /// Simula cuotas / proyección (port de txtCuotas_Change VB6) en backend.
        /// </summary>
        public ErrorDto<SimularCuotasResponse> CajasCrdAbonosSt_SimularCuotas(int codEmpresa, SimularCuotasRequest req)
        {
            try
            {
                var v = ValidarSimulacion(req);
                if (v.Code != 0)
                    return DbHelper.CreateErrorResponse<SimularCuotasResponse>(v.Description ?? string.Empty);

               
                var cuotas = Clamp(req.CantidadCuotas ?? 0, 0, MAX_CUOTAS);

                if (cuotas == 0)
                    return DbHelper.CreateOkResponse(new SimularCuotasResponse());

                var lngFecha = AjustarFechaInicial(codEmpresa, req);
                var curSaldo = CalcularSaldoInicial(req);

                if (curSaldo <= 0)
                    return DbHelper.CreateOkResponse(new SimularCuotasResponse
                    {
                        Proyeccion = new List<ProyeccionCuotaDto>(),
                        FecUltMovR = lngFecha
                    });

                // ✅ Capacidad basada en valor ya limitado
                var proy = CrearListaProyeccion(cuotas);

                var estado = new SimulacionEstado
                {
                    FechaProceso = lngFecha,
                    Saldo = curSaldo,
                    Cuota = req.Cuota ?? 0m
                };

                var totales = new TotalesSimulacion();

                int cuotaMax;
                if (EsBase360(req.BaseCalculo))
                    cuotaMax = SimularBase360(codEmpresa, req, cuotas, proy, estado, totales);
                else
                    cuotaMax = SimularBase365(codEmpresa, req, cuotas, proy, estado, totales);

                var resp = ConstruirRespuesta(req, proy, estado, totales, cuotaMax);

                // Defensa final: por si algo agregara más de lo permitido
                if (resp.Proyeccion != null && resp.Proyeccion.Count > MAX_CUOTAS)
                    resp.Proyeccion = resp.Proyeccion.Take(MAX_CUOTAS).ToList();

                return DbHelper.CreateOkResponse(resp);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<SimularCuotasResponse>("Error simulando cuotas: " + ex.Message, -1);
            }
        }

        private const int MAX_CUOTAS = 480; 
        private const int MAX_PLAZO = 1200;

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static ErrorDto ValidarSimulacion(SimularCuotasRequest req)
        {
            if (req == null)
                return DbHelper.ErrorResponse("Request inválido.");

            // Permites 0 => respuesta vacía (mantiene tu comportamiento)
            if (req.CantidadCuotas < 0 || req.CantidadCuotas > MAX_CUOTAS)
                return DbHelper.ErrorResponse($"CantidadCuotas fuera de rango (0..{MAX_CUOTAS}).");

            if (req.Interes < 0m || req.Interes > 200m)
                return DbHelper.ErrorResponse("Interés fuera de rango.");

            if (req.Plazo < 0 || req.Plazo > MAX_PLAZO)
                return DbHelper.ErrorResponse($"Plazo fuera de rango (0..{MAX_PLAZO}).");

            return DbHelper.CreateOkResponse();
        }

        private long AjustarFechaInicial(int codEmpresa, SimularCuotasRequest req)
        {
            long lngFecha = req.FecUltMov.GetValueOrDefault();
            long priDeduc = req.PriDeduc.GetValueOrDefault();

            // Ajuste fecha vs prideduc
            if (lngFecha < priDeduc)
                lngFecha = (long)_mCobro.fxFechaProcesoAnterior(codEmpresa, (int)priDeduc);

            return lngFecha;
        }

        private static decimal CalcularSaldoInicial(SimularCuotasRequest req)
        {
            if (req.EsRetencion != true)
                return req.SaldoMes ?? 0m;

            // reglas heredadas VB6
            return (req.Plazo.HasValue && req.Plazo.Value > 900)
                ? (req.Cuota ?? 0m) * 20m
                : ((req.Cuota ?? 0m) * (req.Plazo ?? 0)) - (req.AmortizaActual ?? 0m);
        }

        private static bool EsBase360(string? baseCalculo) =>
            string.Equals(baseCalculo, "01", StringComparison.OrdinalIgnoreCase);

        private static List<ProyeccionCuotaDto> CrearListaProyeccion(int cantidadCuotasSafe)
        {
            // ✅ Capar otra vez por defensa y para que Sonar no discuta capacidades
            var capacidad = Clamp(cantidadCuotasSafe, 0, MAX_CUOTAS);
            return new List<ProyeccionCuotaDto>(capacidad);
        }

        private int SimularBase360(
    int codEmpresa,
    SimularCuotasRequest req,
    int cuotas, // ✅ límite sanitizado
    List<ProyeccionCuotaDto> proy,
    SimulacionEstado estado,
    TotalesSimulacion totales)
        {
            int cuotaMax = 0;

            // ✅ Loop bound NO viene directo de req.*
            for (int i = 1; i <= cuotas; i++)
            {
                if (estado.Saldo <= 0) break;

                cuotaMax = i;

                var tmpInter = (estado.Saldo * req.Interes) / 1200m;
                var tmpInterValue = tmpInter ?? 0m;
                decimal tmpAmort = (req.Cuota - tmpInter) ?? 0m;

                totales.TotalInteres += tmpInterValue;
                totales.TotalAmortiza += tmpAmort;

                estado.Saldo -= tmpAmort;
                estado.FechaProceso = (long)_mCobro.fxFechaProcesoSiguiente(codEmpresa, estado.FechaProceso);

                proy.Add(CrearFila(tmpInterValue, tmpAmort, estado.FechaProceso, estado.Saldo, estado.Cuota));

                if (estado.Saldo < 0)
                {
                    AjustarSaldoFinalNegativo(proy, totales, estado);
                    break;
                }
            }

            return cuotaMax;
        }

        private int SimularBase365(
    int codEmpresa,
    SimularCuotasRequest req,
    int cuotas, // ✅ límite sanitizado
    List<ProyeccionCuotaDto> proy,
    SimulacionEstado estado,
    TotalesSimulacion totales)
        {
            int cuotaMax = 0;

            var plazoRst = CalcularPlazoRestante(codEmpresa, req, estado.FechaProceso);
            var baseDate = ParseProcesoToFirstDay(estado.FechaProceso);

            // ✅ Loop bound NO viene directo de req.*
            for (int i = 1; i <= cuotas; i++)
            {
                cuotaMax = i;

                var dias = CalcularDiasPeriodo(req, plazoRst, baseDate);

                decimal tmpInter = (estado.Saldo * (req.Interes / 100m) * dias / 360m) ?? 0m;
                decimal tmpAmort = estado.Cuota - tmpInter;

                totales.TotalInteres += tmpInter;
                totales.TotalAmortiza += tmpAmort;

                estado.Saldo -= tmpAmort;
                estado.FechaProceso = (long)_mCobro.fxFechaProcesoSiguiente(codEmpresa, estado.FechaProceso);
                baseDate = baseDate.AddMonths(1);

                proy.Add(CrearFila(tmpInter, tmpAmort, estado.FechaProceso, estado.Saldo, estado.Cuota));

                plazoRst = Math.Max(1, plazoRst - 1);
                estado.Cuota = MCobroDb.fxCalcula_Cuota(estado.Saldo, plazoRst, req.Interes!, "M");
            }

            return cuotaMax;
        }

        private static ProyeccionCuotaDto CrearFila(decimal interes, decimal amortiza, long fechaProceso, decimal saldo, decimal cuota) =>
            new ProyeccionCuotaDto
            {
                Interes = interes,
                Amortiza = amortiza,
                FechaProceso = fechaProceso,
                Saldo = saldo,
                Cuota = cuota
            };

        private static void AjustarSaldoFinalNegativo(List<ProyeccionCuotaDto> proy, TotalesSimulacion totales, SimulacionEstado estado)
        {
            // estado.Saldo es negativo aquí: se corrige última fila y totales.
            var saldoNeg = estado.Saldo;

            // reemplaza última fila (mismo patrón que tu código original)
            proy[^1] = new ProyeccionCuotaDto
            {
                Interes = 0m,
                Amortiza = saldoNeg,
                FechaProceso = estado.FechaProceso,
                Saldo = 0m,
                Cuota = saldoNeg
            };

            totales.TotalAmortiza += saldoNeg;
            estado.Saldo = 0m;
        }

        private int CalcularPlazoRestante(int codEmpresa, SimularCuotasRequest req, long fechaProceso)
        {
            // Límite defensivo para evitar DoS / datos corruptos.
            // 2400 = 200 años en meses. Ajusta a tu negocio (por ejemplo 1200 = 100 años).
            const int MAX_ITER = 2400;

            long procesosTmp = req.PriDeduc ?? 0L;
            int pasos = 0;

            // Si fechaProceso es inválida o está "antes", no iteres.
            if (fechaProceso <= 0 || procesosTmp <= 0)
                return 1;

            // Si ya estamos al día o adelante, no hay que avanzar.
            if (procesosTmp >= fechaProceso)
                return Math.Max(1, req.Plazo ?? 1);

            while (procesosTmp < fechaProceso && pasos < MAX_ITER)
            {
                procesosTmp = (long)_mCobro.fxFechaProcesoSiguiente(codEmpresa, procesosTmp);
                pasos++;
            }

            // Si no logramos alcanzar fechaProceso dentro del límite,
            // asumimos que los datos vienen fuera de rango -> devolvemos un mínimo seguro.
            if (procesosTmp < fechaProceso)
                return 1;

            // Cálculo defensivo: si Plazo viniera nulo, usamos un mínimo seguro.
            var plazoBase = req.Plazo ?? 1;
            var plazoRst = plazoBase - pasos;
            return Math.Max(1, plazoRst);
        }

        private static int CalcularDiasPeriodo(SimularCuotasRequest req, int plazoRst, DateTime baseDate)
        {
            if (plazoRst == 1 || plazoRst == req.Plazo)
                return 30;

            return DateTime.DaysInMonth(baseDate.Year, baseDate.Month);
        }

        private static SimularCuotasResponse ConstruirRespuesta(
            SimularCuotasRequest req,
            List<ProyeccionCuotaDto> proy,
            SimulacionEstado estado,
            TotalesSimulacion totales,
            int cuotaMax)
        {
            // saldoR según regla VB (defensivo ante nulos)
            decimal saldoR = req.EsRetencion == true
                ? (req.Cuota ?? 0m)
                : ((req.SaldoMes ?? 0m) - totales.TotalAmortiza);

            return new SimularCuotasResponse
            {
                Proyeccion = proy,
                TotalInteres = totales.TotalInteres,
                TotalAmortiza = totales.TotalAmortiza,
                FecUltMovR = estado.FechaProceso,
                CuotaR = estado.Cuota,
                SaldoR = saldoR,
                CuotasMaximas = cuotaMax
            };
        }

        private sealed class SimulacionEstado
        {
            public long FechaProceso { get; set; } = 0;
            public decimal Saldo { get; set; } = 0;
            public decimal Cuota { get; set; } = 0;
        }

        private sealed class TotalesSimulacion
        {
            public decimal TotalAmortiza { get; set; }
            public decimal TotalInteres { get; set; }
        }

        /// <summary>
        /// Recalcula cuota (port de txtCompromiso_Change cuando chkRecalculaCuota = True).
        /// </summary>
        public ErrorDto<RecalculaCuotaResponse> CajasCrdAbonosSt_RecalcularCuota(int codEmpresa, RecalculaCuotaRequest req)
        {
            try
            {
                long lngFecha = req.FecUltMovR;
                if (lngFecha < req.PriDeduc) lngFecha = req.PriDeduc;

                if (!EsProcesoValido(req.PriDeduc) || !EsProcesoValido(lngFecha))
                    return DbHelper.CreateErrorResponse<RecalculaCuotaResponse>("Proceso inválido (se espera yyyymm).");

                const int MAX_ITER_FECHAS = 1500;

                // Calcula “pasos” sin loop
                int pasos = DiffMeses(req.PriDeduc, lngFecha);
                pasos = Clamp(pasos, 0, MAX_ITER_FECHAS);

                // Si excede límite, se considera inválido
                if (DiffMeses(req.PriDeduc, lngFecha) > MAX_ITER_FECHAS)
                    return DbHelper.CreateErrorResponse<RecalculaCuotaResponse>(
                        "Rango de fechas inválido o excede el límite de iteraciones permitido.");

                var plazoRst = req.Plazo - pasos;
                if (plazoRst <= 0) plazoRst = 1;

                var cuotaR = MCobroDb.fxCalcula_Cuota(req.SaldoR, plazoRst, req.Interes, "M");

                return DbHelper.CreateOkResponse(new RecalculaCuotaResponse { CuotaR = cuotaR });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<RecalculaCuotaResponse>("Error recalculando cuota: " + ex.Message);
            }
        }

        // yyyymm válido: mes 01..12, año razonable
        private static bool EsProcesoValido(long yyyymm)
        {
            if (yyyymm <= 0) return false;

            int year = (int)(yyyymm / 100);
            int month = (int)(yyyymm % 100);

            if (month < 1 || month > 12) return false;
            if (year < 1900 || year > 3000) return false;

            return true;
        }

        // Diferencia aproximada en meses entre dos yyyymm (si b>=a)
        private static int DiffMeses(long aYyyymm, long bYyyymm)
        {
            int aYear = (int)(aYyyymm / 100);
            int aMonth = (int)(aYyyymm % 100);
            int bYear = (int)(bYyyymm / 100);
            int bMonth = (int)(bYyyymm % 100);

            int aTotal = aYear * 12 + (aMonth - 1);
            int bTotal = bYear * 12 + (bMonth - 1);

            return Math.Max(0, bTotal - aTotal);
        }

        private static DateTime ParseProcesoToFirstDay(long yyyymm)
        {
            if (yyyymm <= 0)
                throw new ArgumentException("Proceso inválido: se espera yyyymm.");

            var year = (int)(yyyymm / 100);
            var month = (int)(yyyymm % 100);

            if (month < 1 || month > 12)
                throw new ArgumentException("Proceso inválido: mes fuera de rango.");

            return new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        }

        #region Aplicar abono (spCajas_CrdAbono) + validaciones

        private const string SqlOperacionAbono = @"
            select R.id_solicitud, R.saldo, R.proceso, R.cedula, R.codigo, R.plazo, R.amortiza, R.cuota, R.montoApr, R.opex,
                   ISNULL(R.cod_Divisa, 'COL') as divisa, S.nombre, C.descripcion, C.retencion, C.poliza
              from reg_creditos R
             inner join Catalogo C on R.codigo = C.codigo
             inner join Socios S on R.cedula = S.cedula
             where R.id_solicitud = @id";

        /// <summary>
        /// Aplica abono (VB6 sbAbono): validaciones, spCajas_CrdAbono, bitácora,
        /// documento/asientos (sbDocumentoAbono) y devuelve "mensaje?numDoc" para imprimir el recibo.
        /// </summary>
        public ErrorDto CajasCrdAbonosSt_Abono_Aplica(int codEmpresa, CajasCrdAbonoRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                var tipoAbono = request.tipoAbono ?? 0;
                if (tipoAbono < 0 || tipoAbono > 2)
                    return DbHelper.ErrorResponse("Tipo de abono no válido.", -1);

                if (string.IsNullOrWhiteSpace(request.tipoDoc))
                    return DbHelper.ErrorResponse("Debe seleccionar el Documento (Tipo Doc).", -1);

                // Datos de la operación ANTES de aplicar (el SP modifica el saldo)
                var solicitud = conn.QueryFirstOrDefault<CajasCrdAbonosStPDData>(SqlOperacionAbono, new { id = request.id_solicitud });
                if (solicitud == null)
                    return DbHelper.ErrorResponse("- Número de Operación no es válido...", -1);

                var esRetencion = solicitud.retencion == "S" || solicitud.poliza == "S";
                // lblSaldo de pantalla (retención con plazo < 900: remanente por recaudar)
                var saldoAnterior = (esRetencion && (solicitud.plazo ?? 0) < 900)
                    ? ((solicitud.montoApr ?? 0m) * (solicitud.plazo ?? 0)) - (solicitud.amortiza ?? 0m)
                    : (solicitud.saldo ?? 0m);

                var valida = fxVerifica(codEmpresa, conn, request, solicitud, esRetencion, saldoAnterior);
                if (valida.Code != 0)
                    return valida;

                var vNotas = MProGrxMain.sbSIFCleanTxtInject(request.vNotas) ?? string.Empty;

                long vNumDoc = _mRecibos.FxDocumentoConsecutivo(codEmpresa, request.tipoDoc!);
                decimal glngFechaCR = _mProGrx.glngFechaCR(codEmpresa);

                if (request.lblFecUltMovR.HasValue && request.lblFecUltMovR.Value < (long)glngFechaCR)
                    request.lblFecUltMovR = (long)glngFechaCR;

                string vConcepto, vTipo, tipoTexto, bitacoraDetalle;
                switch (tipoAbono)
                {
                    case 1:
                        vConcepto = "CRD002"; vTipo = "E"; tipoTexto = "ABONO EXTRAORDINARIO";
                        bitacoraDetalle = "Abono ExtraOrd. " + (request.chkRecalculaCuota == 1 ? "Con Recal." : "Sin Recal") + " a la Op.: " + request.id_solicitud;
                        break;
                    case 2:
                        vConcepto = "CRD003"; vTipo = "O"; tipoTexto = "CANCELACION DE DEUDA";
                        bitacoraDetalle = "Cancelación de la Operacion : " + request.id_solicitud;
                        break;
                    default:
                        vConcepto = "CRD001"; vTipo = "O"; tipoTexto = "ABONO ORDINARIO";
                        bitacoraDetalle = "Abono Ordinario a la Operacion : " + request.id_solicitud;
                        break;
                }

                var sql = @"exec spCajas_CrdAbono @Operacion,
                                    @Abono ,
                                    @TipoDoc,
                                    @NumDoc ,
                                    @Concepto ,
                                    @Usuario ,
                                    @Caja ,
                                    @Apertura,
                                    @Recalcula,
                                    @CargoAnticipo ,
                                    @IntExtra ,
                                    @FechaPagoReal ";

                var parameters = new
                {
                    Operacion = request.id_solicitud,
                    Abono = request.totalCajs,
                    TipoDoc = request.tipoDoc,
                    NumDoc = vNumDoc.ToString(),
                    Concepto = vConcepto,
                    Usuario = request.mUsuario,
                    Caja = request.mCaja,
                    Apertura = request.mApertura,
                    Recalcula = request.chkRecalculaCuota ?? 0,
                    CargoAnticipo = request.datosAnticipo ?? 0m,
                    IntExtra = (vTipo == "E") ? (request.datosInteres ?? 0m) : 0m,
                    FechaPagoReal = request.FechaCancelacion
                };

                var result = DbHelper.ExecuteSingleQuery<dynamic>(_portalDB, codEmpresa, sql, parameters).Result;
                decimal pendiente = 0m;
                if (result != null && result?.Pendiente != null)
                    pendiente = Convert.ToDecimal(result.Pendiente);

                // VB6: el pendiente es solo una advertencia, el proceso continúa
                var advertencia = pendiente != 0m ? " - Quedó un monto pendiente de :" + pendiente.ToString("#,##0.00") + "." : string.Empty;

                Bitacora(codEmpresa, request.mUsuario ?? string.Empty, bitacoraDetalle);

                var vars = new CajasCrdAbonosStpVariables
                {
                    vTipoDoc = request.tipoDoc,
                    vNumDoc = vNumDoc,
                    vConcepto = vConcepto,
                    id_solicutud = (int?)request.id_solicitud,
                    vSaldoMes = saldoAnterior,
                    notas = vNotas,
                    FechaCancelacion = request.FechaCancelacion,
                    FechaCancelacionEnable = request.fechaCancelacionEnable ?? (tipoAbono != 0),
                    codCaja = request.mCaja,
                    codApertura = request.mApertura,
                    usuarioRegistro = request.mUsuario,
                    tiquete = request.mTiquete,
                    unidadCaja = request.mUnidad,
                    pCharRelleno = MCredito.fxCrdParametro(conn, "05"),
                    oficinaTitular = _mProGrx.sbSifParametrosInicializa(codEmpresa, request.mUsuario ?? string.Empty).Result?.GOficinaTitular ?? string.Empty,
                    enlace = 0
                };

                var doc = sbDocumentoAbono(codEmpresa, solicitud, vars);
                if (doc.Code != 0)
                    return doc;

                var detalleDoc = $" - {tipoTexto} aplicado, con : {request.tipoDoc} ...No.: {vNumDoc}{advertencia}";
                // Formato "mensaje?numDoc" (igual que CtP) para que Angular imprima el recibo
                return new ErrorDto { Code = 0, Description = detalleDoc + "\n - Desea Realizar Otra Transacción a Esta Operación ?" + vNumDoc };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("Error al aplicar el abono: " + ex.Message, -1);
            }
        }

        /// <summary>
        /// FxVerifica (VB6) - retorna mensaje concatenado. Si mensaje vacío => ok.
        /// </summary>
        private ErrorDto fxVerifica(int codEmpresa, SqlConnection conn, CajasCrdAbonoRequest request,
            CajasCrdAbonosStPDData solicitud, bool esRetencion, decimal saldoAnterior)
        {
            try
            {
                var mensaje = new System.Text.StringBuilder();

                void Agregar(string? m)
                {
                    if (!string.IsNullOrEmpty(m)) mensaje.Append(m).Append('\n');
                }

                Agregar(VerificaProceso(conn, solicitud.proceso, request.mCaja, request.mUsuario));
                Agregar(VerificaCongelamiento(codEmpresa, solicitud.cedula));
                Agregar(VerificaOperacion(request.id_solicitud ?? 0));
                Agregar(VerificaSaldoActual(esRetencion, saldoAnterior, request.saldoActual));
                Agregar(VerificaMontos(request, solicitud, esRetencion, saldoAnterior));

                if (_mCajas.fxCajasAperturaEstado(codEmpresa, request.mCaja ?? string.Empty, request.mApertura ?? 0) == "C")
                    Agregar($"- La apertura ..:{request.mApertura} de esta caja ha sido cerrada!");

                var texto = mensaje.ToString().Trim();
                return texto.Length > 0 ? DbHelper.ErrorResponse(texto, -1) : DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("Error en fxVerifica: " + ex.Message, -1);
            }
        }

        private static string? VerificaProceso(SqlConnection conn, string? proceso, string? caja, string? usuario)
        {
            if (proceso != "J") return null;

            var autorizada = conn.QueryFirstOrDefault<bool>(
                "select dbo.fxCajas_AbonoCbrJudAutorizada(@pCaja,@usuario) as Valor",
                new { pCaja = caja, usuario });

            return autorizada
                ? null
                : "- Esta CAJA no cuenta con permisos para realizar abonos a Créditos en Cobro Judicial, verifique...";
        }

        private string? VerificaCongelamiento(int codEmpresa, string cedula)
            => _mAfilicacion.fxgCongelamiento_Obtener(codEmpresa, cedula, "per_abono_cajas")
                ? "- Esta Persona se encuentra CONGELADA, verifique..."
                : null;

        private static string? VerificaOperacion(long operacion)
            => operacion <= 0 ? "- Número de Operación no es válido..." : null;

        private static string? VerificaSaldoActual(bool esRetencion, decimal saldoAnterior, decimal? saldoPantalla)
        {
            if (!saldoPantalla.HasValue)
                return "- No se proporcionó el saldo de la operación.";

            // Misma regla que fxCrdSaldoVerifica de VB6 (retención con saldo > 999 se acepta el de pantalla)
            var actual = esRetencion && saldoAnterior > 999m ? saldoPantalla.Value : saldoAnterior;
            return actual != saldoPantalla.Value
                ? "- Esta Operación ha sido modificada, actualice los datos nuevamente antes de realizar el abono..."
                : null;
        }

        private static string? VerificaMontos(CajasCrdAbonoRequest request, CajasCrdAbonosStPDData solicitud,
            bool esRetencion, decimal saldoAnterior)
        {
            var sb = new System.Text.StringBuilder();
            var amortiza = request.amortiza ?? 0m;

            if (!esRetencion)
            {
                // VB6: (txtDatosAmortiza + txtMoraAmortiza) > lblSaldo
                if (amortiza + (request.moraAmortiza ?? 0m) > saldoAnterior)
                    sb.Append("- La Amortización es mayor al Saldo Actual...\n");
            }
            else
            {
                var remanente = ((solicitud.cuota ?? 0m) * (solicitud.plazo ?? 0)) - (solicitud.amortiza ?? 0m);
                if (amortiza > remanente && (solicitud.plazo ?? 0) <= 900)
                    sb.Append("- La Amortización es mayor que el Remanente a Recaudar : " + remanente + "\n");
            }

            if ((request.compromiso ?? 0m) <= 0m)
                sb.Append("- El compromiso de pago no es un dato válido...verifique...!\n");

            if ((request.totalCajs ?? 0m) <= 0m)
                sb.Append("- Los valores Recibidos en Cajas no son válidos...verifique...!\n");

            // VB6: no se permiten abonos ordinarios parciales salvo para cuotas atrasadas
            if (sb.Length == 0 && (request.tipoAbono ?? 0) == 0)
            {
                var compromiso = request.compromiso ?? 0m;
                var total = request.totalCajs ?? 0m;
                if ((request.moraCuotas ?? 0) > 0)
                {
                    var soloMora = (request.moraAmortiza ?? 0m) + (request.moraCargos ?? 0m) + (request.moraIntereses ?? 0m);
                    if (compromiso < total && total > soloMora)
                        sb.Append("- Tiene que hacer una transaccion solo por la mora y luego otro extraordinario...!\n");
                }
                else if (compromiso > total)
                {
                    sb.Append("- Tiene que hacer una transaccion Cancelando el Compromiso...!\n");
                }
            }

            if (sb.Length == 0 && (request.tipoAbono ?? 0) == 1 && (request.diferencia ?? 0m) != 0m)
                sb.Append("- El Monto detallado en formas de pago no cubre el compromiso de pago. SOLUCION: Copie el Monto detallado en el monto del abono extraordinario...!\n");

            if (sb.Length == 0 && (request.tipoAbono ?? 0) == 2 && (request.diferencia ?? 0m) != 0m)
                sb.Append("- El Monto detallado en formas de pago no cubre el compromiso de cancelación...!\n");

            return sb.Length == 0 ? null : sb.ToString().TrimEnd('\n');
        }

        #endregion

        #region Bitácora + Documento (ControlDocumentosV2)

        public ErrorDto Bitacora(int codEmpresa, string usuario, string detalle)
        {
            return _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = "Registra - WEB",
                Modulo = 5
            });
        }

        public ErrorDto sbDocumentoAbono(int codEmpresa, CajasCrdAbonosStPDData solicitud, CajasCrdAbonosStpVariables variable)
        {
            try
            {
                decimal pTipoCambio = _mCajas.fxCajasTipoCambio(codEmpresa, 0, solicitud.divisa);
                variable.tipoCambio = pTipoCambio;

                if (!variable.vNumDoc.HasValue)
                    return DbHelper.ErrorResponse("Error al registrar el documento de abono: número de documento inválido.", -1);

                var idSolicitud = variable.id_solicutud;
                if (!idSolicitud.HasValue)
                    return DbHelper.ErrorResponse("Error al registrar el documento de abono: solicitud inválida.", -1);

                var docAfectacion = spCrdDocumentoAfectacionStP(codEmpresa, variable.vTipoDoc!, variable.vNumDoc.Value, "R");
                var cuentaOperacion = spCrdOperacionCtas(codEmpresa, idSolicitud.Value);

                var lineas = BuildLineas(docAfectacion, variable, solicitud);

                var doc = ControlDocumentosV2_RegistrarAsync(codEmpresa, solicitud, variable, docAfectacion, cuentaOperacion, lineas).GetAwaiter().GetResult();

                if (doc.Code == -1)
                    return DbHelper.ErrorResponse("Error al registrar el documento de abono: " + doc.Description, -1);

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("Error al registrar el documento de abono: " + ex.Message, -1);
            }
        }

        private CajasCrdAbonoAfectacionData spCrdDocumentoAfectacionStP(int codEmpresa, string vTipoDoc, long vNumDoc, string formato)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                return conn.QueryFirstOrDefault<CajasCrdAbonoAfectacionData>(
                    "exec spCrdDocumentoAfectacionStP @vTipoDoc, @vNumDoc, @Formato",
                    new { vTipoDoc, vNumDoc, Formato = formato }
                ) ?? new CajasCrdAbonoAfectacionData();
            }
            catch
            {
                return new CajasCrdAbonoAfectacionData();
            }
        }

        private CajasCrdAbonooperacionCtas spCrdOperacionCtas(int codEmpresa, long operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                return conn.QueryFirstOrDefault<CajasCrdAbonooperacionCtas>(
                    "exec spCrdOperacionCtas @Operacion",
                    new { Operacion = operacion }
                ) ?? new CajasCrdAbonooperacionCtas();
            }
            catch
            {
                return new CajasCrdAbonooperacionCtas();
            }
        }

        private static string[] BuildLineas(
            CajasCrdAbonoAfectacionData docAfectacion,
            CajasCrdAbonosStpVariables variable,
            CajasCrdAbonosStPDData solicitud)
        {
            var saldoAnterior = variable.vSaldoMes ?? 0m;
            var saldoActual = saldoAnterior - docAfectacion.Principal;
            var relleno = string.IsNullOrEmpty(variable.pCharRelleno) ? "0" : variable.pCharRelleno!;

            // VB6: SIFGlobal.fxStringRelleno(Format(valor, "Standard"), "I", pCharRelleno, 15)
            string Fill(string label, decimal value) =>
                $"{label} ..: " + MHaciendaDB.fxStringRelleno(value.ToString("#,##0.00"), "I", relleno, 15);

            bool vRetencion = solicitud.retencion == "S" || solicitud.poliza == "S";
            var opex = solicitud.opex == 1 ? "OPEX" : string.Empty;

            // Usamos 0..10 para insert (linea1..linea11)
            var l = new string[11];
            l[0] = Fill("Saldo Anterior   ", saldoAnterior);
            l[1] = Fill("Saldo Actual     ", saldoActual);
            l[2] = Fill("Interes Corriente", docAfectacion.IntCor);
            l[3] = Fill("Interes Atrasado ", docAfectacion.IntMor);
            l[4] = Fill("Amortización     ", docAfectacion.Principal);
            l[5] = Fill("Cargos Totales   ", docAfectacion.Cargos);
            l[6] = Fill("Pólizas          ", docAfectacion.Polizas);

            l[7] = $"Operacion/Línea   ..: Op.:{solicitud.id_solicitud} L.:{solicitud.codigo}-{opex}";
            l[8] = $"Descripción       ..: {solicitud.descripcion}";
            l[9] = $"Proc. Retencion   ..: {(vRetencion ? "SI" : "NO")}";
            l[10] = variable.FechaCancelacionEnable == true && variable.FechaCancelacion.HasValue
                ? $"Fecha Real Abono {variable.FechaCancelacion:dd/MM/yyyy}"
                : string.Empty;

            return l;
        }

        public async Task<ErrorDto> ControlDocumentosV2_RegistrarAsync(
            int codEmpresa,
            CajasCrdAbonosStPDData solicitud,
            CajasCrdAbonosStpVariables vars,
            CajasCrdAbonoAfectacionData afectacion,
            CajasCrdAbonooperacionCtas cuentaOperacion,
            string?[] lineas)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (vars.vNumDoc is null || string.IsNullOrWhiteSpace(vars.vTipoDoc))
                    throw new InvalidOperationException("vNumDoc y vTipoDoc son requeridos.");

                if (lineas == null || lineas.Length < 11)
                    throw new InvalidOperationException("Se requieren 11 líneas (linea1..linea11).");

                // VB: curIntC + curIntM + curAmortiza + curCargo (ojo: VB no sumó poliza al monto insert)
                var montoTransaccion = afectacion.IntCor + afectacion.IntMor + afectacion.Principal + afectacion.Cargos;

                using var tx = await conn.BeginTransactionAsync();

                const string sqlInsert = @"
                    INSERT INTO SIF_TRANSACCIONES
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
                        linea1,linea2,linea3,linea4,linea5,linea6,linea7,linea8,linea9,linea10,linea11,
                        detalle,
                        documento,
                        cod_caja,
                        cod_apertura
                    )
                    VALUES
                    (
                        @CodTransaccion,
                        @TipoDocumento,
                        dbo.MyGetdate(),
                        @Usuario,
                        @ClienteId,
                        @ClienteNombre,
                        @CodConcepto,
                        @Monto,
                        'P',
                        @Ref01,
                        @Ref02,
                        @Ref03,
                        @CodOficina,
                        @Linea1,@Linea2,@Linea3,@Linea4,@Linea5,@Linea6,@Linea7,@Linea8,@Linea9,@Linea10,@Linea11,
                        @Detalle,
                        @Documento,
                        @CodCaja,
                        @CodApertura
                    );";

                await conn.ExecuteAsync(
                    sqlInsert,
                    new
                    {
                        CodTransaccion = vars.vNumDoc.Value.ToString(),
                        TipoDocumento = vars.vTipoDoc,
                        Usuario = vars.usuarioRegistro,
                        ClienteId = (solicitud.cedula ?? string.Empty).Trim(),
                        ClienteNombre = (solicitud.nombre ?? string.Empty).Trim(),
                        CodConcepto = vars.vConcepto,
                        Monto = montoTransaccion,
                        Ref01 = solicitud.id_solicitud,
                        Ref02 = solicitud.codigo,
                        Ref03 = vars.vNumDoc,
                        CodOficina = vars.oficinaTitular,
                        Linea1 = lineas[0] ?? "",
                        Linea2 = lineas[1] ?? "",
                        Linea3 = lineas[2] ?? "",
                        Linea4 = lineas[3] ?? "",
                        Linea5 = lineas[4] ?? "",
                        Linea6 = lineas[5] ?? "",
                        Linea7 = lineas[6] ?? "",
                        Linea8 = lineas[7] ?? "",
                        Linea9 = lineas[8] ?? "",
                        Linea10 = lineas[9] ?? "",
                        Linea11 = lineas[10] ?? "",
                        Detalle = vars.notas ?? vars.detalle ?? "",
                        Documento = vars.vNumDoc,
                        CodCaja = vars.codCaja,
                        CodApertura = vars.codApertura
                    },
                    tx
                );

                async Task ExecAsientoAsync(decimal monto, string? cuenta)
                {
                    if (monto <= 0) return;
                    if (string.IsNullOrWhiteSpace(cuenta)) return;

                    const string sp = "spSIFDocsAsiento";

                    var p = new DynamicParameters();
                    p.Add("@TipoDocumento", vars.vTipoDoc, DbType.String);
                    p.Add("@CodTransaccion", vars.vNumDoc.Value.ToString(), DbType.String);
                    p.Add("@Monto", monto * (vars.tipoCambio ?? 1m), DbType.Decimal);
                    p.Add("@Tipo", "C", DbType.String);
                    p.Add("@Divisa", string.IsNullOrWhiteSpace(cuentaOperacion.cod_divisa) ? solicitud.divisa : cuentaOperacion.cod_divisa, DbType.String);
                    p.Add("@TipoCambio", vars.tipoCambio ?? 1m, DbType.Decimal);
                    p.Add("@Enlace", vars.enlace ?? 0, DbType.Int32);
                    p.Add("@CodUnidad", cuentaOperacion.cod_unidad, DbType.String);
                    p.Add("@CodCentroCosto", cuentaOperacion.cod_centro_costo ?? "", DbType.String);
                    p.Add("@Cuenta", cuenta, DbType.String);
                    p.Add("@IdSolicitud", solicitud.id_solicitud, DbType.Int64);
                    p.Add("@Codigo", solicitud.codigo, DbType.String);
                    p.Add("@Documento", vars.vNumDoc, DbType.String);

                    await conn.ExecuteAsync(sp, p, tx, commandType: CommandType.StoredProcedure);
                }

                await ExecAsientoAsync(afectacion.IntCor, cuentaOperacion.CtaIntC);
                await ExecAsientoAsync(afectacion.IntMor, cuentaOperacion.CtaIntM);
                await ExecAsientoAsync(afectacion.Cargos, cuentaOperacion.CtaCargos);
                await ExecAsientoAsync(afectacion.Principal, cuentaOperacion.CtaAmortiza);

                // VB: condición incluye poliza, pero sp usa los mismos parámetros
                if ((afectacion.IntCor + afectacion.IntMor + afectacion.Principal + afectacion.Cargos + afectacion.Polizas) > 0)
                {
                    const string spPagos = "spCajas_DesglocePagosDocFinal";

                    var pPago = new DynamicParameters();
                    pPago.Add("@Caja", vars.codCaja ?? "", DbType.String);
                    pPago.Add("@Apertura", vars.codApertura ?? 0, DbType.Int32);
                    pPago.Add("@Tiquete", vars.tiquete ?? "", DbType.String);
                    pPago.Add("@Usuario", vars.usuarioRegistro ?? "", DbType.String);
                    pPago.Add("@TipoDocumento", vars.vTipoDoc ?? "", DbType.String);
                    pPago.Add("@CodTransaccion", vars.vNumDoc.Value.ToString(), DbType.String);
                    pPago.Add("@Unidad", vars.unidadCaja ?? "", DbType.String);
                    pPago.Add("@IdSolicitud", solicitud.id_solicitud, DbType.Int64);
                    pPago.Add("@Codigo", solicitud.codigo, DbType.String);

                    await conn.ExecuteAsync(spPagos, pPago, tx, commandType: CommandType.StoredProcedure);
                }

                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        #endregion

        #region Helpers expuestos a Angular (si decides llamarlos directo)

        public ErrorDto<decimal> fxFechaProcesoSiguiente(int codEmpresa, decimal pProceso)
        {
            return new ErrorDto<decimal>()
            {
                Code = 0,
                Description = "Ok",
                Result = _mCobro.fxFechaProcesoSiguiente(codEmpresa, pProceso)
            };
        }

        public ErrorDto<decimal> fxCalcula_Cuota(int CodEmpresa, decimal monto, int plazo, object interes, string? frecuencia = "M")
        {
            return new ErrorDto<decimal>()
            {
                Code = 0,
                Description = "Ok",
                Result = MCobroDb.fxCalcula_Cuota(monto, plazo, interes, frecuencia)
            };
        }

        #endregion
    }
}
