namespace Galileo_API.Models.ProGrX.Convenios
{
    public class CrConveniosAhorrosDetalleData
    {
        public int cod_contrato { get; set; } = 0;
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal mensualidad { get; set; } = 0;
        public decimal pendiente { get; set; } = 0;
        public decimal monto { get; set; } = 0;
    }
    
    public class CrConveniosAhorrosData
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cod_orden { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public decimal mensualidad_total { get; set; } = 0;
        public decimal monto_total { get; set; } = 0;
        public List<CrConveniosAhorrosDetalleData> ahorros { get; set; } = new();
    }
    
    public class CrConveniosAhorrosActualizarRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public int cod_orden { get; set; } = 0;
        public string cod_plan { get; set; } = string.Empty;
        public int cod_contrato { get; set; } = 0;
        public decimal monto { get; set; } = 0;
    }
}