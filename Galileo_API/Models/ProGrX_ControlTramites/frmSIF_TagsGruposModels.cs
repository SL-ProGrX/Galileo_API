namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public class SifGruposData
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string? descripcion { get; set; }
    }

    public class SifGruposGuardarRequest
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string? descripcion { get; set; }
    }

    public class SifGruposMiembroData
    {
        public string usuario { get; set; } = string.Empty;
        public string? descripcion { get; set; }
        public bool asignado { get; set; }
    }

    public class SifGruposMiembroAsignarRequest
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool? asignado { get; set; }
    }

    public class SifGruposTagData
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string? descripcion { get; set; }
        public bool asignado { get; set; }
    }

    public class SifGruposTagAsignarRequest
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string tag_codigo { get; set; } = string.Empty;
        public bool? asignado { get; set; }
    }
}