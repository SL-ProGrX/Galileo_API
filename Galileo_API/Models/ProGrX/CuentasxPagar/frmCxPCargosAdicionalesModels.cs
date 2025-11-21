namespace Galileo.Models.CxP
{
    public class CargosAdicionalDto
    {
        public string Cod_Cargo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}