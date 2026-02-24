namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXDivisaData
    {
        public string cod_divisa { get; set; } = string.Empty;
        public int cod_contabilidad { get; set; } = 0;
        public string cod_cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public decimal tc_venta { get; set; } = 0;
        public decimal tc_compra { get; set; } = 0;
        public bool divisa_local { get; set; } = false;
        public int consecutivo { get; set; } = 0;
        public string cod_cuenta_gasto { get; set; } = string.Empty;
        public DateTime? tc_fecha { get; set; }
        public string currency_sim { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string ctaing { get; set; } = string.Empty;
        public string ctagst { get; set; } = string.Empty;
        public string ctaing_desc { get; set; } = string.Empty;
        public string ctagst_desc { get; set; } = string.Empty;
        public string unidad_desc { get; set; } = string.Empty;
        public string centro_desc { get; set; } = string.Empty;
    }

    public class CntXDivisaHistorialData
    {
        public int id_secuencia { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public decimal tc_compra { get; set; } = 0;
        public decimal tc_venta { get; set; } = 0;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
    }

    public class CntXDivisaTipoCambioData
    {
        public int id_cambio { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public decimal tc_compra { get; set; } = 0;
        public decimal tc_venta { get; set; } = 0;
    }
}
