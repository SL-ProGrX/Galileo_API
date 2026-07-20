namespace Galileo_API.Models
{
    public enum MEstudioCreditoTab
    {
        Datos,
        Calculo,
        Observaciones
    }

    public enum MEstudioCreditoFormula
    {
        SalarioDevengado,
        SalarioReal,
        ExtrasFijas,
        DevengadoDelMes,
        CargasSociales,
        PorcentajeSobreSalario,
        Deducciones,
        CreditosCancelados,
        CreditoPorCobrar,
        SalarioLiquido,
        Refundiciones,
        Desembolsos,
        TotalLiquido,
        Fianzas,
        LiquidezSinFianzas,
        LiquidezPorcentajeSinFianzas,
        LiquidezConFianza,
        LiquidezPorcentajeConFianza,
        MontoGirar,
        AplicarTodas,
        PolizaSaldoDeudor
    }

    public sealed class MEstudioCreditoPreAnalisis
    {
        public string expediente { get; set; } = string.Empty;
        public string tag1 { get; set; } = string.Empty;
        public string tag2 { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;
        public string socio { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string estado_v2 { get; set; } = string.Empty;
        public string estado_v2_desc { get; set; } = string.Empty;
        public bool editable { get; set; }
        public string fecha_ingreso { get; set; } = string.Empty;
        public string edad_justifica { get; set; } = string.Empty;
        public int edad_ind { get; set; }
        public int edad_n_cuotas { get; set; }
    }

    public sealed class MEstudioCreditoOperacion
    {
        public long operacion { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string estado_solicitud { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal monto_aprobado { get; set; }
        public long operacion_consulta { get; set; }
        public bool valida { get; set; }
        public string documento { get; set; } = string.Empty;
        public string ts { get; set; } = string.Empty;
        public string ventana { get; set; } = string.Empty;
        public DateTime fecha_desembolso { get; set; }
        public decimal tasa_pts_bono { get; set; }
        public int plazo_bono { get; set; }
        public decimal primera_deduccion { get; set; }
        public int dia_pago { get; set; }
        public string garantia_tipo { get; set; } = string.Empty;
        public long garantia_id { get; set; }
        public string garantia_parametro { get; set; } = string.Empty;
        public string expediente { get; set; } = string.Empty;
    }

    public sealed class MEstudioCreditoParametros
    {
        public int edad_maxima_hombre { get; set; }
        public int edad_maxima_mujer { get; set; }
        public string restriccion_garantia_ahorros { get; set; } = string.Empty;
        public string restriccion_garantia_fiduciaria { get; set; } = string.Empty;
        public string restriccion_garantia_hipotecaria { get; set; } = string.Empty;
        public string restriccion_creditos_sin_garantia { get; set; } = string.Empty;
        public string porcentaje_ccss { get; set; } = string.Empty;
        public decimal porcentaje_asociacion_solidarista { get; set; }
        public int porcentaje_frap_fap { get; set; }
        public long porcentaje_liquidez_libre { get; set; }
        public string consecutivo_expedientes { get; set; } = string.Empty;
        public int vencimiento_dias_no_ejecutados { get; set; }
        public string socio { get; set; } = string.Empty;
        public double porcentaje_poliza_saldo_deudor { get; set; }
        public string aplica_fianzas_endeudamiento { get; set; } = string.Empty;
        public double aplicar_fianzas_monto_girar_mayor { get; set; }
        public decimal salario_minimo_inembargable { get; set; }
        public decimal salario_normativo { get; set; }
        public double poliza_factor_vida { get; set; }
        public double poliza_factor_incendio { get; set; }
        public double poliza_factor_prenda { get; set; }
        public double poliza_factor_desempleo { get; set; }
    }

    public sealed class MEstudioCreditoRenta
    {
        public MEstudioCreditoRenta(
            decimal desde,
            decimal hasta,
            decimal porcentaje)
        {
            this.desde = desde;
            this.hasta = hasta;
            this.porcentaje = porcentaje;
        }

        public decimal desde { get; }

        public decimal hasta { get; }

        public decimal porcentaje { get; }
    }

    public sealed class MEstudioCreditoInteresesData
    {
        public MEstudioCreditoInteresesData(
            decimal interes,
            decimal? interes_variable,
            decimal monto_solicitado,
            DateTime fecha_calculo)
        {
            this.interes = interes;
            this.interes_variable = interes_variable;
            this.monto_solicitado = monto_solicitado;
            this.fecha_calculo = fecha_calculo;
        }

        public decimal interes { get; }

        public decimal? interes_variable { get; }

        public decimal monto_solicitado { get; }

        public DateTime fecha_calculo { get; }
    }

    public sealed class MEstudioCreditoParametroData
    {
        public MEstudioCreditoParametroData(
            string cod_parametro,
            string valor)
        {
            this.cod_parametro = cod_parametro;
            this.valor = valor;
        }

        public string cod_parametro { get; }

        public string valor { get; }
    }
}