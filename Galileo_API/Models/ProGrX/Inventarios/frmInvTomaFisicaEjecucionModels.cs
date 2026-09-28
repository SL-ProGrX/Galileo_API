namespace Galileo.Models.INV
{
    public class EntradasTomaFisicaDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class SalidasTomaFisicaDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class ProductosTomaFisica
    {
        public string cod_producto { get; set; } = string.Empty;
        public int existencia_fisica { get; set; }
    }
}