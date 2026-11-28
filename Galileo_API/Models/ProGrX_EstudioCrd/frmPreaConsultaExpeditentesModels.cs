namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaConsultaExpeditentesFiltroRequest
    {
        public required int codEmpresa { get; set; }
        public string? buscar { get; set; }
        public required int pagina { get; set; } = 0;
        public required int paginacion { get; set; } = 30;
        public string? sortField { get; set; }
        public required int sortOrder { get; set; } = -1;
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaFin { get; set; }
        public string? codEstadoV2 { get; set; }
    }

    public class FrmPreaConsultaExpeditentesListaResponse
    {
        public List<FrmPreaConsultaExpeditentesItem> lista { get; set; } = [];
        public int total { get; set; }
    }

    public class FrmPreaConsultaExpeditentesEstado
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FrmPreaConsultaExpeditentesItem
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cod_preanalisis_ref { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string fecha_creacion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    internal class FrmPreaConsultaExpeditentesItemData
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cod_preanalisis_ref { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public DateTime? fecha_creacion { get; set; } = null;
        public string estado { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}
