namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoAplExcContrAplInformacionData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string operaciones { get; set; } = string.Empty;
        public decimal? mora_total { get; set; }
        public int? cuotas_mora { get; set; }
        public decimal? disponible_exced { get; set; }
        public decimal? intereses { get; set; }
        public decimal? poliza { get; set; }
    }

    public class ExcContratosAplicarRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public List<CoAplExcContrAplInformacionData> Seleccionados { get; set; } = new();
    }

    public class CoAplExcContrAplicadosResult
    {
        public int aplicados { get; set; } = 0;
        public int pendientes { get; set; } = 0;
    }
}
