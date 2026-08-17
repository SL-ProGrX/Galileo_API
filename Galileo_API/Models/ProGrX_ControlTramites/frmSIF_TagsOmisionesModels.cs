namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public class SifTagsOmisionesModel
    {
        public int id_Error { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
        public string activo { get; set; } = string.Empty;
    }

    public class SifTagsOmisionesGuardarRequest
    {
        public int Id_Error { get; set; } = 0;
        public string Descripcion { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Activo { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class SifTagsOmisionesEliminarRequest
    {
        public int Id_Error { get; set; } = 0;
        public string Usuario { get; set; } = string.Empty;
    }

    public class SifTagsOmisionesModuloOpcion
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class SifTagsOmisionesAsignacionModel
    {
        public int id_Error { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class SifTagsOmisionesAsignacionRequest
    {
        public int Id_Error { get; set; } = 0;
        public string Cod_Modulo { get; set; } = string.Empty;
        public required bool Asignado { get; set; }
    }
}
