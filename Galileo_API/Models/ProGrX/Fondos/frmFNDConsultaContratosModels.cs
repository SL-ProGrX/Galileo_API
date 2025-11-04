using System.Text.Json.Serialization;

namespace PgxAPI.Models.ProGrX.Fondos
{
    public class FndConsultaContratosData
    {
        [JsonPropertyName("nombre")]
        public string? nombre { get; set; }

        [JsonPropertyName("descripcion")]
        public string? descripcion { get; set; }

        [JsonPropertyName("dplan")]
        public string? dplan { get; set; }

        [JsonPropertyName("cod_operadora")]
        public int cod_operadora { get; set; }

        [JsonPropertyName("cod_plan")]
        public string? cod_plan { get; set; }

        [JsonPropertyName("cod_contrato")]
        public int cod_contrato { get; set; }

        [JsonPropertyName("estado")]
        public string? estado { get; set; }

        [JsonPropertyName("liq_fecha")]
        public DateTime? liq_fecha { get; set; }

        [JsonPropertyName("fecha_inicio")]
        public DateTime fecha_inicio { get; set; }

        [JsonPropertyName("monto")]
        public decimal monto { get; set; }

        [JsonPropertyName("plazo")]
        public int plazo { get; set; }

        [JsonPropertyName("renueva")]
        public string? renueva { get; set; } // "S" o "N"

        [JsonPropertyName("inc_anual")]
        public decimal inc_anual { get; set; }

        [JsonPropertyName("inc_tipo")]
        public string? inc_tipo { get; set; }

        [JsonPropertyName("aportes")]
        public decimal aportes { get; set; }

        [JsonPropertyName("rendimiento")]
        public decimal rendimiento { get; set; }

        [JsonPropertyName("operacion")]
        public int operacion { get; set; }

        [JsonPropertyName("monto_transito")]
        public decimal monto_transito { get; set; }
    }

    public class FndConsultaSubContratosData
    {
        [JsonPropertyName("idx")]
        public int idx { get; set; }

        [JsonPropertyName("cod_operadora")]
        public int cod_operadora { get; set; }

        [JsonPropertyName("cod_plan")]
        public string? cod_plan { get; set; }

        [JsonPropertyName("cod_contrato")]
        public int cod_contrato { get; set; }

        [JsonPropertyName("cod_beneficiario")]
        public int cod_beneficiario { get; set; }

        [JsonPropertyName("estado")]
        public string? estado { get; set; }

        [JsonPropertyName("cuota")]
        public decimal cuota { get; set; }

        [JsonPropertyName("aportes")]
        public decimal aportes { get; set; }

        [JsonPropertyName("rendimiento")]
        public decimal rendimiento { get; set; }

        [JsonPropertyName("cedula")]
        public string? cedula { get; set; }

        [JsonPropertyName("nombre")]
        public string? nombre { get; set; }

        [JsonPropertyName("fechanac")]
        public DateTime? fechanac { get; set; } // puede ser NULL

        [JsonPropertyName("telefono1")]
        public string? telefono1 { get; set; }

        [JsonPropertyName("telefono2")]
        public string? telefono2 { get; set; }

        [JsonPropertyName("email")]
        public string? email { get; set; }

        [JsonPropertyName("direccion")]
        public string? direccion { get; set; }

        [JsonPropertyName("apto_postal")]
        public string? apto_postal { get; set; }

        [JsonPropertyName("notas")]
        public string? notas { get; set; }

        [JsonPropertyName("parentesco")]
        public string? parentesco { get; set; }
    }

    public class FndConsultaLiquidacionesData
    {
        [JsonPropertyName("cod_plan")]
        public string? cod_plan { get; set; }

        [JsonPropertyName("descripcion")]
        public string? descripcion { get; set; }

        [JsonPropertyName("cod_contrato")]
        public int cod_contrato { get; set; }

        [JsonPropertyName("consec")]
        public int consec { get; set; }

        [JsonPropertyName("fecha")]
        public DateTime fecha { get; set; }

        [JsonPropertyName("usuario")]
        public string? usuario { get; set; }

        [JsonPropertyName("monto")]
        public decimal monto { get; set; }

        [JsonPropertyName("traspaso_tesoreria")]
        public DateTime? traspaso_tesoreria { get; set; } // puede ser null

        [JsonPropertyName("traspaso_usuario")]
        public string? traspaso_usuario { get; set; }

        [JsonPropertyName("solicitud_tesoreria")]
        public int solicitud_tesoreria { get; set; }

        [JsonPropertyName("estado")]
        public string? estado { get; set; }
    }

    public class FndConsultaMovimientosData
    {
        [JsonPropertyName("cod_fnd_detalle")]
        public int cod_fnd_detalle { get; set; }

        [JsonPropertyName("monto")]
        public decimal monto { get; set; }

        [JsonPropertyName("fecha_proceso")]
        public int fecha_proceso { get; set; } // Ej: 201310 (AAAAMM) → puede mantenerse como int o string

        [JsonPropertyName("fecha")]
        public DateTime fecha { get; set; }

        [JsonPropertyName("descripcion_mov")]
        public string? descripcion_mov { get; set; } // campo intermedio (NOTA DE CRÉDITO)

        [JsonPropertyName("ncon")]
        public string? ncon { get; set; }

        [JsonPropertyName("fecha_acredita")]
        public DateTime fecha_acredita { get; set; }

        [JsonPropertyName("cod_contrato")]
        public int cod_contrato { get; set; }

        [JsonPropertyName("cod_plan")]
        public string? cod_plan { get; set; }

        [JsonPropertyName("descripcion")]
        public string? descripcion { get; set; }
    }

    public class FndConsultaMovimientosParams
    {
        public string? plan { get; set; }
        public string? contrato { get; set; }
        public Nullable<DateTime> fechaInicio { get; set; }
        public Nullable<DateTime> fechaCorte { get; set; }
        public bool chkTodas { get; set; }
    }
}