using System.Text.Json.Serialization;

namespace Galileo.Models.FSL
{
    public sealed class FslTablaCoberturaDto
    {
        public int linea { get; set; } = 0;
        public int mes_inicio { get; set; } = 0;
        public int mes_corte { get; set; } = 0;
        public decimal cobertura { get; set; } = 0;
    }

    public sealed class FslTablaCoberturasFiltros
    {
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string filtro { get; set; } = string.Empty;
        public string tipo { get; set; } = "F";
        public string sort_field { get; set; } = "mes_inicio";
        public int sort_order { get; set; } = 1;
    }

    public sealed class FslTablaCoberturaGuardarRequest
    {
        [JsonRequired]
        public int linea { get; set; } = 0;

        [JsonRequired]
        public int mes_inicio { get; set; } = 0;

        [JsonRequired]
        public int mes_corte { get; set; } = 0;

        [JsonRequired]
        public decimal cobertura { get; set; } = 0;

        public string tipo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}