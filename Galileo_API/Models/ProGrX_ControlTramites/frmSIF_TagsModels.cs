namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public class frmSIFTagsModels
    {
        public class SifTagsListaResult
        {
            public int total { get; set; }
            public List<SifTagsData> lista { get; set; } = new();
        }

        public class SifTagsData
        {
            public string tag_codigo { get; set; } = string.Empty;
            public string? descripcion { get; set; }
            public bool? activo { get; set; }
        }

        public class SifTagsNotificacionDto
        {
            public string tag_codigo { get; set; } = string.Empty;
            public string? para_tag { get; set; }
            public string? para_tag_descripcion { get; set; }
            public string? para_email { get; set; }
            public string? cc_tag { get; set; }
            public string? cc_tag_descripcion { get; set; }
            public string? cc_email { get; set; }
            public string? mensaje { get; set; }
        }
    }
}