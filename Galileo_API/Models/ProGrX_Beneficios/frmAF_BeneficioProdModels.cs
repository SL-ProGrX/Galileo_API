namespace PgxAPI.Models.AF
{
    public class ProductoDataLista
    {
        public int Total { get; set; }
        public List<ProductoData> productos { get; set; } = new List<ProductoData>();
    }

    public class ProductoData
    {
        public string cod_producto { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
        public float? costo_unidad { get; set; } = 0;
        public string? cod_producto_inv { get; set; } = string.Empty;
    }
}