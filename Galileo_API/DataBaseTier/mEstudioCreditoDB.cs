using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models;
using System.Globalization;

namespace Galileo_API.DataBaseTier
{
    public class MEstudioCreditoDb
    {
        private readonly PortalDB _portalDB;

        public MEstudioCreditoDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Convierte el código del tipo de documento en su descripción
        /// y viceversa.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public static string MEstudioCredito_TipoDocumento(
            string? tipo)
        {
            return tipo?.Trim() switch
            {
                "CK" => "Cheque",
                "TE" => "Transferencia",
                "EF" or "RE" => "Efectivo",
                "ND" => "Nota Debito",
                "NC" => "Nota Credito",
                "OT" => "Otro...",
                "CD" => "Ctrl Desembolsos",
                "CP" => "Proveedor",
                "RC" => "Retiro en Caja",
                "FD" => "Fondo Transitorio",
                "TS" => "Transferencia SINPE",
                "Cheque" => "CK",
                "Transferencia" => "TE",
                "Efectivo" => "EF",
                "Nota Debito" => "ND",
                "Nota Credito" => "NC",
                "Otro..." => "OT",
                "Ctrl Desembolsos" => "CD",
                "Proveedor" => "CP",
                "Retiro en Caja" => "RC",
                "Fondo Transitorio" => "FD",
                "Transferencia SINPE" => "TS",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Obtiene la fecha de proceso siguiente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="proceso"></param>
        /// <returns></returns>
        public ErrorDto<decimal?>
            MEstudioCredito_FechaProcesoSiguiente(
                int codEmpresa,
                decimal proceso)
        {
            const string sql = @"
                select dbo.fxSIFPrmProcesoSig(@Proceso)
                    as resultado;";

            return DbHelper.ExecuteSingleQuery<decimal?>(
                _portalDB,
                codEmpresa,
                sql,
                proceso,
                new
                {
                    Proceso = proceso
                });
        }

        /// <summary>
        /// Obtiene la fecha de proceso anterior.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="proceso"></param>
        /// <returns></returns>
        public ErrorDto<decimal?>
            MEstudioCredito_FechaProcesoAnterior(
                int codEmpresa,
                decimal proceso)
        {
            const string sql = @"
                select dbo.fxSIFPrmProcesoAnt(@Proceso)
                    as resultado;";

            return DbHelper.ExecuteSingleQuery<decimal?>(
                _portalDB,
                codEmpresa,
                sql,
                proceso,
                new
                {
                    Proceso = proceso
                });
        }

        /// <summary>
        /// Obtiene la tabla utilizada para calcular el impuesto
        /// sobre la renta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<MEstudioCreditoRenta>>
            MEstudioCredito_RentaTablaObtener(int codEmpresa)
        {
            const string sql = @"
                select
                    desde,
                    hasta,
                    porcentaje / 100.0 as porcentaje
                from CRD_PREA_TABLA_IMPUESTO
                order by desde;";

            return DbHelper.ExecuteListQuery<MEstudioCreditoRenta>(
                _portalDB,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Calcula el impuesto sobre la renta para el salario indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="salario"></param>
        /// <returns></returns>
        public ErrorDto<decimal?> MEstudioCredito_RentaCalcular(
            int codEmpresa,
            decimal salario)
        {
            const string sql = @"
                select dbo.fxCRDPreaCalculaRenta(@Salario)
                    as resultado;";

            var respuesta = DbHelper.ExecuteSingleQuery<decimal?>(
                _portalDB,
                codEmpresa,
                sql,
                0,
                new
                {
                    Salario = salario
                });

            if (respuesta.Code == 0 &&
                respuesta.Result.HasValue)
            {
                respuesta.Result = Math.Round(
                    respuesta.Result.Value,
                    2,
                    MidpointRounding.ToEven);
            }

            return respuesta;
        }

        /// <summary>
        /// Obtiene la tasa básica pasiva vigente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<decimal?>
            MEstudioCredito_TasaBasicaPasivaObtener(
                int codEmpresa)
        {
            const string sql = @"
                select top 1
                    isnull(
                        try_convert(decimal(18, 6), valor),
                        0
                    ) as resultado
                from CRD_PARAMETROS
                where cod_parametro = '07';";

            return DbHelper.ExecuteSingleQuery<decimal?>(
                _portalDB,
                codEmpresa,
                sql,
                0);
        }

        /// <summary>
        /// Obtiene la fecha de ingreso utilizada para el cálculo
        /// de antigüedad.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<DateTime?>
            MEstudioCredito_FechaIngresoObtener(
                int codEmpresa,
                long operacion)
        {
            const string sql = @"
                select coalesce(
                    (
                        select top 1
                            case
                                when S.estadoactual = 'S'
                                    then S.fechaingreso
                                else GetDate()
                            end
                        from socios S
                        inner join reg_creditos R
                            on S.cedula = R.cedula
                        where R.id_solicitud = @Operacion
                    ),
                    GetDate()
                ) as fecha_ingreso;";

            return DbHelper.ExecuteSingleQuery<DateTime?>(
                _portalDB,
                codEmpresa,
                sql,
                default,
                new
                {
                    Operacion = operacion
                });
        }

        /// <summary>
        /// Calcula la antigüedad desde la fecha indicada utilizando
        /// la fecha del servidor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="fechaIngreso"></param>
        /// <returns></returns>
        public ErrorDto<string> MEstudioCredito_MemoriaCalcular(
            int codEmpresa,
            DateTime fechaIngreso)
        {
            const string sql = @"
                select GetDate() as fecha_servidor;";

            var consultaFecha =
                DbHelper.ExecuteSingleQuery<DateTime?>(
                    _portalDB,
                    codEmpresa,
                    sql);

            if (consultaFecha.Code != 0 ||
                !consultaFecha.Result.HasValue)
            {
                return DbHelper.CreateErrorResponse(
                    consultaFecha.Description ??
                    "No fue posible obtener la fecha del servidor.",
                    consultaFecha.Code.GetValueOrDefault(-1),
                    string.Empty);
            }

            int dias = (
                consultaFecha.Result.Value.Date -
                fechaIngreso.Date).Days;

            int anios = 0;
            int meses = 0;

            while (dias > 365)
            {
                anios++;
                dias -= 365;
            }

            while (dias > 30)
            {
                meses++;
                dias -= 30;
            }

            string descripcion = ConstruirDescripcionAntiguedad(
                anios,
                meses,
                dias);

            return DbHelper.CreateOkResponse(descripcion);
        }

        /// <summary>
        /// Obtiene la fecha de cálculo de créditos.
        /// </summary>
        public ErrorDto<DateTime?>
            MEstudioCredito_FechaCalculoObtener(
                int codEmpresa)
        {
            const string sql = @"
                select top 1
                    coalesce(
                        cr_fecha_calculo,
                        GetDate()
                    ) as fecha_calculo
                from par_ahcr;";

            return DbHelper.ExecuteSingleQuery<DateTime?>(
                _portalDB,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Calcula los intereses acumulados hasta la fecha
        /// de formalización.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="fechaFormalizacion"></param>
        /// <returns></returns>
        public ErrorDto<decimal?>
            MEstudioCredito_InteresesHastaFormalizar(
                int codEmpresa,
                long operacion,
                DateTime fechaFormalizacion)
        {
            const string sql = @"
                select top 1
                    R.int as interes,
                    R.interesv as interes_variable,
                    R.montosol as monto_solicitado,
                    coalesce(
                        (
                            select top 1
                                cr_fecha_calculo
                            from par_ahcr
                        ),
                        GetDate()
                    ) as fecha_calculo
                from reg_creditos R
                where R.id_solicitud = @Operacion;";

            var consulta =
                DbHelper.ExecuteSingleQuery<
                    MEstudioCreditoInteresesData>(
                    _portalDB,
                    codEmpresa,
                    sql,
                    default,
                    new
                    {
                        Operacion = operacion
                    });

            if (consulta.Code != 0)
            {
                return DbHelper.CreateErrorResponse<decimal?>(
                    consulta.Description ??
                    "No fue posible consultar la operación.",
                    consulta.Code.GetValueOrDefault(-1));
            }

            MEstudioCreditoInteresesData? datos =
                consulta.Result;

            if (datos is null)
            {
                return DbHelper.CreateErrorResponse<decimal?>(
                    "No se encontró la operación indicada.",
                    -2);
            }

            if (datos.fecha_calculo.Date <
                fechaFormalizacion.Date)
            {
                return DbHelper.CreateOkResponse<decimal?>(0);
            }

            int numeroDias = Math.Abs(
                (
                    datos.fecha_calculo.Date -
                    fechaFormalizacion.Date
                ).Days) + 1;

            decimal tasa =
                datos.interes_variable ??
                datos.interes;

            decimal intereses =
                (tasa / 36000m) *
                datos.monto_solicitado *
                numeroDias;

            return DbHelper.CreateOkResponse<decimal?>(
                intereses);
        }

        /// <summary>
        /// Obtiene y transforma los parámetros generales
        /// del estudio de crédito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<MEstudioCreditoParametros>
            MEstudioCredito_ParametrosInicializar(
                int codEmpresa)
        {
            const string sql = @"
                select
                    trim(cod_parametro) as cod_parametro,
                    coalesce(
                        convert(varchar(100), valor),
                        ''
                    ) as valor
                from CRD_PREA_PARAMETROS;";

            var consulta =
                DbHelper.ExecuteListQuery<
                    MEstudioCreditoParametroData>(
                    _portalDB,
                    codEmpresa,
                    sql);

            if (consulta.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    consulta.Description ??
                    "No fue posible cargar los parámetros.",
                    consulta.Code.GetValueOrDefault(-1),
                    new MEstudioCreditoParametros());
            }

            var valores = (consulta.Result ?? [])
                .GroupBy(item => item.cod_parametro)
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => grupo.Last().valor,
                    StringComparer.OrdinalIgnoreCase);

            var parametros = new MEstudioCreditoParametros
            {
                edad_maxima_hombre =
                    ObtenerEntero(valores, "01"),

                edad_maxima_mujer =
                    ObtenerEntero(valores, "02"),

                restriccion_garantia_ahorros =
                    ObtenerTexto(valores, "03"),

                restriccion_garantia_fiduciaria =
                    ObtenerTexto(valores, "04"),

                restriccion_garantia_hipotecaria =
                    ObtenerTexto(valores, "05"),

                restriccion_creditos_sin_garantia =
                    ObtenerTexto(valores, "06"),

                porcentaje_ccss =
                    ObtenerTexto(valores, "07"),

                porcentaje_asociacion_solidarista =
                    ObtenerDecimal(valores, "08"),

                porcentaje_frap_fap =
                    ObtenerEntero(valores, "09"),

                porcentaje_liquidez_libre =
                    ObtenerLong(valores, "10"),

                consecutivo_expedientes =
                    ObtenerTexto(valores, "11"),

                vencimiento_dias_no_ejecutados =
                    ObtenerEntero(valores, "12"),

                porcentaje_poliza_saldo_deudor =
                    ObtenerDouble(valores, "13"),

                aplica_fianzas_endeudamiento =
                    ObtenerTexto(valores, "14"),

                aplicar_fianzas_monto_girar_mayor =
                    ObtenerDouble(valores, "15"),

                salario_minimo_inembargable =
                    ObtenerDecimal(valores, "17"),

                poliza_factor_vida =
                    ObtenerDouble(valores, "80"),

                poliza_factor_incendio =
                    ObtenerDouble(valores, "81"),

                poliza_factor_desempleo =
                    ObtenerDouble(valores, "82"),

                poliza_factor_prenda =
                    ObtenerDouble(valores, "83")
            };

            parametros.salario_normativo =
                parametros.salario_minimo_inembargable +
                ObtenerDecimal(valores, "22");

            return DbHelper.CreateOkResponse(parametros);
        }

        /// <summary>
        /// Valida si el estado permite modificar el preanálisis.
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        public static bool
            MEstudioCredito_EstadoPreanalisisValidar(
                string? estado)
        {
            string estadoNormalizado = (
                estado ?? string.Empty
            ).Trim().ToUpperInvariant();

            return estadoNormalizado is not
                ("A" or "D" or "B");
        }

        private static string ConstruirDescripcionAntiguedad(
            int anios,
            int meses,
            int dias)
        {
            var partes = new List<string>();

            if (anios > 0)
            {
                partes.Add($"{anios} año(s)");
            }

            if (meses > 0)
            {
                partes.Add($"{meses} mes(es)");
            }

            string resultado = string.Join(", ", partes);

            if (dias <= 0)
            {
                return resultado;
            }

            if (!string.IsNullOrEmpty(resultado))
            {
                resultado += " con ";
            }

            return $"{resultado}{dias} dia(s) ";
        }

        private static string ObtenerTexto(
            IReadOnlyDictionary<string, string> valores,
            string codigo)
        {
            return valores.TryGetValue(
                codigo,
                out string? valor)
                    ? valor.Trim()
                    : string.Empty;
        }

        private static int ObtenerEntero(
            IReadOnlyDictionary<string, string> valores,
            string codigo)
        {
            string valor = ObtenerTexto(valores, codigo);

            if (int.TryParse(
                    valor,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out int resultado))
            {
                return resultado;
            }

            return int.TryParse(
                valor,
                NumberStyles.Any,
                CultureInfo.CurrentCulture,
                out resultado)
                    ? resultado
                    : 0;
        }

        private static long ObtenerLong(
            IReadOnlyDictionary<string, string> valores,
            string codigo)
        {
            string valor = ObtenerTexto(valores, codigo);

            if (long.TryParse(
                    valor,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out long resultado))
            {
                return resultado;
            }

            return long.TryParse(
                valor,
                NumberStyles.Any,
                CultureInfo.CurrentCulture,
                out resultado)
                    ? resultado
                    : 0;
        }

        private static decimal ObtenerDecimal(
            IReadOnlyDictionary<string, string> valores,
            string codigo)
        {
            string valor = ObtenerTexto(valores, codigo);

            if (decimal.TryParse(
                    valor,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal resultado))
            {
                return resultado;
            }

            return decimal.TryParse(
                valor,
                NumberStyles.Any,
                CultureInfo.CurrentCulture,
                out resultado)
                    ? resultado
                    : 0;
        }

        private static double ObtenerDouble(
            IReadOnlyDictionary<string, string> valores,
            string codigo)
        {
            string valor = ObtenerTexto(valores, codigo);

            if (double.TryParse(
                    valor,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double resultado))
            {
                return resultado;
            }

            return double.TryParse(
                valor,
                NumberStyles.Any,
                CultureInfo.CurrentCulture,
                out resultado)
                    ? resultado
                    : 0;
        }
    }
}