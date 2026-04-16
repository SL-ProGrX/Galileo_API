using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoControlReporteItemDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string grupo { get; set; } = string.Empty;
    }

    public class CoControlReportesFiltrosDto
    {
        public List<DropDownListaGenericaModel> estadosPersona { get; set; } = new();
        public List<DropDownListaGenericaModel> gestiones { get; set; } = new();
        public List<DropDownListaGenericaModel> usuarios { get; set; } = new();
        public List<DropDownListaGenericaModel> tiposSalida { get; set; } = new();
    }

    public class CoControlReportesRequestDto
    {
        public string codigoReporte { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string? codEstadoPersona { get; set; }
        public string? codGestion { get; set; }
        public string? usuario { get; set; }
        public bool todasFechas { get; set; }
        public string? fechaInicio { get; set; }
        public string? fechaCorte { get; set; }
        public string usuarioEjecuta { get; set; } = string.Empty;
    }

    public class CoControlReportesCuboRequestDto
    {
        public bool todasFechas { get; set; }
        public string? fechaInicio { get; set; }
        public string? fechaCorte { get; set; }
        public string usuarioEjecuta { get; set; } = string.Empty;
    }
}
