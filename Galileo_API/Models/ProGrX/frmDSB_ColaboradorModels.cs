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

    public class ColaboradorClaveCambiaRequest
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string ClaveActual { get; set; } = string.Empty;
        public string ClaveNueva { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
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

    public class ColaboradorAutorizacionRequest
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string BoletaId { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;
        public string EstadoNuevo { get; set; } = string.Empty;
    }

    public class ColaboradorSolicitudConfiguracionRequest
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Opcion { get; set; } = string.Empty;
        public string? Tipo { get; set; }
    }

    public class ColaboradorSolicitudTipoData
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ColaboradorSolicitudConfiguracionData
    {
        public List<ColaboradorSolicitudTipoData> Tipos { get; set; } = [];
        public DateTime FechaActual { get; set; }
        public DateTime? FechaMinima { get; set; }
        public decimal DiasDisponibles { get; set; }
        public bool RequiereAutorizacion { get; set; }
        public decimal? HorasMaximas { get; set; }
        public bool PermiteLiquidacion { get; set; }
        public decimal? PorcentajePatrono { get; set; }
    }

    public class ColaboradorSolicitudDiasRequest
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public DateTime? Inicio { get; set; }
        public DateTime? Corte { get; set; }
    }

    public class ColaboradorSolicitudRegistrarRequest
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Opcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateTime? Inicio { get; set; }
        public DateTime? Corte { get; set; }
        public int? Horas { get; set; }
        public int? Dias { get; set; }
        public decimal? PorcentajePatrono { get; set; }
        public string Estado { get; set; } = "S";
        public short? LiquidaId { get; set; }
    }

    public class ColaboradorSolicitudRegistroData
    {
        public string BoletaId { get; set; } = string.Empty;
    }
}
