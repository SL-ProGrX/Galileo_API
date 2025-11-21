namespace Galileo.Models.Security
{
    public class ActivacionCuentaDto
    {
        public int UserId { get; set; }
        public string UsuarioActual { get; set; } = string.Empty;
        public string UsuarioAfectado { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
    }
}
