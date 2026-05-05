namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class VivHonorariosDetalleOperacionData
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string identificacion_contacto { get; set; } = string.Empty;
        public string nombre_contacto { get; set; } = string.Empty;
    }

    public class VivHonorariosDetalleLineaData
    {
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
    }

    public class VivHonorariosDetalleGuardarLinea
    {
        public string codigo { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
    }

    public class VivHonorariosDetalleGuardarRequest
    {
        public int operacion { get; set; } = 0;
        public int id_contacto { get; set; } = 0;
        public int id_garantia { get; set; } = 0;
        public string profesional { get; set; } = string.Empty;
        public List<VivHonorariosDetalleGuardarLinea> lineas { get; set; } = new();
    }
}
