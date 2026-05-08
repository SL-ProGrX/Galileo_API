namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class VivTramiteNotaOperacionData
    {
        public string numero_operacion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string expediente { get; set; } = string.Empty;
        public string numero_finca { get; set; } = string.Empty;
        public string num_plano_catastro { get; set; } = string.Empty;
        public string desc_zona { get; set; } = string.Empty;
        public decimal area_finca { get; set; } = 0;
        public string provincia { get; set; } = string.Empty;
        public string canton { get; set; } = string.Empty;
    }

    public class VivTramiteNotaData
    {
        public int id_nota { get; set; } = 0;
        public string nota { get; set; } = string.Empty;
        public string desc_estado { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string fecha_registro { get; set; } = string.Empty;
        public long id_garantia { get; set; } = 0;
        public int id_contacto { get; set; } = 0;
        public string tipo { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string numero_operacion { get; set; } = string.Empty;
        public string numero_finca { get; set; } = string.Empty;
    }


    public class VivTramiteNotaGuardarRequest
    {
        public int id_nota { get; set; } = 0;
        public long id_garantia { get; set; } = 0;
        public int id_contacto { get; set; } = 0;
        public string profesional { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
    }
}
