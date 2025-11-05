namespace PgxAPI.Models.TES
{
    public class TesBancosSaldosMonitoreoDto
    {
        public int id_banco { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string cta { get; set; } = string.Empty;
        public bool monitoreo { get; set; } 
    }

    public class TesBancosSaldosHistoricoDto
    {
        public int idx { get; set; }
        public int id_banco { get; set; }
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
        public decimal saldo_inicial { get; set; }
        public decimal total_debitos { get; set; }
        public decimal total_creditos { get; set; }
        public decimal saldo_final { get; set; }
        public decimal ajuste { get; set; }
        public decimal saldo_minimo { get; set; }
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class TesBancosSaldosCierresDto
    {
        public int id_banco { get; set; }
        public DateTime inicio { get; set; }
        public bool inicio_habilitado { get; set; }
        public DateTime corte { get; set; }
        public decimal saldo_inicial { get; set; }
        public decimal total_debitos { get; set; }
        public decimal total_creditos { get; set; }
        public decimal saldo_final { get; set; }
        public decimal ajuste { get; set; }
        public decimal saldo_minimo { get; set; }
        public string? tipo_cierre { get; set; }
    }

    public class HistoricoFiltros
    {
        public bool todas_fechas { get; set; }
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
    }
}