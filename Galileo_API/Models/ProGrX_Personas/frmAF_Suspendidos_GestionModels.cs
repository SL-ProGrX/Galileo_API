namespace Galileo.Models.ProGrX_Personas
{
    public class AfSuspendidosBitacoraDto
    {
        public int id_bitacora { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string revisado_usuario { get; set; } = string.Empty;
        public DateTime revisado_fecha { get; set; }
    }

    public class AfSuspendidosGestionFiltros
    {
        public bool todas_fechas { get; set; }
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
        public string cedula { get; set; } = string.Empty;
    }

    public class AfSuspendidosArchivoDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }
}
