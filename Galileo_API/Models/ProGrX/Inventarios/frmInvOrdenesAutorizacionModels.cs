namespace Galileo.Models.INV
{
    public sealed class ResolucionTransaccionDto
    {
        public string cod_orden { get; set; } = string.Empty;

        public string tipo_orden { get; set; } = string.Empty;

        public decimal total { get; set; } = 0;

        public string user_solicita { get; set; } = string.Empty;

        public DateTime? fecha { get; set; }

        public string causa { get; set; } = string.Empty;

        public string nota { get; set; } = string.Empty;

        public string proceso { get; set; } = string.Empty;

        public bool seleccionado { get; set; } = false;
    }

    public sealed class InvOrdenesAutorizacionFiltros
    {
        public DateTime? fecha_inicio { get; set; }

        public DateTime? fecha_corte { get; set; }

        public string tipo { get; set; } = string.Empty;

        public string usuario { get; set; } = string.Empty;

        public bool todas_fechas { get; set; } = false;
    }

    public sealed class InvOrdenesAutorizacionProcesarRequest
    {
        public string usuario { get; set; } = string.Empty;

        public List<ResolucionTransaccionDto> ordenes { get; set; } = [];
    }
}