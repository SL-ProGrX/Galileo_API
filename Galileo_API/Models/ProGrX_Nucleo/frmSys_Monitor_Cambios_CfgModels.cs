namespace Galileo_API.Models.ProGrX_Nucleo
{
    public class MonitorCambiosCfgModulosDto
    {
        public string nombre { get; set; } = string.Empty;
        public string modulo { get; set; } = string.Empty;
    }

    public class MonitorCambiosCfgFiltros
    {
        public bool chkFechas { get; set; } = false;
        public bool chkHoras { get; set; } = false;
        public DateTime dtpInicio { get; set; } = DateTime.Now;
        public DateTime dtpCorte { get; set; } = DateTime.Now;
        public string usuario { get; set; } = string.Empty;
        public string modulo { get; set; } = string.Empty;
        public string fuente { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;

        public string appNombre { get; set; } = string.Empty;
        public string appVersion { get; set; } = string.Empty;
        public string logEquipo { get; set; } = string.Empty;
        public string logIP { get; set; } = string.Empty;
        public string mac { get; set; } = string.Empty;
    }

}
