namespace Galileo_API.Models.ProGrX.Bancos
{
    public class TesMonitorSinpeDebCrdModels
    {
        public long Consecutivo { get; set; } = 0;
        public decimal Debito { get; set; } = 0;
        public decimal Credito { get; set; } = 0;
        public string Descripcion { get; set; } = string.Empty;
    }
}
