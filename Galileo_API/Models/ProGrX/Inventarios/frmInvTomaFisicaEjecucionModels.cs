namespace PgxAPI.Models.INV
{
    public class EntradasTomaFisicaDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class SalidasTomaFisicaDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ProductosTomaFisica
    {
        public string cod_producto { get; set; } = string.Empty;
        public int existencia_fisica { get; set; }
    }
}