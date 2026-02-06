namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CopiaEsquemaRequest
    {
        public int? codFuente { get; set; }
        public int? codDestino { get; set; }
        public bool? inicializa { get; set; }
    }

    public class ContabilidadDto
    {
        public int cod_contabilidad { get; set; }
        public string? nombre { get; set; }
        public int? nivel1 { get; set; }
        public int? nivel2 { get; set; }
        public int? nivel3 { get; set; }
        public int? nivel4 { get; set; }
        public int? nivel5 { get; set; }
        public int? nivel6 { get; set; }
        public int? nivel7 { get; set; }
        public int? nivel8 { get; set; }
    }

}
