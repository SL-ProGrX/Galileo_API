namespace Galileo.Models.INV
{
    public class Precio
    {
        public string Cod_Precio { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string omision { get; set; } = string.Empty;
    }

    public class PreciosDataLista
    {
        public int Total { get; set; }
        public List<Precio> Precios { get; set; } = new List<Precio>();
    }
}