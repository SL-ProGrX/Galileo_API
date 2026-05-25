namespace Galileo.Models.AH
{
    public class AutorizadorePatrimonioDto
    {
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
    }

    public class FrmAhAutorizadoresGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FrmAhAutorizadoresGuardarResponse
    {
        public string usuario { get; set; } = string.Empty;
        public string accion { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }
}
