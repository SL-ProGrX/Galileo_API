namespace PgxAPI.Models.CxP
{
    public class CxpEventosDto
    {
        public int idx { get; set; }
        public string itmx { get; set; } = string.Empty;
    }

    public class CxpEventosVentasFiltros
    {
        public int? id_venta { get; set; }
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
        public int? proveedorId { get; set; }
        public string? proveedorNombre { get; set; } = string.Empty;
        public string? cedula { get; set; } = string.Empty;
        public string? nombre { get; set; } = string.Empty;
        public string? usuario { get; set; } = string.Empty;
        public string appcod { get; set; } = string.Empty;
    }

    public class CxpEventosVentasDto
    {
        public int id_venta { get; set; }
        public int cod_evento { get; set; } 
        public int cod_proveedor { get; set; }
        public string cliente_cedula { get; set; } = string.Empty;
        public string cliente_nombre { get; set; } = string.Empty;
        public decimal v_factura { get; set; }
        public decimal v_sub_total { get; set; }
        public decimal v_iva { get; set; }
        public decimal v_total { get; set; }
        public string v_descripcion { get; set; } = string.Empty;
        public decimal cxp_comision { get; set; }
        public decimal cxp_comision_porc { get; set; }
        public string cxp_documento { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public int crd_operacion { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string evento_desc { get; set; } = string.Empty;
        public string proveedor_desc { get; set; } = string.Empty;
        public string crd_codigo { get; set; } = string.Empty;
        public string crd_desc { get; set; } = string.Empty;
        public decimal monto_girar { get; set; }
    }
}