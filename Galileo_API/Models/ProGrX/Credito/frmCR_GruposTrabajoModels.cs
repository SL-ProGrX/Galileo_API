namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrGrupoTrabajoGrupoData
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrGrupoTrabajoGrupoComboData
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrGrupoTrabajoGrupoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrGrupoTrabajoMiembroData
    {
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class CrGrupoTrabajoMiembroMarcarRequest
    {
        public string usuario_sesion { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool marcado { get; set; } = false;
    }

    public class CrGrupoTrabajoEtiquetaData
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class CrGrupoTrabajoEtiquetaMarcarRequest
    {
        public string usuario_sesion { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public string tag_codigo { get; set; } = string.Empty;
        public bool marcado { get; set; } = false;
    }

    public class CrGrupoTrabajoComiteData
    {
        public int id_comite { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class CrGrupoTrabajoComiteMarcarRequest
    {
        public string usuario_sesion { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public int id_comite { get; set; } = 0;
        public bool marcado { get; set; } = false;
    }
}
