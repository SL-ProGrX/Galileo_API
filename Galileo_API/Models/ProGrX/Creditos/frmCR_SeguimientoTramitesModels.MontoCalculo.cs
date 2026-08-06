using System.Text.Json.Serialization;

namespace Galileo_API.Models.ProGrX.Creditos
{
    /// <summary>
    /// Opciones del combo cboCalculoAdd del formulario VB6.
    /// </summary>
    public static class CrSeguimientoTramitesTipoCalculoMonto
    {
        /// <summary>Monto del Crédito: el VB6 no recalcula nada.</summary>
        public const string MontoCredito = "C";

        /// <summary>Monto a Girar: parte del monto capturado y le suma los rebajos.</summary>
        public const string MontoGirar = "G";

        /// <summary>Giro en Cero: el monto resultante cubre solo los rebajos.</summary>
        public const string GiroCero = "Z";
    }

    public class CrSeguimientoTramitesRecepcionMontoCalcularRequest
    {
        [JsonRequired]
        public int operacion { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string estado_solicitud { get; set; } = string.Empty;
        public string tipo_calculo { get; set; } = string.Empty;
        [JsonRequired]
        public decimal monto { get; set; }
        [JsonRequired]
        public decimal tasa { get; set; }
        [JsonRequired]
        public int plazo { get; set; }
        [JsonRequired]
        public bool ind_primera_cuota { get; set; }
        [JsonRequired]
        public bool ind_deduce_planilla { get; set; }
        [JsonRequired]
        public DateTime fecha_desembolso { get; set; }
        [JsonRequired]
        public int primer_deduccion_anio { get; set; }
        [JsonRequired]
        public int primer_deduccion_mes { get; set; }
        [JsonRequired]
        public int primer_deduccion_quincena { get; set; }
        public string frecuencia_pago { get; set; } = "M";
    }

    public class CrSeguimientoTramitesRecepcionMontoCalculadoData
    {
        public decimal monto { get; set; }
        public decimal cuota { get; set; }
        public decimal rebajos { get; set; }
        public decimal intereses { get; set; }
        public decimal poliza { get; set; }
        public decimal cargos { get; set; }
        public decimal primer_cuota { get; set; }
    }

    internal sealed class CrSeguimientoTramitesMontoCalculoRaw
    {
        public string garantia { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public decimal tasa_int { get; set; }
        public string convenio { get; set; } = string.Empty;
        public DateTime? fecha_calculo_int { get; set; }
        public DateTime? fecha_inicio_calculo { get; set; }
        public int dia_pago { get; set; }
        public decimal rebajos { get; set; }
        public decimal poliza_base { get; set; }
    }

    /// <summary>
    /// Valores que se mantienen fijos durante las cinco pasadas de acercamiento del VB6.
    /// </summary>
    internal sealed class CrSeguimientoTramitesMontoCalculoContexto
    {
        public bool cobra_tasa_formaliza { get; init; }
        public bool credito_excedente { get; init; }
        public bool formalizada { get; init; }
        public int dias_interes { get; init; }
        public decimal primer_deduccion { get; init; }
        public int dia_pago { get; init; }
    }
}
