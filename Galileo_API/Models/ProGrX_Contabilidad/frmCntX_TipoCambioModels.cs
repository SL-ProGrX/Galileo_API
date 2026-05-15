namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXTipoCambioInicializaRequest
    {
        public decimal tc_actual { get; set; } = 0;
        public decimal monto_actual { get; set; } = 0;
        public string moneda { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public DateTime fecha { get; set; } = DateTime.Today;
    }

    public class CntXTipoCambioInicializaData
    {
        public decimal tc_actual { get; set; } = 0;
        public decimal tc_inicial { get; set; } = 0;
        public decimal monto_actual { get; set; } = 0;
        public decimal monto_divisa { get; set; } = 0;
        public decimal monto_funcional { get; set; } = 0;
        public decimal tc_permitido { get; set; } = 0;
        public decimal variacion { get; set; } = 0;
        public string moneda { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string divisa_descripcion { get; set; } = string.Empty;
        public DateTime fecha { get; set; } = DateTime.Today;
    }
}
