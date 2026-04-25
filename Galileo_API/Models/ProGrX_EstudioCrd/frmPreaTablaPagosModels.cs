namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class CrdPreaTablaPagosData
    {
        public int idx { get; set; } = 0;
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public int npagos { get; set; } = 0;
        public int cod_institucion { get; set; } = 0;
    }
}
