namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrDigitalCatalogoData
    {
        public required short id { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string codigo_catalogo { get; set; } = string.Empty;
        public string descripcion_catalogo { get; set; } = string.Empty;
        public string nombre_estado { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
        public required short orden { get; set; }
        public required bool activo { get; set; }
        public required bool final { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public required bool is_new { get; set; }
    }
}
