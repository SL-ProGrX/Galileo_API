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
        public string? usuario { get; set; } = string.Empty;
        public string? modulo { get; set; } = string.Empty;
        public string? fuente { get; set; } = string.Empty;
        public string? detalle { get; set; } = string.Empty;

        public string? appNombre { get; set; } = string.Empty;
        public string? appVersion { get; set; } = string.Empty;
        public string? logEquipo { get; set; } = string.Empty;
        public string? logIP { get; set; } = string.Empty;
        public string? mac { get; set; } = string.Empty;
    }

    public class MovimientoLogDto
    {
        public int Consecutivo { get; set; }

        // rs!ModuloDesc
        public string ModuloDesc { get; set; } = string.Empty;

        // rs!UsuarioNombre
        public string UsuarioNombre { get; set; } = string.Empty;

        // rs!Usuario
        public string Usuario { get; set; } = string.Empty;

        // rs!Movimiento
        public string Movimiento { get; set; } = string.Empty;

        // rs!Fecha_FORMAT
        // Si ya viene formateada desde SQL
        public string Fecha_FORMAT { get; set; } = string.Empty;

        // rs!Detalle
        public string Detalle { get; set; } = string.Empty;

        // rs!App_Nombre
        public string App_Nombre { get; set; } = string.Empty;

        // rs!App_Version
        public string App_Version { get; set; } = string.Empty;

        // rs!App_Equipo
        public string App_Equipo { get; set; } = string.Empty;

        // rs!Equipo_MAC
        public string Equipo_MAC { get; set; } = string.Empty;

        // rs!App_IP
        public string App_IP { get; set; } = string.Empty;
    }

}
