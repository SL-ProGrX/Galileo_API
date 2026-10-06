using System.Text.Json.Serialization;

namespace Galileo.Models.FSL
{
    public sealed class FslTablaTipoDto
    {
        public string codigo { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        [JsonRequired]
        public bool activa { get; set; }
    }

    public sealed class FslTablasTiposFiltros
    {
        public int pagina { get; set; } = 0;

        public int paginacion { get; set; } = 30;

        public string filtro { get; set; } =
            string.Empty;

        public string tipo { get; set; } = "G";

        public string sort_field { get; set; } =
            "codigo";

        public int sort_order { get; set; } = 1;
    }

    public sealed class FslTablaTipoGuardarRequest
    {
        public string tipo { get; set; } =
            string.Empty;

        public string codigo { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        [JsonRequired]
        public bool activa { get; set; }

        public string usuario { get; set; } =
            string.Empty;
    }
}