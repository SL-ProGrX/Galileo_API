namespace Galileo_API.Models.ProGrX.Clientes
{
    public class FrmAfCambioPinPersonaModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class FrmAfCambioPinAplicarModel
    {
        public string cedula { get; set; } = string.Empty;
        public string pinPlano { get; set; } = string.Empty;
        public string pinSeguro { get; set; } = string.Empty;
        public string ticket { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}
