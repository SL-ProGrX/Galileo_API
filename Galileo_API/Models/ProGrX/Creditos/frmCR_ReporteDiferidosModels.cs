using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrReporteDiferidosConsultaRequest
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string codigo { get; set; } = string.Empty;
    }

    public class CrReporteDiferidosItem
    {
        public long id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public decimal monto_calculo { get; set; } = 0;
        public DateTime? fechaforp { get; set; }
        public DateTime? fecha_calculo_int { get; set; }
        public int dias_total { get; set; } = 0;
        public decimal total_dif { get; set; } = 0;
        public DateTime? fecha_corte { get; set; }
        public int dias_corte { get; set; } = 0;
        public decimal dif_corte { get; set; } = 0;
        public int dias_acumulados { get; set; } = 0;
        public decimal dif_acumulado { get; set; } = 0;
        public decimal tasa { get; set; } = 0;
    }

    internal sealed class CrReporteDiferidosOperacionBase
    {
        public long id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public decimal? montocalculo { get; set; }
        public DateTime? fechaforp { get; set; }
        public DateTime? fecha_calculo_int { get; set; }
        public decimal @int { get; set; } = 0;
    }
}