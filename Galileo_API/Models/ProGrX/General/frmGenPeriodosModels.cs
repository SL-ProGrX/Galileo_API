namespace Galileo.Models.GEN
{
    public class PeriodoDto
    {
        public int? Anio { get; set; }
        public int? Mes { get; set; }
        public int? Proceso { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int? Activo { get; set; }
        public string DescripcionEstado { get; set; } = string.Empty;
    }
}