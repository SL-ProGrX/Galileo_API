namespace PgxAPI.Models.SIF
{
    public class SifComunicadoDto
    {
        public int cod_comunicado { get; set; }
        public DateTime fecha { get; set; }
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
        public string ffuente { get; set; } = string.Empty;
        public string fcolor { get; set; } = string.Empty;
        public int fcursiva { get; set; }
        public int fnegrita { get; set; }
    }
}
