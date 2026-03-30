using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Polizas
{
    public class FrmCRPolizasReportesModels
    {
        public class ReporteListaModel
        {
            public string CodigoReporte { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
        }

        public class CrdPolizasReportesInicializarResponse
        {
            public DateTime FechaServidor { get; set; }
            public bool EsAseVersion { get; set; }
            public string LabelDepartamento { get; set; } = "Departamento";
            public string LabelSeccion { get; set; } = "Sección";

            public List<ReporteListaModel> Reportes { get; set; } = new();
            public List<DropDownListaGenericaModel> Instituciones { get; set; } = new();
            public List<DropDownListaGenericaModel> Nacionalidades { get; set; } = new();
            public List<DropDownListaGenericaModel> EstadosCiviles { get; set; } = new();
            public List<DropDownListaGenericaModel> TiposId { get; set; } = new();
            public List<DropDownListaGenericaModel> Provincias { get; set; } = new();
            public List<DropDownListaGenericaModel> Divisas { get; set; } = new();
            public List<DropDownListaGenericaModel> Polizas { get; set; } = new();
            public List<DropDownListaGenericaModel> EstadosPersona { get; set; } = new();
            public List<DropDownListaGenericaModel> EstadosLaborales { get; set; } = new();
            public List<DropDownListaGenericaModel> Sexos { get; set; } = new();
            public List<DropDownListaGenericaModel> FechasTipo { get; set; } = new();
        }

        public class CrdPolizasLineaModel
        {
            public string Codigo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string Poliza { get; set; } = string.Empty;
            public string Retencion { get; set; } = string.Empty;
        }
        public class CrdPolizasReportesSocioModel
        {            public string Cedula { get; set; } = string.Empty;
            public string CedulaAlterna { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

        public class CrdPolizaReporteMetadataModel
        {
            public string CodigoPoliza { get; set; } = string.Empty;
            public string CodigoRetencion { get; set; } = string.Empty;
            public int Prendaria { get; set; }
            public string Descripcion { get; set; } = string.Empty;
        }
        public class CrdPolizasReportesRequest
        {
            public string CodigoReporte { get; set; } = string.Empty; // R001, R002, R003
            public bool Resumen { get; set; } = false;

            public string PolizaCodigo { get; set; } = "TODOS";
            public string? LineaCodigo { get; set; }
            public string? Cedula { get; set; }
            public decimal? Operacion { get; set; }

            public DateTime? FechaProceso { get; set; }
            public DateTime? FechaMovimientoInicio { get; set; }
            public DateTime? FechaMovimientoFin { get; set; }
            public DateTime? CoberturaVenceInicio { get; set; }
            public DateTime? CoberturaVenceFin { get; set; }
            public DateTime? FechaNacimientoInicio { get; set; }
            public DateTime? FechaNacimientoFin { get; set; }

            public bool FiltrarProceso { get; set; } = true;
            public bool FiltrarFechasMovimiento { get; set; } = true;
            public bool FiltrarCoberturaVence { get; set; } = true;
            public bool FiltrarNacimiento { get; set; } = true;
            public bool FiltrarOperacion { get; set; } = true;
            public bool FiltrarProvincia { get; set; } = true;
            public bool FiltrarCanton { get; set; } = true;
            public bool FiltrarDistrito { get; set; } = true;

            public string? EstadoPersona { get; set; }
            public string? EstadoCivil { get; set; }
            public string? EstadoLaboral { get; set; }
            public string? Institucion { get; set; }
            public string? TipoId { get; set; }
            public string? Divisa { get; set; }
            public string? Nacionalidad { get; set; }
            public string? Sexo { get; set; }

            public string? DepartamentoCodigo { get; set; }
            public string? SeccionCodigo { get; set; }

            public string? Provincia { get; set; }
            public string? Canton { get; set; }
            public string? Distrito { get; set; }

            public bool EsAseVersion { get; set; } = true;
        }

        public class CrdPolizasReporteConfigResponse
        {
            public string ReporteNombre { get; set; } = string.Empty;
            public string ReporteTitulo { get; set; } = string.Empty;
            public string SubTitulo { get; set; } = string.Empty;
            public string SelectionFormula { get; set; } = string.Empty;

            public string FormulaFecha { get; set; } = string.Empty;
            public string FormulaEmpresa { get; set; } = string.Empty;
            public string FormulaUsuario { get; set; } = string.Empty;
            public string FormulaTitulo { get; set; } = string.Empty;
            public string FormulaSubTitulo { get; set; } = string.Empty;

            public bool EsResumen { get; set; } = false;
        }

    }
}
