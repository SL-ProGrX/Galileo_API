namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasTiposCambiosData
    {
        public int id_cambio { get; set; }
        public int cod_contabilidad { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
        public decimal tc_compra { get; set; }
        public decimal tc_venta { get; set; }
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public decimal variacion { get; set; }
        public string? usuario { get; set; }
        public DateTime? fecha { get; set; }
        public bool isNew { get; set; } = false;
    }
}