namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXAreaUtilidadDto
    {
        public decimal AreaUTMes { get; set; }
        public decimal AreaUTAcumulada { get; set; }
        public decimal AreaUTCMes { get; set; }
        public decimal AreaUTCAcumulada { get; set; }
    }

    public class CntXAreaMayorizarRow
    {
        public decimal SaldoInicial { get; set; }
        public decimal TotalDebitos { get; set; }
        public decimal TotalCreditos { get; set; }
        public decimal CSaldoInicial { get; set; }
        public decimal CTotalDebitos { get; set; }
        public decimal CTotalCreditos { get; set; }
        public string CuentaMadre { get; set; } = string.Empty;
    }
}
