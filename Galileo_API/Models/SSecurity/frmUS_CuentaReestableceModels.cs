namespace PgxAPI.Models.US
{
    public class CuentaReestablecer
    {
        public int UsuarioId { get; set; } = 0;
        public string UsuarioNombre { get; set; } = string.Empty;
        public bool CambiaSesion { get; set; } = false;
        public string Nuevo { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string UsuarioMovimiento { get; set; } = string.Empty;
    }
}
