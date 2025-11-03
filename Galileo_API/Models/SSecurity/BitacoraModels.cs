namespace PgxAPI.Models
{
    public class BitacoraRequestDto
    {
        public int Cliente { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaCorte { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraCorte { get; set; }
        public string? Usuario { get; set; }
        public int? Modulo { get; set; }
        public string? Movimiento { get; set; }
        public string? Detalle { get; set; }
        public string? AppName { get; set; }
        public string? AppVersion { get; set; }
        public string? LogEquipo { get; set; }
        public string? LogIP { get; set; }
        public string? EquipoMAC { get; set; }

        public bool todas { get; set; }
        public bool todos { get; set; }
    }

    public class BitacoraResultDto
    {
        public string ModuloDesc { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Movimiento { get; set; } = string.Empty;
        public DateTime Fecha_FORMAT { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public string App_Nombre { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string App_Equipo { get; set; } = string.Empty;
        public string Equipo_MAC { get; set; } = string.Empty;
        public string App_IP { get; set; } = string.Empty;


    }
}
