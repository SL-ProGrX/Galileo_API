namespace Galileo.Models.AF
{
    /// <summary>Respuesta paginada del catálogo de productos de beneficios.</summary>
    public class ProductoDataLista
    {
        public int total { get; set; }
        public List<ProductoData> productos { get; set; } = new List<ProductoData>();
    }

    /// <summary>Producto del catálogo de beneficios (afi_bene_productos).</summary>
    public class ProductoData
    {
        public string cod_producto { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
        public float? costo_unidad { get; set; } = 0;
        public string? cod_producto_inv { get; set; } = string.Empty;

        /// <summary>Indica que la fila es nueva en la tabla de Angular; no persiste en base de datos.</summary>
        public bool isNew { get; set; }
    }
}