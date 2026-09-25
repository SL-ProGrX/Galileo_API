using System.Text.Json.Serialization;

namespace Galileo.Models.INV
{
    public class TransacQryParametros
    {
        public string? estado { get; set; } = string.Empty;
        public string? tipo { get; set; } = string.Empty;

        [JsonPropertyName("tipoFecha")]
        public string? tipo_fecha { get; set; } = "T";

        [JsonPropertyName("fechaInicio")]
        public string? fecha_inicio { get; set; } = string.Empty;

        [JsonPropertyName("fechaCorte")]
        public string? fecha_corte { get; set; } = string.Empty;

        [JsonPropertyName("tipoUsuario")]
        public string? tipo_usuario { get; set; } = "T";

        public string? usuario { get; set; } = string.Empty;
        public string? vfiltro { get; set; } = string.Empty;
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 500;
    }

    public class TransacQryDataList
    {
        public int total { get; set; } = 0;
        public List<TransacQryData> transacciones { get; set; } = new();
    }

    public class TransacQryData
    {
        public string boleta { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string genera_user { get; set; } = string.Empty;
        public DateTime? genera_fecha { get; set; }
        public string autoriza_user { get; set; } = string.Empty;
        public DateTime? autoriza_fecha { get; set; }
        public string procesa_user { get; set; } = string.Empty;
        public DateTime? procesa_fecha { get; set; }
    }
}