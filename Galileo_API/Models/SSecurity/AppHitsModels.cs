namespace Galileo.Models.Security
{
    public class AppHits
    {
        public string Hit_Cod { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int? Activo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
    }
}
