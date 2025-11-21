namespace Galileo.Models.SIF
{
    public class JuzgadosDto
    {
        public string cod_juzgado { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string telefono_01 { get; set; } = string.Empty;
        public string telefono_02 { get; set; } = string.Empty;
        public string tel_fax { get; set; } = string.Empty;
        public string email_01 { get; set; } = string.Empty;
        public string email_02 { get; set; } = string.Empty;
        public string apto_postal { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string nombre_contacto { get; set; } = string.Empty;
        public string sitio_web { get; set; } = string.Empty;

        public int provincia { get; set; }
        public string canton { get; set; } = string.Empty;
        public string distrito { get; set; } = string.Empty;
    }
}