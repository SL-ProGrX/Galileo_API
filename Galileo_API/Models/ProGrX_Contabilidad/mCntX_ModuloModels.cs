namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXParametrosDto
    {
        public int CodigoConta { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public int Nivel1 { get; set; }
        public int Nivel2 { get; set; }
        public int Nivel3 { get; set; }
        public int Nivel4 { get; set; }
        public int Nivel5 { get; set; }
        public int Nivel6 { get; set; }
        public int Nivel7 { get; set; }
        public int Nivel8 { get; set; }
        public int TotalChr { get; set; }
        public string Mascara { get; set; } = string.Empty;
        public string MascaraCod { get; set; } = string.Empty;
        public int PeriodoMes { get; set; }
        public int PeriodoAnio { get; set; }
        public string DivisaLocal { get; set; } = string.Empty;
    }

    public class CntXPeriodoDivisaDto
    {
        public string DivisaLocal { get; set; } = string.Empty;
        public DateTime? Periodo { get; set; }
    }

    public class CntXTipoCambioDto
    {
        public decimal TcVenta { get; set; }
        public decimal TcCompra { get; set; }
    }
}
