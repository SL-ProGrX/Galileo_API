namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class DivisaDto
    {
        public string? cod_divisa { get; set; }
        public string? descripcion { get; set; }
    }

    public class TipoCambioDto
    {
        public int id_cambio { get; set; }
        public decimal tc_compra { get; set; }
        public decimal tc_venta { get; set; }
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
    }

    public class ProcesarDiferencialRequestDto
    {
        public string codDivisa { get; set; }
        public decimal tcCompra { get; set; }
        public decimal tcVenta { get; set; }
    }

}
