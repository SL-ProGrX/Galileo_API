namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class AfBenSeguimientoBeneficioData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string registra_user { get; set; } = string.Empty;
        public DateTime? registra_fecha { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; } = 0;
    }

    public sealed class AfBenSeguimientoClaveRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; } = 0;
    }

    public sealed class AfBenSeguimientoRegistroData
    {
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public sealed class AfBenSeguimientoOmisionData
    {
        public int id_error { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public int? asignado { get; set; }
        public string aplicado { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
        public int? linea_err { get; set; }
        public bool seleccionado { get; set; } = false;
    }

    public sealed class AfBenSeguimientoOmisionCambiarRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; } = 0;
        public int id_error { get; set; } = 0;
        public bool seleccionado { get; set; } = false;
        public string usuario { get; set; } = string.Empty;
    }

    public sealed class AfBenSeguimientoOmisionCambiarData
    {
        public int? linea_err { get; set; }
        public bool seleccionado { get; set; } = false;
    }

    public sealed class AfBenSeguimientoAplicarRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; } = 0;
        public string tag_codigo { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}