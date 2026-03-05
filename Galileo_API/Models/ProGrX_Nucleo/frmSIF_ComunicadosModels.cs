namespace Galileo.Models.SIF
{
    public class SifComunicadoDto
    {
        public required int cod_comunicado { get; set; }
        public required DateTime fecha { get; set; }
        public required DateTime inicio { get; set; }
        public required DateTime corte { get; set; }
        public  string usuario { get; set; } = string.Empty;
        public  string nota { get; set; } = string.Empty;
        public string ffuente { get; set; } = string.Empty;
        public string fcolor { get; set; } = string.Empty;
        public required int fcursiva { get; set; }
        public required int fnegrita { get; set; }
    }
}
