namespace Galileo_API.Models.ProGrX_Polizas
{
    public class PolAlertasParametrosGuardarDto
    {
        // Cliente envía esto
        public string UnidadTiempo { get; set; } = "DAY"; // "DAY" | "HOUR" | "MINUTE"
        public int AlertaRoja { get; set; } = 0;
        public int AlertaAmarilla { get; set; } = 0;

        public string? ContactoOficina { get; set; }
        public string? ContactoTelefono { get; set; }
        public string? ContactoEmail { get; set; }
    }

    public class PolAlertasParametrosDto
    {
        // Cliente recibe esto (para pintar pantalla)
        public string UnidadTiempo { get; set; } = "";
        public string UnidadTiempoEsp { get; set; } = ""; // "Días" | "Horas" | "Minutos"
        public int AlertaRoja { get; set; } = 0;
        public int AlertaAmarilla { get; set; } = 0;

        public string? ContactoOficina { get; set; }
        public string? ContactoTelefono { get; set; }
        public string? ContactoEmail { get; set; }
    }

    public class PolAlertasEmailAgregarDto
    {
        // Cliente envía esto
        public string Email { get; set; } = "";
    }

    public class PolAlertasEmailDto
    {
        // Cliente recibe esto
        public int IdRegistro { get; set; } = 0;
        public string Email { get; set; } = "";
        public string? UsuarioInserta { get; set; }
        public DateTime? FechaInserta { get; set; }
    }
}
