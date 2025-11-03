namespace PgxAPI.Models.GEN
{
    public class PvParametrosModDto
    {
        public int COD_PAR { get; set; }
        public int CHK_FACTURA_MIN { get; set; }
        public int CHK_DESCUENTO_BIFIV { get; set; }
        public int CHK_COSTO_ULTCOMP { get; set; }
        public int CHK_COSTO_CERO { get; set; }
        public int CHK_MODO_ASIENTO { get; set; }
        public string APLICA_IV_SOBRE { get; set; } = string.Empty;
        public string CXP_TC_NC { get; set; } = string.Empty;
        public string CXP_TC_ND { get; set; } = string.Empty;
        public string CXP_TC_PAGO { get; set; } = string.Empty;
        public string INV_TC_ENTRADA { get; set; } = string.Empty;
        public string INV_TC_SALIDA { get; set; } = string.Empty;
        public string INV_TC_TRASLADO { get; set; } = string.Empty;
        public string INV_TC_COMPRA { get; set; } = string.Empty;
        public string POS_TC_FACTURA { get; set; } = string.Empty;
        public string POS_TC_RECIBO { get; set; } = string.Empty;
        public string POS_REI_USER { get; set; } = string.Empty;
        public string POS_REI_CLAVE { get; set; } = string.Empty;
        public string POS_CP_USER { get; set; } = string.Empty;
        public string POS_CP_CLAVE { get; set; } = string.Empty;
        public decimal? TC_COMPRA { get; set; }
        public decimal? TC_VENTA { get; set; }
        public Nullable<DateTime> TC_FECHA { get; set; }
        public string TC_USUARIO { get; set; } = string.Empty;
    }
}
