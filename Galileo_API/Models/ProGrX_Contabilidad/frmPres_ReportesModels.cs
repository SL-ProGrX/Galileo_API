namespace Galileo.Models.PRES
{
    public class PresPeriodoRequest
    {
        public int Inicio_Anio { get; set; }
        public int Inicio_Mes { get; set; }
        public int Corte_Anio { get; set; }
        public int Corte_Mes { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class PresFiltrosReportes
    {
        public required string tipoInforme { get; set; }
        public required int contabilidad { get; set; }
        public required string unidadNegocio { get; set; }
        public required string centroCosto { get; set; }
        public required string modelo { get; set; }
        public string? periodo { get; set; }
        public string? tiposAjuste { get; set; }
        public bool cuentasResumen { get; set; } = false;

        public string? tipoReporte { get; set; }
        public string? nivelReporte { get; set; }

        public bool chkCalculaEstadosPreliminares { get; set; } = false;
        public bool chkNoMostrarCeros { get; set; } = false;
        public bool chkMostrarTitulos { get; set; } = false;
        public bool chkCuentasOrden { get; set; } = false;
        public bool chkFormatoNumCuentas { get; set; } = false;
    }

    public class PresReportesIndicadoresData
    {
        public decimal? Tipo_Cambio { get; set; }
        public double? Tasa_Basica_Pasiva { get; set; }
        public double? Indice_Inflacion { get; set; }
    }
}