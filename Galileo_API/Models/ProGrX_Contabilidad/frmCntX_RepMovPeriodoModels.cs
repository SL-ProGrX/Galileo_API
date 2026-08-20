namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntxRepMovPeriodoPeriodoDto
    {
        public int item { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class CntxRepMovPeriodoFiltroDto
    {
        public string? tipo { get; set; }
        public int? periodo { get; set; }
        public string? unidad { get; set; }
        public string? centroCosto { get; set; }
        public string? area { get; set; }
        public string? reporte { get; set; }
        public string? nivel { get; set; }
        public string? mostrar { get; set; }
        public string? usuario { get; set; }
    }

    public class PeriodoInicioRow
    {
        public int inicio_mes { get; set; } = 1;
        public int inicio_anio { get; set; } = 1990;
    }

    public class MovimientoPeriodoRow
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public decimal movimiento { get; set; } = 0;
    }
}
