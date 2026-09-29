namespace Galileo.Models.ProGrX
{
    public class ColaboradorVinculoData
    {
        public bool Vinculado { get; set; }
        public string? EmpleadoId { get; set; }
    }

    public class ColaboradorPerfilData
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? EstadoPersona { get; set; }
        public string? CodNomina { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public string? CentroTrabajo { get; set; }
        public string? Departamento { get; set; }
        public string? Seccion { get; set; }
        public string? FotoBase64 { get; set; }
        public string? FotoContentType { get; set; }
    }

    public class ColaboradorEmpleadoOpcionData
    {
        public string Id { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string EmpleadoId { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string? EstadoPersona { get; set; }
    }

    public class ColaboradorAccesoRequest
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public bool Vincular { get; set; } = false;
        public string AppVersion { get; set; } = string.Empty;
    }

    public class ColaboradorAccesoData
    {
        public bool ClaveValida { get; set; }
        public bool Vinculado { get; set; }
        public ColaboradorPerfilData? Perfil { get; set; }
        public string? ErrorVinculacion { get; set; }
    }

    public class ColaboradorClaveReestableceRequest
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
    }

    public class ColaboradorClaveReestableceData
    {
        public bool Cambio { get; set; }
    }

    public class ColaboradorMenuRequest
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Opcion { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public string? Estado { get; set; }
        public string? Vista { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCorte { get; set; }
    }
}
