namespace Galileo.Models.ProGrX_Nucleo
{
    public class AutorizadoresExpDto
    {
        public required int autorizador_id { get; set; }
        public string aut_usuario { get; set; } = string.Empty;
        public string aut_clave { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }
}