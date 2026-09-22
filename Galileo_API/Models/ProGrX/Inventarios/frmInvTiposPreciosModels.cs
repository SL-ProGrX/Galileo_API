namespace Galileo.Models.INV
{
    public class Precio
    {
        public string cod_Precio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool activo { get; set; }
        public string omision { get; set; } = string.Empty;
    }

    public class PreciosDataLista
    {
        public int total { get; set; }
        public List<Precio> precios { get; set; } = new List<Precio>();
    }
}