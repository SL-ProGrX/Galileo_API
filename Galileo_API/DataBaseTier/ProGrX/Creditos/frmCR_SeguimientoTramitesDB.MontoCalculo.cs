using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        /// <summary>Pasadas de acercamiento del ciclo de imgMonto_Click del VB6.</summary>
        private const int CrSeguimientoTramitesAcercamientos = 5;

        /// <summary>
        /// Recalcula el monto del crédito a partir de los rebajos, intereses, póliza y
        /// cargos, conservando el ciclo de acercamiento de imgMonto_Click del VB6.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionMontoCalculadoData>
            Cr_SeguimientoTramites_Recepcion_Monto_Calcular(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionMontoCalcularRequest request)
        {
            var result = new CrSeguimientoTramitesRecepcionMontoCalculadoData();
            string? mensaje = Cr_SeguimientoTramites_Monto_Request_Validar(request);
            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                return DbHelper.CreateErrorResponse(mensaje, -2, result);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                CrSeguimientoTramitesMontoCalculoRaw? datos =
                    Cr_SeguimientoTramites_Monto_Datos_Obtener(conn, request);

                if (datos is null)
                {
                    return DbHelper.CreateErrorResponse("No existe esta Solicitud", -2, result);
                }

                return DbHelper.CreateOkResponse(
                    Cr_SeguimientoTramites_Monto_Ciclo_Ejecutar(
                        codEmpresa,
                        conn,
                        request,
                        datos));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
        }

        private static string? Cr_SeguimientoTramites_Monto_Request_Validar(
            CrSeguimientoTramitesRecepcionMontoCalcularRequest? request)
        {
            if (request is null || request.operacion <= 0)
            {
                return "Debe indicar la operación.";
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return "Debe indicar la línea de crédito.";
            }

            // El VB6 sale sin recalcular cuando el combo está en Monto del Crédito.
            string tipo = request.tipo_calculo.Trim().ToUpperInvariant();
            return tipo is CrSeguimientoTramitesTipoCalculoMonto.MontoGirar
                or CrSeguimientoTramitesTipoCalculoMonto.GiroCero
                    ? null
                    : "Debe indicar un tipo de cálculo válido.";
        }

        private CrSeguimientoTramitesRecepcionMontoCalculadoData
            Cr_SeguimientoTramites_Monto_Ciclo_Ejecutar(
                int codEmpresa,
                IDbConnection conn,
                CrSeguimientoTramitesRecepcionMontoCalcularRequest request,
                CrSeguimientoTramitesMontoCalculoRaw datos)
        {
            CrSeguimientoTramitesMontoCalculoContexto contexto =
                Cr_SeguimientoTramites_Monto_Contexto_Crear(codEmpresa, request, datos);

            bool montoGirar = string.Equals(
                request.tipo_calculo.Trim(),
                CrSeguimientoTramitesTipoCalculoMonto.MontoGirar,
                StringComparison.OrdinalIgnoreCase);

            decimal rebajos = datos.rebajos;
            decimal montoBase = montoGirar ? request.monto : 0m;
            decimal monto = rebajos + montoBase;
            decimal cargos = 0m;
            decimal primeraCuota = request.ind_primera_cuota ? datos.cuota : 0m;
            decimal intereses = 0m;
            decimal poliza = 0m;
            decimal cuota = datos.cuota;

            for (int paso = 1; paso <= CrSeguimientoTramitesAcercamientos; paso++)
            {
                intereses = Cr_SeguimientoTramites_Monto_Intereses_Calcular(
                    codEmpresa,
                    request,
                    contexto,
                    monto);

                poliza = Cr_SeguimientoTramites_Monto_Poliza_Calcular(
                    contexto.cobra_tasa_formaliza,
                    datos,
                    monto);

                monto = decimal.Round(poliza, 2)
                    + primeraCuota
                    + decimal.Round(intereses, 2)
                    + rebajos
                    + decimal.Round(cargos, 2)
                    + montoBase;

                decimal cargosPrevios = cargos;
                cargos = Cr_SeguimientoTramites_Monto_Cargos_Calcular(
                    conn,
                    request,
                    decimal.Round(monto, 2));

                monto += cargos - cargosPrevios;
                cuota = MCobroDb.fxCalcula_Cuota(
                    decimal.Round(monto, 2),
                    request.plazo,
                    request.tasa,
                    Cr_SeguimientoTramites_Monto_Frecuencia_Normalizar(request.frecuencia_pago));
            }

            return new CrSeguimientoTramitesRecepcionMontoCalculadoData
            {
                monto = decimal.Round(monto, 2),
                cuota = cuota,
                rebajos = rebajos,
                intereses = decimal.Round(intereses, 2),
                poliza = decimal.Round(poliza, 2),
                cargos = decimal.Round(cargos, 2),
                primer_cuota = primeraCuota
            };
        }

        private CrSeguimientoTramitesMontoCalculoContexto
            Cr_SeguimientoTramites_Monto_Contexto_Crear(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionMontoCalcularRequest request,
                CrSeguimientoTramitesMontoCalculoRaw datos)
        {
            return new CrSeguimientoTramitesMontoCalculoContexto
            {
                cobra_tasa_formaliza = _mCobroDb.fxCobraTasaFormaliza(
                    codEmpresa,
                    request.codigo,
                    request.destino),
                credito_excedente = _mCobroDb.fxCreditoExcedente(codEmpresa, request.codigo),
                formalizada = string.Equals(
                    request.estado_solicitud.Trim(),
                    "F",
                    StringComparison.OrdinalIgnoreCase),
                dias_interes = Cr_SeguimientoTramites_Monto_Dias_Calcular(request, datos),
                primer_deduccion = Cr_SeguimientoTramites_Formalizacion_PrimerDeduccion_Componer(
                    request.primer_deduccion_anio,
                    request.primer_deduccion_mes,
                    request.primer_deduccion_quincena),
                dia_pago = request.ind_deduce_planilla
                    ? CrSeguimientoTramitesDiaPagoPlanilla
                    : datos.dia_pago
            };
        }

        /// <summary>
        /// Días de interés del VB6. Cuando la operación no tiene fecha de cálculo se usa
        /// el último instante del mes de la primer deducción, por lo que la conversión a
        /// entero del VB6 termina sumando el día en curso.
        /// </summary>
        private static int Cr_SeguimientoTramites_Monto_Dias_Calcular(
            CrSeguimientoTramitesRecepcionMontoCalcularRequest request,
            CrSeguimientoTramitesMontoCalculoRaw datos)
        {
            DateTime fechaInicio = datos.fecha_inicio_calculo ?? request.fecha_desembolso;
            DateTime fechaCorte = datos.fecha_calculo_int
                ?? new DateTime(
                    request.primer_deduccion_anio,
                    Math.Clamp(request.primer_deduccion_mes, 1, 12),
                    1,
                    23,
                    59,
                    59);

            if (fechaCorte < fechaInicio)
            {
                return 0;
            }

            return (int)Math.Round(
                (fechaCorte - fechaInicio).TotalDays,
                MidpointRounding.ToEven);
        }

        private decimal Cr_SeguimientoTramites_Monto_Intereses_Calcular(
            int codEmpresa,
            CrSeguimientoTramitesRecepcionMontoCalcularRequest request,
            CrSeguimientoTramitesMontoCalculoContexto contexto,
            decimal monto)
        {
            if (!contexto.cobra_tasa_formaliza)
            {
                return 0m;
            }

            if (contexto.formalizada)
            {
                return contexto.credito_excedente
                    ? _mCobroDb.fxInteresesHastaFormalizar(
                        codEmpresa,
                        request.operacion,
                        request.codigo,
                        request.fecha_desembolso,
                        monto)
                    : monto * request.tasa / 36000m * contexto.dias_interes;
            }

            if (request.ind_primera_cuota)
            {
                return MCobroDb.fxInteresesDiasPrimerCuota(
                    request.fecha_desembolso,
                    monto,
                    request.tasa);
            }

            return _mCobroDb.fxInteresesHastaFormalizar(
                codEmpresa,
                request.operacion,
                request.codigo,
                request.fecha_desembolso,
                monto,
                contexto.primer_deduccion,
                contexto.dia_pago);
        }

        private static decimal Cr_SeguimientoTramites_Monto_Poliza_Calcular(
            bool cobraTasaFormaliza,
            CrSeguimientoTramitesMontoCalculoRaw datos,
            decimal monto)
        {
            if (!cobraTasaFormaliza)
            {
                return 0m;
            }

            bool aplica = !string.Equals(
                    datos.garantia.Trim(),
                    "H",
                    StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                    datos.convenio.Trim(),
                    "N",
                    StringComparison.OrdinalIgnoreCase);

            return aplica ? monto / 1000000m * datos.poliza_base : 0m;
        }

        private static decimal Cr_SeguimientoTramites_Monto_Cargos_Calcular(
            IDbConnection conn,
            CrSeguimientoTramitesRecepcionMontoCalcularRequest request,
            decimal monto)
        {
            return conn.QueryFirstOrDefault<decimal?>(
                """
                select isnull(
                    dbo.fxCrd_Operacion_Cargos_Calcula(@Operacion, @Codigo, @Monto), 0);
                """,
                new
                {
                    Operacion = request.operacion,
                    Codigo = request.codigo.Trim(),
                    Monto = monto
                }) ?? 0m;
        }

        private static string Cr_SeguimientoTramites_Monto_Frecuencia_Normalizar(
            string? frecuencia)
        {
            string normalizada = (frecuencia ?? string.Empty).Trim().ToUpperInvariant();
            return string.IsNullOrEmpty(normalizada) ? "M" : normalizada;
        }

        private static CrSeguimientoTramitesMontoCalculoRaw?
            Cr_SeguimientoTramites_Monto_Datos_Obtener(
                IDbConnection conn,
                CrSeguimientoTramitesRecepcionMontoCalcularRequest request)
        {
            const string sql = """
                select
                    rtrim(isnull(R.Garantia, '')) as garantia,
                    isnull(R.cuota, 0) as cuota,
                    isnull(R.int, 0) as tasa_int,
                    rtrim(isnull(C.convenio, 'N')) as convenio,
                    R.FECHA_CALCULO_INT as fecha_calculo_int,
                    R.FECHA_INICIO_CALCULO as fecha_inicio_calculo,
                    isnull(dbo.fxCRDPoliticaPago(dbo.MyGetdate()), 32) as dia_pago,
                    isnull(dbo.fxCrdSGTMontoDeducciones(R.id_solicitud), 0)
                        - isnull(dbo.fxCrdCargosOperacion(R.id_solicitud), 0) as rebajos,
                    (select isnull(max(CR_PSDMNT), 0) from par_ahcr) as poliza_base
                from reg_creditos R
                inner join Catalogo C on R.codigo = C.codigo
                where R.id_solicitud = @Operacion;
                """;

            return conn.QueryFirstOrDefault<CrSeguimientoTramitesMontoCalculoRaw>(
                sql,
                new { Operacion = request.operacion });
        }
    }
}
