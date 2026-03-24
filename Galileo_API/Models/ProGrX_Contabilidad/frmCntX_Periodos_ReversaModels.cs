namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXPeriodosData
    {
        public DateTime periodo_corte { get; set; } = DateTime.Now;
        public string estado { get; set; } = string.Empty;
        public DateTime? cierre_fecha { get; set; }
        public string cierre_usuario { get; set; } = string.Empty;
    }

    public class CntXPeriodosLogData
    {
        public DateTime? corte { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class ReversaPeriodoRequest
    {
        public int codigo_contabilidad { get; set; } = 0; 
        public DateTime cierre { get; set; } = DateTime.Now;
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}
