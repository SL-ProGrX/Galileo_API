namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXTipoCambioData
    {
        public int id_cambio { get; set; } = 0;
        public decimal tc_compra { get; set; } = 0;
        public decimal tc_venta { get; set; } = 0;
        public DateTime inicio { get; set; } = DateTime.Now;
        public DateTime corte { get; set; } = DateTime.Now;
        public decimal? variacion { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
    }
}
