namespace Galileo.Models.GEN
{
    /// <summary>
    /// Parámetros de los módulos comerciales (pv_parametros_mod).
    /// Homologado a rs! de sbCargaParGen (frmGenParametrosPro).
    /// Las claves de autorización POS no se exponen (Regla 20).
    /// </summary>
    public class GenParametrosProData
    {
        // General
        public int chk_factura_min { get; set; }
        public int chk_descuento_bifiv { get; set; }
        public int chk_costo_ultcomp { get; set; }
        public int chk_costo_cero { get; set; }
        public int chk_modo_asiento { get; set; }
        public string aplica_iv_sobre { get; set; } = "SB";

        // CxP
        public string cxp_tc_nc { get; set; } = string.Empty;
        public string cxp_tc_nd { get; set; } = string.Empty;
        public string cxp_tc_pago { get; set; } = string.Empty;

        // Inv / Compras
        public string inv_tc_entrada { get; set; } = string.Empty;
        public string inv_tc_salida { get; set; } = string.Empty;
        public string inv_tc_traslado { get; set; } = string.Empty;
        public string inv_tc_compra { get; set; } = string.Empty;

        // POS
        public string pos_tc_factura { get; set; } = string.Empty;
        public string pos_tc_recibo { get; set; } = string.Empty;
        public string pos_rei_user { get; set; } = string.Empty;
        public string pos_cp_user { get; set; } = string.Empty;

        // Tipos de Cambio
        public decimal tc_compra { get; set; }
        public decimal tc_venta { get; set; }
    }
}
