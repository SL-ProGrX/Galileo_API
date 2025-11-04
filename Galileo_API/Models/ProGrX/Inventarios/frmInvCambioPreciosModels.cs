namespace PgxAPI.Models.INV
{
    public class FacturaPrecioDetalleDto
    {
        public string cod_factura { get; set; } = string.Empty;
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public string cod_bodega { get; set; } = string.Empty;
        public float precio { get; set; }
        public float imp_ventas { get; set; }
        public float descuento { get; set; }
        public float total { get; set; }
        public float porc_utilidad { get; set; }
        public decimal nuevo_precio { get; set; }

    }

    public class PrecioExcelDto
    {
        public string? cod_producto { get; set; }
        public string? descripcion { get; set; }
        public int linea_id { get; set; }
        public string? no_existe { get; set; }
        public decimal precio_actual { get; set; }
        public decimal precio_nuevo { get; set; }
        public string? procesa_cambio { get; set; }
        public string? notas { get; set; }
        public string? categoria { get; set; }
        public string? familia { get; set; }
        public string? unidad_medida { get; set; }

    }
}