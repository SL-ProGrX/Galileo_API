namespace Galileo_API.Models.ProGrX_Polizas
{

    public class PolizaAseguradoraDto
    {
        public DateTime? corte { get; set; }
        public string? identificacion { get; set; } = string.Empty;
        public string? nombre { get; set; } = string.Empty;
        public decimal? monto { get; set; }
        public DateTime? fechaNacimiento { get; set; }
        public string? genero { get; set; } = string.Empty;
        public string? nacionalidad { get; set; } = string.Empty;
        public string? movimiento { get; set; } = string.Empty;
    }


}

