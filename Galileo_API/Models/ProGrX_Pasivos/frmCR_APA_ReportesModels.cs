namespace Galileo_API.Models.ProGrX_Pasivos
{
    public class CrApaReportesOperacion
    {
        public required string Operacion { get; set; }
        public string Acreedor { get; set; } = string.Empty;
        public decimal? Monto { get; set; }
        public decimal? Saldo { get; set; }
        public DateTime? Formaliza { get; set; }
    }

    public class CrApaReportesSaldoCorte
    {
        public string Cod_Acreedor { get; set; } = string.Empty;
        public string Operacion { get; set; } = string.Empty;
        public decimal? Monto { get; set; }
        public decimal? Saldo_Hoy { get; set; }
        public DateTime? Fecha_Formaliza { get; set; }
        public decimal? Cuota { get; set; }
        public decimal? Tasa { get; set; }
        public int? Plazo { get; set; }
        public decimal? Saldo_Corte { get; set; }
        public DateTime? Corte { get; set; }
        public string Acreedor { get; set; } = string.Empty;
        public string Cuenta_Contable { get; set; } = string.Empty;
        public int? N_Cuota { get; set; }
        public int? Linea { get; set; }
        public decimal? Abono { get; set; }
        public decimal? Intereses { get; set; }
        public decimal? Comision { get; set; }
        public decimal? Amortizacion { get; set; }
        public decimal? Cargos { get; set; }
        public string Documento { get; set; } = string.Empty;
        public DateTime? Fecha_Pago { get; set; }
    }
}
