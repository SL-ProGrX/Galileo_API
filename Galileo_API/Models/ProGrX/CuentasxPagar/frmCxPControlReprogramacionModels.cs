namespace Galileo.Models.CxP
{
    public class VCxpProgramacionPago
    {
        public int Cod_Proveedor { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public string Cod_Factura { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Cxp_Estado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public DateTime Vence { get; set; }
        public DateTime Fecha_Ingreso { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Forma_Pago { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public decimal Importe_Divisa_Real { get; set; }
        public string DataKey { get; set; } = string.Empty;
    }

    public class Pago
    {
        public decimal Monto { get; set; }
        public decimal Importe_Real { get; set; }
    }

    public class AjusteFactura
    {
        public int Cod_Proveedor { get; set; }
        public string Cod_Factura { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public decimal Monto_Ajuste { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class FacturaDet
    {
        public int Pago { get; set; }
        public decimal Monto { get; set; }
    }
}