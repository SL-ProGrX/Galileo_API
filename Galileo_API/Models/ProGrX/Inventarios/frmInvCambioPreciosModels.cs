namespace Galileo.Models.INV
{
    public class FacturaPrecioDetalleDto
    {
        public string cod_factura { get; set; } = string.Empty;
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required int cantidad { get; set; }
        public string cod_bodega { get; set; } = string.Empty;
        public required float precio { get; set; }
        public required float imp_ventas { get; set; }
        public required float descuento { get; set; }
        public required float total { get; set; }
        public required float porc_utilidad { get; set; }
        public required decimal nuevo_precio { get; set; }

    }

    public class PrecioExcelDto
    {
        public string? cod_producto { get; set; }
        public string? descripcion { get; set; }
        public required int linea_id { get; set; }
        public string? no_existe { get; set; }
        public required decimal precio_actual { get; set; }
        public required decimal precio_nuevo { get; set; }
        public string? procesa_cambio { get; set; }
        public string? notas { get; set; }
        public string? categoria { get; set; }
        public string? familia { get; set; }
        public string? unidad_medida { get; set; }

    }
}