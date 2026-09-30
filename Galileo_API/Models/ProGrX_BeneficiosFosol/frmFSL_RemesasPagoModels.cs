namespace Galileo.Models.FSL
{
    public sealed class FslListaPaginadaDto<T>
    {
        public int total { get; set; } = 0;
        public List<T> lista { get; set; } = [];
    }

    public sealed class FslRemesaDto
    {
        public long tesoreria_remesa { get; set; } = 0;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_descripcion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class FslRemesasFiltros
    {
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string filtro { get; set; } = string.Empty;
        public string sort_field { get; set; } = "registro_fecha";
        public int sort_order { get; set; } = -1;
    }

    public sealed class FslRemesaGuardarRequest
    {
        public long cod_remesa { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
    }

    public sealed class FslRemesaCerrarRequest
    {
        public long cod_remesa { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public sealed class FslExpedientesFiltros
    {
        public long cod_remesa { get; set; } = 0;
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string filtro { get; set; } = string.Empty;
        public string sort_field { get; set; } = "cedula";
        public int sort_order { get; set; } = 1;
    }

    public sealed class FslExpedienteRemesaDto
    {
        public string cod_expediente { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal total_sobrante { get; set; } = 0;
        public string presenta_cedula { get; set; } = string.Empty;
        public string presenta_nombre { get; set; } = string.Empty;
    }

    public sealed class FslExpedienteSeleccionDto
    {
        public string cod_expediente { get; set; } = string.Empty;
    }

    public sealed class FslRemesaAplicarRequest
    {
        public long cod_remesa { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public List<FslExpedienteSeleccionDto> casos { get; set; } = [];
    }
}