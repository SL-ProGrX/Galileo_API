using System.Text.Json.Serialization;

namespace Galileo.Models.FSL
{
    public sealed class FslRequisitoDto
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
    }

    public sealed class FslRequisitosFiltros
    {
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string filtro { get; set; } = string.Empty;
        public string sort_field { get; set; } = "cod_requisito";
        public int sort_order { get; set; } = 1;
    }

    public sealed class FslRequisitoGuardarRequest
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;

        [JsonRequired]
        public bool activo { get; set; } = false;

        public string usuario { get; set; } = string.Empty;
    }

    public sealed class FslRequisitoCausaDto
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool opcional { get; set; } = false;
        public bool asignado { get; set; } = false;
    }

    public sealed class FslRequisitoAsignacionRequest
    {
        public string cod_plan { get; set; } = string.Empty;
        public string cod_causa { get; set; } = string.Empty;
        public string cod_requisito { get; set; } = string.Empty;

        [JsonRequired]
        public bool opcional { get; set; } = false;

        [JsonRequired]
        public bool asignado { get; set; } = false;

        public string usuario { get; set; } = string.Empty;
    }
}