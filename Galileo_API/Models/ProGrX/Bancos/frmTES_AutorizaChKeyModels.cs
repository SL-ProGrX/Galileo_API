namespace PgxAPI.Models.ProGrX.Bancos
{
    public class AutorizaChKeyData
    {
        public int CodEmpresa { get; set; } = 0;
        public string usuarioLogin { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string claveActual { get; set; } = string.Empty;
        public string claveNueva { get; set; } = string.Empty;
        public string claveConfirmar { get; set; } = string.Empty;
    }
}