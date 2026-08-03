using System.Text.Json.Serialization;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSeguimientoTramitesFormalizacionDeductoraContextoRequest
    {
        public string codigo { get; set; } = string.Empty;
        [JsonRequired]
        public long deductora_id { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionDeductoraContextoData
    {
        public string frecuencia_id { get; set; } = "M";
        public List<CrSeguimientoTramitesOpcionItem> frecuencias { get; set; } = new();
        public decimal primer_deduccion { get; set; }
        public int primer_deduccion_anio { get; set; }
        public int primer_deduccion_mes { get; set; }
        public int primer_deduccion_quincena { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionRecursoDisponibleRequest
    {
        public string recurso { get; set; } = string.Empty;
        [JsonRequired]
        public DateTime fecha_desembolso { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionRecursoDisponibleData
    {
        public decimal disponible { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionPrevalidacionRequest
    {
        [JsonRequired]
        public int operacion { get; set; }
        [JsonRequired]
        public int banco_id { get; set; }
        public string emite_tipo { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pasos previos del VB6 que dependen de una ventana hija y por lo tanto se resuelven en Angular.
    /// </summary>
    public class CrSeguimientoTramitesFormalizacionPrevalidacionData
    {
        public int requisitos_pendientes { get; set; }
        public bool requiere_requisitos { get; set; }
        public string comprobante_ck { get; set; } = string.Empty;
        public bool requiere_documento_ck { get; set; }
        public bool banco_sin_documento_ck { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionAplicarRequest
    {
        [JsonRequired]
        public int operacion { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string estado_solicitud { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string documento_referido { get; set; } = string.Empty;
        public string emite_tipo { get; set; } = string.Empty;
        public string cuenta_bancaria { get; set; } = string.Empty;
        [JsonRequired]
        public int banco_id { get; set; }
        [JsonRequired]
        public decimal monto { get; set; }
        [JsonRequired]
        public decimal tasa { get; set; }
        [JsonRequired]
        public int plazo { get; set; }
        [JsonRequired]
        public decimal tasa_facial { get; set; }
        [JsonRequired]
        public long pagare { get; set; }
        [JsonRequired]
        public bool ind_deduce_planilla { get; set; }
        [JsonRequired]
        public bool ind_enviar_tesoreria { get; set; }
        [JsonRequired]
        public bool ind_primera_cuota { get; set; }
        [JsonRequired]
        public long deductora_id { get; set; }
        [JsonRequired]
        public int primer_deduccion_anio { get; set; }
        [JsonRequired]
        public int primer_deduccion_mes { get; set; }
        [JsonRequired]
        public int primer_deduccion_quincena { get; set; }
        [JsonRequired]
        public DateTime fecha_formalizacion { get; set; }
        [JsonRequired]
        public DateTime fecha_desembolso { get; set; }
        public DateTime? fecha_vence { get; set; }
        public string recurso { get; set; } = string.Empty;
        public string fnd_garantia { get; set; } = string.Empty;
        [JsonRequired]
        public int fnd_contrato { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesFormalizacionAnularRequest
    {
        [JsonRequired]
        public int operacion { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string estado_solicitud { get; set; } = string.Empty;
        [JsonRequired]
        public decimal monto { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesFormalizacionResult
    {
        public int operacion { get; set; }
        public bool aplicado { get; set; }
        public bool imprime_boleta_ck { get; set; }
        public bool imprime_recibo_anulacion { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesFormalizacionFechasRequest
    {
        [JsonRequired]
        public int operacion { get; set; }
        public DateTime? fecha_formalizacion { get; set; }
        public DateTime? fecha_desembolso { get; set; }
        public string estado_solicitud { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesFormalizacionIndicadoresRequest
    {
        [JsonRequired]
        public int operacion { get; set; }
        [JsonRequired]
        public bool ind_primera_cuota { get; set; }
        [JsonRequired]
        public bool ind_traslado_salario { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionResumenRequest
    {
        [JsonRequired]
        public int operacion { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string estado_solicitud { get; set; } = string.Empty;
        [JsonRequired]
        public bool ind_deduce_planilla { get; set; }
        [JsonRequired]
        public bool ind_primera_cuota { get; set; }
        [JsonRequired]
        public int primer_deduccion_anio { get; set; }
        [JsonRequired]
        public int primer_deduccion_mes { get; set; }
        [JsonRequired]
        public int primer_deduccion_quincena { get; set; }
        [JsonRequired]
        public DateTime fecha_desembolso { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionResumenLinea
    {
        public string descripcion { get; set; } = string.Empty;
        public decimal valor { get; set; }
        public string nota { get; set; } = string.Empty;
        /// <summary>R resta, S suma, T total, D separador. Equivale al color del lsw del VB6.</summary>
        public string tipo { get; set; } = "R";
    }

    public class CrSeguimientoTramitesFormalizacionResumenData
    {
        public List<CrSeguimientoTramitesFormalizacionResumenLinea> lineas { get; set; } = new();
        public decimal monto_a_girar { get; set; }
        public decimal retenido { get; set; }
        public decimal monto_giros { get; set; }
        public DateTime? fecha_calculo { get; set; }
        public int dias_interes { get; set; }
    }

    internal sealed class CrSeguimientoTramitesFormalizacionResumenRaw
    {
        public decimal refundiciones { get; set; }
        public decimal iva_refundicion { get; set; }
        public decimal desembolsos { get; set; }
        public decimal retenciones { get; set; }
        public decimal iva_retenciones { get; set; }
        public decimal desembolsosret { get; set; }
        public decimal cargos { get; set; }
        public decimal int_devolucion { get; set; }
        public decimal condonacion { get; set; }
    }

    internal sealed class CrSeguimientoTramitesFormalizacionResumenOperacionRaw
    {
        public decimal montoapr { get; set; }
        public decimal cuota { get; set; }
        public string garantia { get; set; } = string.Empty;
        public string convenio { get; set; } = string.Empty;
        public string primer_cuota { get; set; } = string.Empty;
        public int dia_pago { get; set; }
    }

    internal sealed class CrSeguimientoTramitesFormalizacionInteresDiasRaw
    {
        public DateTime? fecha_calculo_int { get; set; }
        public DateTime? fecha_inicio_calculo { get; set; }
        public string convenio { get; set; } = string.Empty;
        public string retencion { get; set; } = string.Empty;
        public string poliza { get; set; } = string.Empty;
        public decimal montoapr { get; set; }
        public decimal tasa_int { get; set; }
    }

    internal sealed class CrSeguimientoTramitesFormalizacionAplicarRaw
    {
        public int pasaformalizacion { get; set; }
        public int boletack { get; set; }
        public string errormsj { get; set; } = string.Empty;
    }

    internal sealed class CrSeguimientoTramitesFormalizacionAnulacionRaw
    {
        public int nivel { get; set; }
        public int meses_diferencia { get; set; }
        public int desembolsos_tesoreria { get; set; }
        public int retencion { get; set; }
        public DateTime? fechaforp { get; set; }
    }

    internal sealed class CrSeguimientoTramitesFormalizacionExcedenteRaw
    {
        public decimal @base { get; set; }
    }

    internal sealed class CrSeguimientoTramitesFormalizacionMovimientosRaw
    {
        public int movimientos { get; set; }
        public int morosidad { get; set; }
        public int refundiciones_posteriores { get; set; }
        public int mora_refundiciones_posteriores { get; set; }
        public int refundiciones_movidas { get; set; }
    }
}
