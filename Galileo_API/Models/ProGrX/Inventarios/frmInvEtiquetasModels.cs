namespace Galileo.Models.INV
{
    public class ProductData
    {
        public int Cantidad { get; set; } = 0;
        public string Modelo { get; set; } = string.Empty;
        public string Cod_Barras { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Producto { get; set; } = string.Empty;
        public float Precio { get; set; } = 0;
    }

    public class GenerateSatoRequest
    {
        public string Redondeo { get; set; } = string.Empty;
        public required decimal Value { get; set; } = 0;
        public required int Opcion { get; set; } = 0;
        public string? CodProducto { get; set; } = string.Empty;
        public string? CodFactura { get; set; } = string.Empty;
        public int? CodProveedor { get; set; }
    }

    public class CodBarrasData
    {
        public string Cod_Barras { get; set; } = string.Empty;
        public int Cod_ProdClas { get; set; } = 0;
    }
}