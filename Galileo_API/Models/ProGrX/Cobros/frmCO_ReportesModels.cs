namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoReporteItemDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CoReporteCodigoDescripcionDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CoReporteCuboProcesarDto
    {
        public string usuario { get; set; } = string.Empty;
    }
}
