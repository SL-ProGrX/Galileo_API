namespace Galileo.Models
{
    public class MProGrXSecurityMainBitacora
    {
        public int CodEmpresa { get; set; }
        public int pCliente { get; set; }
        public string usuario { get; set; } = string.Empty;
        public int vModulo { get; set; }
        public string strTipoMovimiento { get; set; } = string.Empty;
        public string strDetalleMovimiento { get; set; } = string.Empty;
        public string AppName { get; set; } = "ProGrX";
        public string AppVersion { get; set; } = string.Empty;
        public string Maquina { get; set; } = string.Empty;
        public string Maquina_MAC { get; set; } = string.Empty;
    }
}
