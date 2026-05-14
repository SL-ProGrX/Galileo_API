namespace Galileo.Models.ProGrX.Fondos
{
    public class FndCierreMensualData
    {
        public required string cedula { get; set; }
        public int consec { get; set; }
        public int id_solicitud { get; set; }
        public required string codigo { get; set; }
        public int cuota { get; set; }
        public decimal abono { get; set; }
        public decimal intcp { get; set; }
        public decimal amortiza { get; set; }
        public DateTime fechas { get; set; }
        public DateTime fechap { get; set; }
        public required string tcon { get; set; }
        public required string ncon { get; set; }
        public required string estado { get; set; }
        public required string estado_asiento { get; set; }
        public required string cod_concepto { get; set; }
        public decimal saldo { get; set; }
        public string? cod_app { get; set; }
        public string? usuario { get; set; }
        public string? cod_caja { get; set; }
        public decimal cargo { get; set; }
        public decimal iva { get; set; }
    }
}