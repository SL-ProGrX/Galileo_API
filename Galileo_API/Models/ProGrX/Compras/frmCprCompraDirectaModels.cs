namespace PgxAPI.Models.CPR
{
    public class CompraDirectaData
    {
        public string cod_factura { get; set; } = string.Empty;
        public int cod_proveedor { get; set; }
        public string cod_orden { get; set; } = string.Empty;
        public string cod_compra { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string forma_pago { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public float sub_total { get; set; }
        public string notas { get; set; } = string.Empty;
        public float descuento { get; set; }
        public float imp_ventas { get; set; }
        public float imp_consumo { get; set; }
        public float total { get; set; }
        public string cxp_estado { get; set; } = string.Empty;
        public string asiento_estado { get; set; } = string.Empty;
        public DateTime asiento_fecha { get; set; }
        public string genera_user { get; set; } = string.Empty;
        public DateTime genera_fecha { get; set; }
        public DateTime anula_fecha { get; set; }
        public DateTime anula_fec_afecta { get; set; }
        public string anula_user { get; set; } = string.Empty;
        public string asiento_numero { get; set; } = string.Empty;
        public string anula_asiento_numero { get; set; } = string.Empty;
        public string causa_desc { get; set; } = string.Empty;
        public string causa_id { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
        public string divisa { get; set; } = string.Empty;
        public string tipo_pago { get; set; } = string.Empty;
    }

    public class CompraDirectaDetalle
    {
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_bodega { get; set; } = string.Empty;
        public float cantidad { get; set; }
        public float precio { get; set; }
        public float descuento { get; set; }
        public float imp_ventas { get; set; }
        public float total { get; set; }

        public bool i_existe { get; set; }
        public bool i_completo { get; set; }
    }

    public class CompraDirectaInsert
    {
        public string cod_factura { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string causa { get; set; } = string.Empty;
        public string? notas { get; set; }
        public int cod_proveedor { get; set; }

        public string forma_pago { get; set; } = string.Empty;
        public string tipo_pago { get; set; } = string.Empty;

        public string divisa { get; set; } = string.Empty;

        public float sub_total { get; set; }
        public float descuento { get; set; }
        public float imp_ventas { get; set; }
        public float total { get; set; }

        public List<CompraDirectaDetalle> lineas { get; set; } = new List<CompraDirectaDetalle>();
    }

    public class CompraDirectaListaData
    {
        public List<CompraDirectaDetalle> lineas { get; set; } = new List<CompraDirectaDetalle>();
        public int total { get; set; }
        public float cantidad { get; set; }
    }
}
