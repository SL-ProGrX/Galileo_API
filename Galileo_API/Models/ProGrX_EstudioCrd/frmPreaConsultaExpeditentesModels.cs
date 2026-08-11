namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaConsultaExpeditentesListaResponse
    {
        public List<FrmPreaConsultaExpeditentesItem> lista { get; set; } = [];
        public int total { get; set; }
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
