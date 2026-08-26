namespace Galileo.Models.TES
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
        public decimal cheques_pendientes { get; set; }
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

    public class TesBancosSaldosCargaMasivaDto
    {
        public int? linea { get; set; }
        public string codigo_cuenta { get; set; } = string.Empty;
        public string nombre_cuenta { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public decimal saldo_inicial { get; set; }
        public decimal saldo_final { get; set; }
    }

    public class TesBancosSaldosCargaMasivaRequest
    {
        public string tipo_cierre { get; set; } = string.Empty;
        public List<TesBancosSaldosCargaMasivaDto> registros { get; set; } = new();
    }

    public class TesBancosSaldosCargaMasivaResult
    {
        public int registros_insertados { get; set; }
        public int registros_actualizados { get; set; }
        public int registros_error { get; set; }
        public List<TesBancosSaldosCargaMasivaErrorDto> errores { get; set; } = new();
    }

    public class TesBancosSaldosCargaMasivaErrorDto
    {
        public int linea { get; set; }
        public string codigo_cuenta { get; set; } = string.Empty;
        public string nombre_cuenta { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    public class HistoricoFiltros
    {
        public bool todas_fechas { get; set; }
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
    }
}
