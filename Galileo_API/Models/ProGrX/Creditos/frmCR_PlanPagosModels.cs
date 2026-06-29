namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrPlanPagosObtenerDto
    {
        public CrPlanPagosHeaderDto header { get; set; } = new();
        public CrPlanPagosTotalesDto totales { get; set; } = new();
        public List<CrPlanPagosData> plan_pagos { get; set; } = new();
        public CrPlanPagosReporteDto reporte { get; set; } = new();
    }
    public class CrPlanPagosReporteDto
    {
        public DateTime fecha_servidor { get; set; }
        public string oficina { get; set; } = string.Empty;
    }
    public class CrPlanPagosHeaderDto
    {
        public int operacion { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal saldo { get; set; }
        public decimal cuota { get; set; }
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public decimal tasa_original { get; set; }
        public string dia_pago { get; set; } = string.Empty;
        public string base_calculo { get; set; } = string.Empty;
        public string factor_calculo { get; set; } = string.Empty;
        public decimal pri_deduc { get; set; }
        public string pri_deduc_format { get; set; } = string.Empty;
        public DateTime? fec_ult_cta { get; set; }
        public DateTime? activacion_fecha { get; set; }
        public DateTime? activacion_min { get; set; }
        public DateTime? activacion_max { get; set; }
    }

    public class CrPlanPagosTotalesDto
    {
        public int cuotas { get; set; }
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public int dias { get; set; }
        public decimal intereses { get; set; }
        public decimal cargos { get; set; }
        public int mora_dias { get; set; }
    }

    public class CrPlanPagosData
    {
        public int sep1 { get; set; }
        public decimal id_seq { get; set; }
        public int num_cuota { get; set; }
        public decimal fecha_proceso { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public DateTime? fecha_pago { get; set; }
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public decimal cuota { get; set; }
        public decimal iva { get; set; }
        public decimal cargos { get; set; }
        public decimal poliza { get; set; }
        public decimal intcor { get; set; }
        public decimal intmor { get; set; }
        public decimal principal { get; set; }
        public decimal saldo_anterior { get; set; }
        public decimal saldo_actual { get; set; }
        public int dias_calculo { get; set; }
        public string estado { get; set; } = string.Empty;
        public int mora_dias { get; set; }
        public DateTime? mov_fecha { get; set; }
        public decimal mov_monto { get; set; }
        public decimal mov_iva { get; set; }
        public decimal mov_cargos { get; set; }
        public decimal mov_poliza { get; set; }
        public decimal mov_intcor { get; set; }
        public decimal mov_intmor { get; set; }
        public decimal mov_principal { get; set; }
        public string usuario_caja { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string num_comprobante { get; set; } = string.Empty;
        public int sep2 { get; set; }
        public string concepto { get; set; } = string.Empty;
    }

    public class CrPlanPagosListaResult<T>
    {
        public int total { get; set; }
        public List<T> lista { get; set; } = new();
    }

    public class CrPlanPagosCargosData
    {
        public int linea { get; set; }
        public decimal id_seq { get; set; }
        public string detalle { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal mov_saldo { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public decimal abono { get; set; }
        public decimal pendiente { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrPlanPagosPolizasData
    {
        public decimal id_seq { get; set; }
        public string cod_poliza { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string num_poliza { get; set; } = string.Empty;
        public string num_contrato { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal mov_monto { get; set; }
        public decimal mov_saldo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string aseguradora_nombre { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
    }

    public class CrPlanPagosDocumentosData
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string documento_desc { get; set; } = string.Empty;
        public string cod_transaccion { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string concepto_desc { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal mov_monto { get; set; }
        public decimal mov_intcor { get; set; }
        public decimal mov_intmor { get; set; }
        public decimal mov_cargos { get; set; }
        public decimal mov_polizas { get; set; }
        public decimal mov_principal { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class CrPlanPagosValoresData
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_transaccion { get; set; } = string.Empty;
        public string forma_pago_desc { get; set; } = string.Empty;
        public string num_referencia { get; set; } = string.Empty;
        public decimal monto_doc { get; set; }
        public decimal monto { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; }
        public string saldo_favor_id { get; set; } = string.Empty;
        public DateTime? registra_fecha { get; set; }
        public string registra_usuario { get; set; } = string.Empty;
        public string referencias { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
    }

    public class CrPlanPagosAjustesData
    {
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string movimiento { get; set; } = string.Empty;
        public string movimiento_desc { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class CrPlanPagosActivarRequest
    {
        public int? operacion { get; set; }
        public int? plazo { get; set; }
        public DateTime? fecha_activacion { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrPlanPagosRevisarRequest
    {
        public int? operacion { get; set; }
        public string usuario { get; set; } = string.Empty;
        public decimal? cuota { get; set; }
        public decimal? saldo { get; set; }
        public decimal? tasa { get; set; }
        public string factor_calculo { get; set; } = string.Empty;
        public bool? ajusta_cuota { get; set; }
        public decimal? cuota_manual { get; set; }
        public int? plazo_ext { get; set; }
        public bool? cuota_derivada { get; set; }
        public bool? plazo_aumenta_auto { get; set; }
    }

    public class CrPlanPagosEmailRequest
    {
        public int? operacion { get; set; }
        public string usuario { get; set; } = string.Empty;
    }
}