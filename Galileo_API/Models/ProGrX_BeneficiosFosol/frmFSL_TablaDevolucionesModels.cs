using System.Text.Json.Serialization;

namespace Galileo.Models.FSL
{
    public sealed class FslTablaDevolucionDto
    {
        public int cod_devolucion { get; set; } = 0;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string garantia { get; set; } = string.Empty;
        public string garantia_descripcion { get; set; } = string.Empty;
        public string _base { get; set; } = string.Empty;
        public string base_descripcion { get; set; } = string.Empty;
        public decimal porcentaje { get; set; } = 0;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public sealed class FslTablaDevolucionesFiltros
    {
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string filtro { get; set; } = string.Empty;
        public string sort_field { get; set; } = "fecha_inicio";
        public int sort_order { get; set; } = 1;
    }

    public sealed class FslTablaDevolucionGuardarRequest
    {
        [JsonRequired]
        public int cod_devolucion { get; set; } = 0;

        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string garantia { get; set; } = string.Empty;
        public string _base { get; set; } = string.Empty;

        [JsonRequired]
        public decimal porcentaje { get; set; } = 0;

        public string usuario { get; set; } = string.Empty;
    }
}