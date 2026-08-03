namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCFacturasCancelaPendienteDto
    {
        public int Operacion { get; set; }
        public string Cod_Factura { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime? Fecha_Pago { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Importe { get; set; }
        public DateTime? Fecha_Emision { get; set; }
        public DateTime? Activa_Fecha { get; set; }
    }

    public class CxCFacturasCancelaFacturaRequestDto
    {
        public required int Operacion { get; set; }
        public string Factura { get; set; } = string.Empty;
        public decimal? Abono { get; set; }
        public string Tipo_Documento { get; set; } = string.Empty;
        public string Numero_Documento { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CxCFacturasCancelaAbonoRequestDto
    {
        public string Tipo_Documento { get; set; } = string.Empty;
        public string Numero_Documento { get; set; } = string.Empty;
        public string Caja { get; set; } = string.Empty;
        public int? Apertura { get; set; }
        public string Tiquete { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }
}
