namespace Galileo.Models.Security
{
    public class PerfilUsuarioDto
    {
        public int? UserId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tel_Cell { get; set; } = string.Empty;
        public string Tel_Trabajo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? token { get; set; } = string.Empty;
    }
}
