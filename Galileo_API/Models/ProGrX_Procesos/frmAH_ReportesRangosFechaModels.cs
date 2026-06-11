namespace Galileo.Models.AH
{
    public class InstitucionesPatrimonioDto
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class EstadosPersonaPatrimonioDto
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class FrmAhReportesRangosFechaReporteDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FrmAhReportesRangosFechaFiltrosDto
    {
        public List<DropDownListaGenericaModel> estados_persona { get; set; } = [];
        public List<DropDownListaGenericaModel> instituciones { get; set; } = [];
        public List<FrmAhReportesRangosFechaReporteDto> reportes { get; set; } = [];
    }

    public class FrmAhReportesRangosFechaReporteRequest
    {
        public string codigo_reporte { get; set; } = string.Empty;
        public string? cod_estado { get; set; }
        public string? cod_institucion { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string nombre_empresa { get; set; } = string.Empty;
    }

    public class FrmAhReportesRangosFechaReporteResponse
    {
        public string nombre_reporte { get; set; } = string.Empty;
        public string titulo { get; set; } = string.Empty;
        public string titulo_ventana { get; set; } = "Reportes del Modulo de Patrimonio";
        public string sub_titulo { get; set; } = string.Empty;
        public string filtros { get; set; } = string.Empty;
        public string folder { get; set; } = "Patrimonio";
        public string cod_reporte { get; set; } = "P";
        public string usuario { get; set; } = string.Empty;
        public string empresa { get; set; } = string.Empty;
    }

}
