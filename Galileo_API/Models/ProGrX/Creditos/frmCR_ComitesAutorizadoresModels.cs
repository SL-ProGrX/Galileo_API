namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrComitesAutorizadoresLista<T>
    {
        public int total { get; set; }
        public List<T> lista { get; set; } = new();
    }

    public class CrComitesPuestoDto
    {
        public string id_puesto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool? isNew { get; set; }
    }

    public class CrComitesPersonaDto
    {
        public string cedula { get; set; } = string.Empty;
        public string cedula_original { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string id_puesto { get; set; } = string.Empty;
        public string puesto { get; set; } = string.Empty;
        public bool? activo { get; set; }
        public DateTime? fecha_activa { get; set; }
        public string usuario_activa { get; set; } = string.Empty;
        public DateTime? fecha_bloqueo { get; set; }
        public string usuario_bloqueo { get; set; } = string.Empty;
        public bool? isNew { get; set; }
    }

    public class CrComitesAsignacionDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrComitesAsignacionRequest
    {
        public int? id_comite { get; set; }
        public string cedula { get; set; } = string.Empty;
        public bool? asignado { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrComitesSpPassDto
    {
        public int Pass { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}