namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TesMonitoreoBancosDto
    {
        public int id_banco { get; set; }
        public string idx { get; set; } = string.Empty;
    }

    public class TesMonitoreoDto
    {
        public int codigoCierre { get; set; }
        public int codigoBanco { get; set; }
        public string? descripcionBanco { get; set; }
        public string? cuentaBanco { get; set; }
        public DateTime inicio { get; set; }
        public decimal saldoInicial { get; set; }
        public decimal totalDebitos { get; set; }
        public decimal totalCreditos { get; set; }
        public decimal chequesPendientes { get; set; }
        public decimal chequesDia { get; set; }
        public decimal transferencias { get; set; }
        public decimal saldoFinal { get; set; }
        public decimal saldoMinimo { get; set; }
        public decimal diferencias { get; set; }
    }

    public class TesMonitoreoDocumentosDto
    {
        public string movimiento { get; set; } = string.Empty;
        public decimal total { get; set; }
    }
}