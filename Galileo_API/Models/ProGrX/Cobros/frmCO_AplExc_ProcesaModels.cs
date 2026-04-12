 namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoAplExcProcInformacionData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string fecha_vencimiento { get; set; } = string.Empty;
        public decimal? mora_total { get; set; }
        public int? cuotas_mora { get; set; }
        public decimal? disponible_exced { get; set; }
        public decimal? intereses { get; set; }
        public decimal? poliza { get; set; }
        public decimal? sobrante { get; set; }
        public string giro_exced { get; set; } = string.Empty;
        public string excepciones { get; set; } = string.Empty;
    }

    public class ExcedenteAplicarRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public List<CoAplExcProcInformacionData> Seleccionados { get; set; } = new();
    }

    public class CoAplExcProcesadosResult
    {
        public int aplicados { get; set; } = 0;
        public int pendientes { get; set; } = 0;
    }
}
