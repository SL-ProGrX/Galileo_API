using System.Text.Json.Serialization;

namespace Galileo.Models.FSL
{
    public sealed class FslTipoAplicacionPlanDto
    {
        public string cod_plan { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public string tipo_desembolso { get; set; } =
            string.Empty;

        public string tipo_desembolso_descripcion { get; set; } =
            string.Empty;

        [JsonRequired]
        public bool activo { get; set; }
    }

    public sealed class FslTipoAplicacionCausaDto
    {
        public string cod_causa { get; set; } =
            string.Empty;

        public string cod_plan { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public string monto_base { get; set; } =
            string.Empty;

        public string monto_base_descripcion { get; set; } =
            string.Empty;

        public string tipo_tabla { get; set; } =
            string.Empty;

        public string tipo_tabla_descripcion { get; set; } =
            string.Empty;

        [JsonRequired]
        public bool activa { get; set; }
    }

    public sealed class FslTipoAplicacionFiltros
    {
        public int pagina { get; set; } = 0;

        public int paginacion { get; set; } = 30;

        public string filtro { get; set; } =
            string.Empty;

        public string sort_field { get; set; } =
            string.Empty;

        public int sort_order { get; set; } = 1;
    }

    public sealed class FslTipoAplicacionPlanGuardarRequest
    {
        public string cod_plan { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public string tipo_desembolso { get; set; } =
            string.Empty;

        [JsonRequired]
        public bool activo { get; set; }

        public string usuario { get; set; } =
            string.Empty;
    }

    public sealed class FslTipoAplicacionCausaGuardarRequest
    {
        public string cod_causa { get; set; } =
            string.Empty;

        public string cod_plan { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public string monto_base { get; set; } =
            string.Empty;

        public string tipo_tabla { get; set; } =
            string.Empty;

        [JsonRequired]
        public bool activa { get; set; }

        public string usuario { get; set; } =
            string.Empty;
    }
}