using System.Data;
using System.Globalization;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        private const int CrSeguimientoTramitesDiaPagoPlanilla = 32;

        /// <summary>
        /// Arma el resumen de la operación conservando el orden y los cálculos de
        /// sbResumenOperacion del formulario VB6.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionResumenData>
            Cr_SeguimientoTramites_Formalizacion_Resumen_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionResumenRequest request)
        {
            var result = new CrSeguimientoTramitesFormalizacionResumenData();
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse("Debe indicar la operación.", -2, result);
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la línea de crédito.",
                    -2,
                    result);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                CrSeguimientoTramitesFormalizacionResumenOperacionRaw? operacion =
                    Cr_SeguimientoTramites_Formalizacion_Resumen_Operacion_Obtener(
                        conn,
                        request.operacion);

                if (operacion is null)
                {
                    return DbHelper.CreateErrorResponse("No existe esta Solicitud", -2, result);
                }

                CrSeguimientoTramitesFormalizacionResumenRaw totales =
                    Cr_SeguimientoTramites_Formalizacion_Resumen_Totales_Obtener(
                        conn,
                        request.operacion);

                return DbHelper.CreateOkResponse(
                    Cr_SeguimientoTramites_Formalizacion_Resumen_Construir(
                        codEmpresa,
                        request,
                        operacion,
                        totales));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Arma el request del resumen a partir de la solicitud de formalización,
        /// tal como el VB6 reutiliza los controles del formulario al llamar sbResumenOperacion.
        /// </summary>
        private static CrSeguimientoTramitesFormalizacionResumenRequest
            Cr_SeguimientoTramites_Formalizacion_Resumen_Request_Crear(
                CrSeguimientoTramitesFormalizacionAplicarRequest request)
        {
            return new CrSeguimientoTramitesFormalizacionResumenRequest
            {
                operacion = request.operacion,
                codigo = request.codigo,
                destino = request.destino,
                estado_solicitud = request.estado_solicitud,
                ind_deduce_planilla = request.ind_deduce_planilla,
                ind_primera_cuota = request.ind_primera_cuota,
                primer_deduccion_anio = request.primer_deduccion_anio,
                primer_deduccion_mes = request.primer_deduccion_mes,
                primer_deduccion_quincena = request.primer_deduccion_quincena,
                fecha_desembolso = request.fecha_desembolso
            };
        }

        private CrSeguimientoTramitesFormalizacionResumenData
            Cr_SeguimientoTramites_Formalizacion_Resumen_Construir(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionResumenRequest request,
                CrSeguimientoTramitesFormalizacionResumenOperacionRaw operacion,
                CrSeguimientoTramitesFormalizacionResumenRaw totales)
        {
            int diaPago = request.ind_deduce_planilla
                ? CrSeguimientoTramitesDiaPagoPlanilla
                : operacion.dia_pago;

            decimal primerDeduccion = Cr_SeguimientoTramites_Formalizacion_PrimerDeduccion_Componer(
                request.primer_deduccion_anio,
                request.primer_deduccion_mes,
                request.primer_deduccion_quincena);

            DateTime fechaCalculo = _seguimientoDb.fxFechaCalculo(
                codEmpresa,
                request.codigo,
                primerDeduccion,
                diaPago);

            bool cobraTasa = _mCobroDb.fxCobraTasaFormaliza(
                codEmpresa,
                request.codigo,
                request.destino);

            decimal interes = Cr_SeguimientoTramites_Formalizacion_Interes_Calcular(
                codEmpresa,
                request,
                cobraTasa,
                primerDeduccion,
                diaPago);

            decimal primeraCuota = request.ind_primera_cuota ? operacion.cuota : 0m;
            decimal poliza = Cr_SeguimientoTramites_Formalizacion_Poliza_Calcular(
                codEmpresa,
                request.codigo,
                cobraTasa,
                operacion);

            int diasInteres = cobraTasa
                ? (fechaCalculo.Date - request.fecha_desembolso.Date).Days + 1
                : 0;

            var data = new CrSeguimientoTramitesFormalizacionResumenData
            {
                lineas = Cr_SeguimientoTramites_Formalizacion_Resumen_Lineas_Crear(
                    operacion,
                    totales,
                    new CrSeguimientoTramitesFormalizacionResumenCalculo
                    {
                        interes = interes,
                        primera_cuota = primeraCuota,
                        poliza = poliza,
                        nota_interes = cobraTasa
                            ? string.Create(
                                CultureInfo.InvariantCulture,
                                $"({diasInteres}) {fechaCalculo:dd/MM/yyyy}")
                            : string.Empty
                    }),
                retenido = totales.desembolsosret + totales.cargos,
                fecha_calculo = fechaCalculo,
                dias_interes = diasInteres
            };

            data.monto_a_girar = data.lineas
                .Where(linea => linea.tipo is "R" or "S" or "B")
                .Sum(linea => linea.tipo == "R" ? -linea.valor : linea.valor);

            data.monto_giros = Cr_SeguimientoTramites_Formalizacion_MontoGiros_Calcular(
                totales,
                primeraCuota,
                data.retenido);

            return data;
        }

        /// <summary>
        /// Reproduce curGiros de fxVerificaFormalizacion del VB6, que se calcula sobre los
        /// índices 10, 8 y 3 del ListView de sbResumenOperacion:
        /// Cargos Adicionales - Primer Cuota + IVA Refundiciones - (DesembolsosRet + Cargos).
        /// La fórmula se conserva idéntica al legacy por decisión funcional, aunque el término
        /// de Cargos se cancele contra el retenido.
        /// </summary>
        internal static decimal Cr_SeguimientoTramites_Formalizacion_MontoGiros_Calcular(
            CrSeguimientoTramitesFormalizacionResumenRaw totales,
            decimal primeraCuota,
            decimal retenido)
        {
            return totales.cargos - primeraCuota + totales.iva_refundicion - retenido;
        }

        private static List<CrSeguimientoTramitesFormalizacionResumenLinea>
            Cr_SeguimientoTramites_Formalizacion_Resumen_Lineas_Crear(
                CrSeguimientoTramitesFormalizacionResumenOperacionRaw operacion,
                CrSeguimientoTramitesFormalizacionResumenRaw totales,
                CrSeguimientoTramitesFormalizacionResumenCalculo calculo)
        {
            Func<string, decimal, string, string, CrSeguimientoTramitesFormalizacionResumenLinea>
                crear = Cr_SeguimientoTramites_Formalizacion_Resumen_Linea_Crear;

            return
            [
                crear("-> Monto Aprobado", operacion.montoapr, "B", string.Empty),
                crear("(-) Refundiciones CRD", totales.refundiciones, "R", string.Empty),
                crear("(-) IVA Refundiciones CRD", totales.iva_refundicion, "R", string.Empty),
                crear("(-) Desembolsos y Rebajos", totales.desembolsos, "R", string.Empty),
                crear("(-) Refund.Retenciones", totales.retenciones, "R", string.Empty),
                crear("(-) IVA Refund.Retenciones", totales.iva_retenciones, "R", string.Empty),
                crear("(-) Dias de Interes", calculo.interes, "R", calculo.nota_interes),
                crear("(-) Primer Cuota", calculo.primera_cuota, "R", string.Empty),
                crear("(-) P.S.D.", calculo.poliza, "R", string.Empty),
                crear("(-) Cargos Adicionales", totales.cargos, "R", string.Empty),
                crear("(+) Dev. Int. Refundiciones", totales.int_devolucion, "S", string.Empty),
                crear("(+) Condonación", totales.condonacion, "S", string.Empty)
            ];
        }

        private static CrSeguimientoTramitesFormalizacionResumenLinea
            Cr_SeguimientoTramites_Formalizacion_Resumen_Linea_Crear(
                string descripcion,
                decimal valor,
                string tipo,
                string nota)
        {
            return new CrSeguimientoTramitesFormalizacionResumenLinea
            {
                descripcion = descripcion,
                valor = valor,
                tipo = tipo,
                nota = nota
            };
        }

        private decimal Cr_SeguimientoTramites_Formalizacion_Interes_Calcular(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionResumenRequest request,
            bool cobraTasa,
            decimal primerDeduccion,
            int diaPago)
        {
            if (!cobraTasa)
            {
                return 0m;
            }

            bool formalizada = string.Equals(
                request.estado_solicitud.Trim(),
                "F",
                StringComparison.OrdinalIgnoreCase);

            if (formalizada && !_mCobroDb.fxCreditoExcedente(codEmpresa, request.codigo))
            {
                return Cr_SeguimientoTramites_Formalizacion_InteresDias_Calcular(
                    codEmpresa,
                    request.operacion);
            }

            return _mCobroDb.fxInteresesHastaFormalizar(
                codEmpresa,
                request.operacion,
                request.codigo,
                request.fecha_desembolso,
                null,
                primerDeduccion,
                diaPago);
        }

        /// <summary>
        /// Reproduce fxInteresDiasX del formulario VB6 para operaciones ya formalizadas.
        /// </summary>
        private decimal Cr_SeguimientoTramites_Formalizacion_InteresDias_Calcular(
            int codEmpresa,
            int operacion)
        {
            const string sql = """
                select
                    R.FECHA_CALCULO_INT as fecha_calculo_int,
                    isnull(R.FECHA_INICIO_CALCULO, R.FECHA_CALCULO_INT) as fecha_inicio_calculo,
                    rtrim(isnull(C.convenio, 'N')) as convenio,
                    rtrim(isnull(C.retencion, 'N')) as retencion,
                    rtrim(isnull(C.poliza, 'N')) as poliza,
                    isnull(R.montoapr, 0) as montoapr,
                    isnull(R.int, 0) as tasa_int
                from reg_creditos R
                inner join Catalogo C on R.codigo = C.codigo
                where R.id_solicitud = @Operacion;
                """;

            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn => conn.QueryFirstOrDefault<CrSeguimientoTramitesFormalizacionInteresDiasRaw>(
                    sql,
                    new { Operacion = operacion }));

            CrSeguimientoTramitesFormalizacionInteresDiasRaw? raw = response.Result;
            if (response.Code != 0 || raw is null)
            {
                return 0m;
            }

            if (raw.convenio == "S" || raw.retencion == "S" || raw.poliza == "S")
            {
                return 0m;
            }

            DateTime? fechaCalculo = raw.fecha_calculo_int;
            DateTime? fechaInicio = raw.fecha_inicio_calculo;
            if (!fechaCalculo.HasValue || !fechaInicio.HasValue)
            {
                return 0m;
            }

            int dias = fechaCalculo.Value.Date < fechaInicio.Value.Date
                ? 0
                : (fechaCalculo.Value.Date - fechaInicio.Value.Date).Days + 1;

            return decimal.Round(raw.montoapr * raw.tasa_int / 36000m * dias, 2);
        }

        private decimal Cr_SeguimientoTramites_Formalizacion_Poliza_Calcular(
            int codEmpresa,
            string codigo,
            bool cobraTasa,
            CrSeguimientoTramitesFormalizacionResumenOperacionRaw operacion)
        {
            if (!cobraTasa)
            {
                return 0m;
            }

            if (string.Equals(operacion.garantia, "H", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(operacion.convenio, "N", StringComparison.OrdinalIgnoreCase))
            {
                return 0m;
            }

            return _mCobroDb.fxCuotaPolizaVida(codEmpresa, operacion.montoapr, codigo);
        }

        private static CrSeguimientoTramitesFormalizacionResumenOperacionRaw?
            Cr_SeguimientoTramites_Formalizacion_Resumen_Operacion_Obtener(
                IDbConnection conn,
                int operacion)
        {
            const string sql = """
                select
                    isnull(R.montoapr, 0) as montoapr,
                    isnull(R.cuota, 0) as cuota,
                    rtrim(isnull(R.Garantia, '')) as garantia,
                    rtrim(isnull(R.Primer_Cuota, '')) as primer_cuota,
                    rtrim(isnull(C.convenio, 'N')) as convenio,
                    isnull(dbo.fxCRDPoliticaPago(dbo.MyGetdate()), 32) as dia_pago
                from reg_creditos R
                inner join Catalogo C on R.codigo = C.codigo
                where R.id_solicitud = @Operacion;
                """;

            return conn.QueryFirstOrDefault<CrSeguimientoTramitesFormalizacionResumenOperacionRaw>(
                sql,
                new { Operacion = operacion });
        }

        private static CrSeguimientoTramitesFormalizacionResumenRaw
            Cr_SeguimientoTramites_Formalizacion_Resumen_Totales_Obtener(
                IDbConnection conn,
                int operacion)
        {
            return conn.QueryFirstOrDefault<CrSeguimientoTramitesFormalizacionResumenRaw>(
                "exec spCrdSGTResumen @Operacion;",
                new { Operacion = operacion })
                ?? new CrSeguimientoTramitesFormalizacionResumenRaw();
        }

        private sealed class CrSeguimientoTramitesFormalizacionResumenCalculo
        {
            public decimal interes { get; init; }
            public decimal primera_cuota { get; init; }
            public decimal poliza { get; init; }
            public string nota_interes { get; init; } = string.Empty;
        }
    }
}
