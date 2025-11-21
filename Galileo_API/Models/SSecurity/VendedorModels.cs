namespace Galileo.Models.Security
{
    public class Vendedor
    {
        public string Cod_Vendedor { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Cuenta_Cliente { get; set; } = string.Empty;
        public string Comision_Tipo { get; set; } = string.Empty;
        public double Comision_Cliente { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public int Activo { get; set; }
        public string Estado { get; set; } = string.Empty;

    }
}
