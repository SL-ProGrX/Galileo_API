namespace Galileo_API.Models.ProGrX.Cobros
{
    public sealed class CoGestionesMasivasCargaRequest
    {
        public string usuario_cobro { get; set; } = string.Empty;
        public List<string> cedulas { get; set; } = new();
    }

    public sealed class CoGestionesMasivasCargaItemDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal mora_financiera { get; set; } = 0;
        public decimal mora_legal { get; set; } = 0;
    }

    public sealed class CoGestionesMasivasCargaResultDto
    {
        public int total { get; set; } = 0;
        public decimal total_mora_financiera { get; set; } = 0;
        public decimal total_mora_legal { get; set; } = 0;
        public List<CoGestionesMasivasCargaItemDto> lista { get; set; } = new();
    }

    public sealed class CoGestionesMasivasProcesarRequest
    {
        public string usuario_cobro { get; set; } = string.Empty;
        public string cod_gestion { get; set; } = string.Empty;
        public string cod_causa { get; set; } = string.Empty;
        public string cod_arreglo { get; set; } = string.Empty;
        public decimal? monto { get; set; }
        public DateTime? vence { get; set; }
        public string notas { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
    }
}