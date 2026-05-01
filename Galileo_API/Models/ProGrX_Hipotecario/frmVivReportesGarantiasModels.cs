namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class FrmVivReportesGarantiasModels
    {
        public class VivReporteGarantiasRequest
        {
            public int TabIndex { get; set; }

            public string FechaInicio { get; set; } = string.Empty;

            public string FechaCorte { get; set; } = string.Empty;

            public string Usuario { get; set; } = string.Empty;

            public string NombreEmpresa { get; set; } = string.Empty;

            public string OpcionReporte { get; set; } = string.Empty;

            public bool ContactosDetallado { get; set; } = false;

            public bool ZonasDetallado { get; set; } = false;

            public bool TramitesPendientesTodos { get; set; } = false;

            public bool IncluirTodos { get; set; } = false;

            public string? Tipo { get; set; }

            public int? IdContacto { get; set; }

            public int? IdEmpresa { get; set; }

            public int? IdZona { get; set; }

            public string? TipoContacto { get; set; }

            public string? Estado { get; set; }
        }

        public class VivReporteGarantiasProdAcumRequest
        {
            public DateTime FechaCorte { get; set; }

            public string Usuario { get; set; } = string.Empty;

            public string NombreEmpresa { get; set; } = string.Empty;
        }

        public class VivReporteGarantiasResponse
        {
            public string Reporte { get; set; } = string.Empty;

            public string Titulo { get; set; } = string.Empty;

            public string SubTitulo { get; set; } = string.Empty;

            public string FechaDesde { get; set; } = string.Empty;

            public string FechaCorte { get; set; } = string.Empty;

            public string Empresa { get; set; } = string.Empty;

            public string Fecha { get; set; } = string.Empty;

            public string Usuario { get; set; } = string.Empty;

            public string SelectionFormula { get; set; } = string.Empty;

            public List<string> StoredProcParams { get; set; } = new();
        }
    }
}
