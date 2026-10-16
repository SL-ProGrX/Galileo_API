namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public sealed class PreaScoreCrediticioHistorialDto
    {
        public List<PreaScoreCrediticioRegistroDto> historial { get; set; } = new();
    }

    public sealed class PreaScoreCrediticioRegistroDto
    {
        public int id { get; set; }
        public string cod_preanalisis { get; set; } = string.Empty;
        public byte tipo_score { get; set; }
        public DateTime fecha_registra { get; set; }
        public string usuario_registra { get; set; } = string.Empty;
        public List<PreaScoreCrediticioDetalleDto> detalles { get; set; } = new();
    }

    public sealed class PreaScoreCrediticioDetalleDto
    {
        public int id { get; set; }
        public string modelo { get; set; } = string.Empty;
        public string? objetivo { get; set; }
        public decimal probabilidad_defecto { get; set; }
        public bool decision_binaria { get; set; }
        public string veredicto { get; set; } = string.Empty;
        public string categoria_riesgo { get; set; } = string.Empty;
        public decimal umbral_aplicado { get; set; }
    }
}
