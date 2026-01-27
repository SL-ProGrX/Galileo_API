namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXEmpresaDto
    {
        public int id_empresa { get; set; } = 0;
        public string nombre { get; set; } = string.Empty;
        public string cedula_juridica { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string apto_postal { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string telefono { get; set; } = string.Empty;
        public string fax { get; set; } = string.Empty;
        public string contacto { get; set; } = string.Empty;
    }
}
