namespace Galileo.Models.ProGrX.ControlTramites
{
    public class SifTagsModulosProcesoData
    {
        public string cod_modulo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class SifTagsModulosProcesoGuardarRequest
    {
        public string cod_modulo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class SifTagsModulosEtiquetaData
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class SifTagsModulosEtiquetaGuardarRequest
    {
        public string cod_modulo { get; set; } = string.Empty;
        public string tag_codigo { get; set; } = string.Empty;
        public bool? asignado { get; set; }
    }
}