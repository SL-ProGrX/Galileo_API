namespace PgxAPI.Models.INV
{

    public class EntradasTomaFisicaDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class SalidasTomaFisicaDTO
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
