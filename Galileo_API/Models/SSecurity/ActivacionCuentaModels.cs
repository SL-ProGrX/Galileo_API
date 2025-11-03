namespace PgxAPI.Models
{
    public class ActivacionCuentaDTO
    {
        public int UserId { get; set; }
        public string UsuarioActual { get; set; } = string.Empty;
        public string UsuarioAfectado { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
    }

    public class ErrorActivacionCuentaDTO
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
