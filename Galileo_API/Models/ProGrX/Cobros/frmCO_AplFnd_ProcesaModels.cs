namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoAplFndProcInformacionData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string fecha_vencimiento { get; set; } = string.Empty;
        public decimal? mora_total { get; set; }
        public int? cuotas_mora { get; set; }
        public decimal? cuota_ao_pend { get; set; }
        public decimal? disponible_sinpe { get; set; }
        public decimal? disponible_fondos { get; set; }
        public int? ind_ahorro_obrero { get; set; }
        public int? ind_creditos { get; set; }
        public int? ind_sobres { get; set; }
        public decimal? monto_sobres { get; set; }
    }

    public class FondosAplicarRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public List<CoAplFndProcInformacionData> Seleccionados { get; set; } = new();
    }

    public class CoAplFndProcesadosResult
    {
        public int aplicados { get; set; } = 0;
        public int pendientes { get; set; } = 0;
    }
}
