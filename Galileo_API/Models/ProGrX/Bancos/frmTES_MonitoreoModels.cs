namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TES_Monitoreo_BancosDTO
    {
        public int id_banco { get; set; }
        public string idx { get; set; } = string.Empty;
    }

    //public class TES_MonitoreoDTO
    //{
    //    public int id_banco { get; set; }
    //    public string descripcion { get; set; } = string.Empty;
    //    public string cta { get; set; } = string.Empty;
    //    public DateTime corte { get; set; }
    //    public decimal saldo_inicial { get; set; }
    //    public decimal saldo_final { get; set; }
    //    public string ctaconta { get; set; } = string.Empty;
    //    public decimal saldo_minimo { get; set; }
    //    public decimal total_debitos { get; set; } = 0;
    //    public decimal total_creditos { get; set; } = 0;
    //}

    public class TES_MonitoreoDTO
    {
        public int codigoCierre { get; set; }
        public int codigoBanco { get; set; }
        public string descripcionBanco { get; set; }
        public string cuentaBanco { get; set; }
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

    public class TES_Monitoreo_DocumentosDTO
    {
        public string movimiento { get; set; } = string.Empty;
        public decimal total { get; set; }
    }
}
