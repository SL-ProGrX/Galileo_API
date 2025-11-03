namespace PgxAPI.Models.CPR
{
    public class OrdenCompraSinFacturaData
    {
        public string cod_orden { get; set; } = string.Empty;
        public string tipo_orden { get; set; } = string.Empty;
        public string pin_entrada { get; set; } = string.Empty;
        public string pin_autorizacion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
        public string genera_fecha { get; set; } = string.Empty;
        public string genera_user { get; set; } = string.Empty;
        public string autoriza_fecha { get; set; } = string.Empty;
        public string autoriza_user { get; set; } = string.Empty;
        public float subtotal { get; set; }
        public float descuento { get; set; }
        public float imp_ventas { get; set; }
        public float? imp_consumo { get; set; }
        public float total { get; set; }
        public int cod_proveedor { get; set; }
        public string plantilla { get; set; } = string.Empty;
        public string causa_id { get; set; } = string.Empty;
        public string causa_desc { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
        public string no_solicitud { get; set; } = string.Empty;
    }

    public class OrdenCompraFacturaData
    {
        public string cod_factura { get; set; } = string.Empty;
        public string cod_proveedor { get; set; } = string.Empty;
        public string cod_orden { get; set; } = string.Empty;
        public string cod_compra { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string forma_pago { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public float sub_total { get; set; }
        public string notas { get; set; } = string.Empty;
        public float descuento { get; set; }
        public float imp_ventas { get; set; }
        public float imp_consumo { get; set; }
        public float total { get; set; }
        public string cxp_estado { get; set; } = string.Empty;
        public string asiento_estado { get; set; } = string.Empty;
        public string asiento_fecha { get; set; } = string.Empty;
        public string genera_user { get; set; } = string.Empty;
        public string genera_fecha { get; set; } = string.Empty;
        public string anula_fecha { get; set; } = string.Empty;
        public string anula_fec_afecta { get; set; } = string.Empty;
        public string anula_user { get; set; } = string.Empty;
        public string asiento_numero { get; set; } = string.Empty;
        public string anula_asiento_numero { get; set; } = string.Empty;
        public string causa { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
        public string compraNotas { get; set; } = string.Empty;

        public string no_solicitud { get; set; } = string.Empty;

    }

    public class OrdenCompraDetalleData
    {
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string unidad { get; set; } = string.Empty;
        public int qtyOrg { get; set; } = 0;
        public int qtyPend { get; set; } = 0;
        public int cantidad { get; set; } = 0;
        public string cod_bodega { get; set; } = string.Empty;
        public float precio { get; set; }
        public float imp_ventas { get; set; }
        public float descuento { get; set; }
        public float total { get; set; }
        public string tipoProd { get; set; } = string.Empty;
        public string familia { get; set; } = string.Empty;
    }

    public class CompraOrdenInsert
    {
        public string cod_factura { get; set; } = string.Empty;
        public string cod_causa { get; set; } = string.Empty;
        public int cod_proveedor { get; set; }
        public string cod_compra { get; set; } = string.Empty;
        public string cod_orden { get; set; } = string.Empty;
        public string genera_user { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public float sub_total { get; set; }
        public float descuento { get; set; }
        public float imp_ventas { get; set; }
        public float total { get; set; }
        public string? notas { get; set; }
        public string tipo_pago { get; set; } = string.Empty;
        public string forma_pago { get; set; } = string.Empty;
    }

    public class CompraOrdenContadoUpdate
    {
        public int cod_proveedor { get; set; }
        public string cod_factura { get; set; } = string.Empty;
        public float total { get; set; }
        public string genera_user { get; set; } = string.Empty;
    }

    //nuevos
    public class CompraOrdenLineasData
    {
        public int total { get; set; }
        public long cantidad { get; set; }
        public List<OrdenCompraDetalleData> lineas { get; set; } = new List<OrdenCompraDetalleData>();
    }

    public class CompraOrderLineaTablaFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
        public string? CodOrden { get; set; }
        public long? CodProveedor { get; set; }
    }

    public class ComprasOrdenDatos
    {
        public string cod_causa { get; set; } = "";
        public string factura { get; set; } = "";
        public string fecha { get; set; } = "";
        public string cod_orden { get; set; } = "";
        public string pin { get; set; } = "";
        public string tipo_pago { get; set; } = "";
        public string cod_factura { get; set; } = "";
        public string cod_compra { get; set; } = "";
        public int cod_proveedor { get; set; } = 0;
        public string genera_user { get; set; } = "";
        public string forma_pago { get; set; } = "";
        public string notas { get; set; } = "";

        public float total { get; set; }
        public float imp_ventas { get; set; }
        public float sub_total { get; set; }
        public float descuento { get; set; }

        public List<OrdenCompraDetalleData> lineas { get; set; } = new List<OrdenCompraDetalleData>();
    }


    public class FacturasAutorizarDTO
    {
        public int? id { get; set; }
        public string? cod_documento { get; set; }
        public string? nombre_prov { get; set; }
        public int? monto_total { get; set; }
        public string? estado_descripcion { get; set; }

    }
}
