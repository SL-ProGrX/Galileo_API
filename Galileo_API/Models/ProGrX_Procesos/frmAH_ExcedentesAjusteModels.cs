namespace Galileo.Models.AH
{
    public class AjusteExcedenteDto
    {
        public string Ajuste_id { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Ajuste { get; set; } = 0;
        public string Detalle { get; set; } = string.Empty;
        public DateTime Vencimiento { get; set; }
    }
}
