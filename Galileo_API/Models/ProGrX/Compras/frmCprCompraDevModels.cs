namespace Galileo.Models.CPR
{
    public class FacturasDto
    {
        public string cod_factura { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class FacturaDto
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
        public Nullable<DateTime> anula_fecha { get; set; }
        public Nullable<DateTime> anula_fec_afecta { get; set; }
        public string anula_user { get; set; } = string.Empty;
        public string asiento_numero { get; set; } = string.Empty;
        public string anula_asiento_numero { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
    }

    public class FacturaDetalleDto
    {
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int? cantidad { get; set; }
        public string cod_bodega { get; set; } = string.Empty;
        public float? precio { get; set; }
        public float? imp_ventas { get; set; }
        public float? Total { get; set; }
    }

    public class BodegaDto
    {
        public string cod_bodega { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class DevolucionDto
    {
        public string devolucion { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string cod_proveedor { get; set; } = string.Empty;
        public float sub_total { get; set; }
        public float descuento { get; set; }
        public float impuesto { get; set; }
        public float total { get; set; }
        public string? notas { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string cargo { get; set; } = string.Empty;
    }

    public class DevolucionData
    {
        public string cod_compra_dev { get; set; } = string.Empty;
        public string cod_factura { get; set; } = string.Empty;
        public int cod_proveedor { get; set; }
        public string cod_cargo { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string notas { get; set; } = string.Empty;
        public float sub_total { get; set; }
        public float descuento { get; set; }
        public float imp_ventas { get; set; }
        public float imp_consumo { get; set; }
        public float total { get; set; }
        public string genera_user { get; set; } = string.Empty;
        public DateTime genera_fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public string asiento_estado { get; set; } = string.Empty;
        public DateTime asiento_fecha { get; set; }
        public string proveedor { get; set; } = string.Empty;
        public string cargox { get; set; } = string.Empty;
    }

    public class DevolucionInsert
    {
        public string cod_factura { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public long cod_proveedor { get; set; }
        public List<FacturaDetalleDto> lineas { get; set; } = new List<FacturaDetalleDto>();
        public float total { get; set; }
        public string? notas { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string cargo { get; set; } = string.Empty;
        public float sub_total { get; set; }
        public float descuento { get; set; }
        public float imp_ventas { get; set; }
    }
}