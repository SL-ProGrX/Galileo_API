namespace Galileo.Models.ProGrX.Clientes
{
    public class AfTelefonoDto
    {
        public int Telefono { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Contacto { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public int Tipo { get; set; }
        public string Ext { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}