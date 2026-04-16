namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class VivZonaData
    {
        public int idzona { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; } = false;
    }

    public class VivZonaCantonData
    {
        public string canton { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool check { get; set; } = false;
    }
}
