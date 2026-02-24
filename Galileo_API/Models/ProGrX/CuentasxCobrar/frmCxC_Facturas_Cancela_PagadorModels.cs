namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCFactPendienteCancelacionData
    {
        public int operacion { get; set; } = 0;
        public string cod_factura { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string cod_divisa { get; set; } = string.Empty;
        public DateTime? fecha_pago { get; set; }
        public decimal importe { get; set; } = 0;
        public DateTime? fecha_emision { get; set; }
        public DateTime? activa_fecha { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CxCFactCancPagFacturasRequest
    {
        public string pagador { get; set; } = string.Empty;
        public string divisa { get; set; } = string.Empty;
        public string factura { get; set; } = string.Empty;
        public string cliente { get; set; } = string.Empty;
    }

    public class CxCFactCancPagRegistrarAbonoRequest
    {
        public string mcaja { get; set; } = "";
        public int mapertura { get; set; } = 0;
        public string mtiquete { get; set; } = "";
        public string tipodoc { get; set; } = "";
        public string usuario { get; set; } = "";
        public string pagador { get; set; } = "";
        public List<CxCFactPendienteCancelacionData> lista { get; set; } = new List<CxCFactPendienteCancelacionData>();
    }
}
