namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoAplFndContrAplInformacionData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string operaciones { get; set; } = string.Empty;
        public decimal? mora_total { get; set; } 
        public int? cuotas_mora { get; set; }
        public decimal? total_fondos { get; set; }
        public decimal? intereses { get; set; }
    }

    public class ContratosAplicarRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public List<CoAplFndContrAplInformacionData> Seleccionados { get; set; } = new();
    }

    public class CoAplFndContrAplicadosResult
    {
        public int aplicados { get; set; } = 0;
        public int pendientes { get; set; } = 0;
    }
}
