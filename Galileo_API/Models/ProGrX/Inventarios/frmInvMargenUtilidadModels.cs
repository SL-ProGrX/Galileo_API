namespace Galileo.Models.INV
{
    public class InvMargenUtilidadPrecioAplicarRequest
    {
        public string cod_precio { get; set; } = string.Empty;
        public decimal utilidad { get; set; } = 0;
    }

    public class InvMargenUtilidadAplicarRequest
    {
        public int cod_linea { get; set; } = 0;
        public int cod_sublinea { get; set; } = 0;
        public string modo { get; set; } = string.Empty;
        public bool actualiza_precio_regular { get; set; } = false;
        public decimal utilidad_precio_regular { get; set; } = 0;
        public List<InvMargenUtilidadPrecioAplicarRequest> precios { get; set; } = [];
    }
}