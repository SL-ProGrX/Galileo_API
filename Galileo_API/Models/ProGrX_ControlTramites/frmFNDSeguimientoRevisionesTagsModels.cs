namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class FndSeguimientoRevisionFondoData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public int cod_contrato { get; set; } = 0;
        public string cod_plan { get; set; } = string.Empty;
        public int cod_operadora { get; set; } = 0;
    }

    public sealed class FndSeguimientoRevisionClaveRequest
    {
        public string cedula { get; set; } = string.Empty;
        public int cod_contrato { get; set; } = 0;
        public string cod_plan { get; set; } = string.Empty;
        public int cod_operadora { get; set; } = 0;
    }

    public sealed class FndSeguimientoRevisionDetalleData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int cod_operadora { get; set; } = 0;
        public string operadora { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string plan { get; set; } = string.Empty;
        public int cod_contrato { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public string estado_descripcion { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public decimal monto { get; set; } = 0M;
        public int plazo { get; set; } = 0;
        public string renueva { get; set; } = string.Empty;
        public string renueva_descripcion { get; set; } = string.Empty;
        public decimal inc_anual { get; set; } = 0M;
        public string inc_tipo { get; set; } = string.Empty;
        public string inc_tipo_descripcion { get; set; } = string.Empty;
        public decimal aportes { get; set; } = 0M;
        public decimal rendimiento { get; set; } = 0M;
        public decimal total { get; set; } = 0M;
        public string operacion { get; set; } = string.Empty;
    }

    public sealed class FndSeguimientoRevisionRegistroData
    {
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public sealed class FndSeguimientoRevisionOmisionData
    {
        public int id_error { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public int? asignado { get; set; }
        public string aplicado { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
        public int? linea_err { get; set; }
        public bool seleccionado { get; set; } = false;
    }

    public sealed class FndSeguimientoRevisionOmisionCambiarRequest
    {
        public string cedula { get; set; } = string.Empty;
        public int cod_contrato { get; set; } = 0;
        public string cod_plan { get; set; } = string.Empty;
        public int id_error { get; set; } = 0;
        public bool seleccionado { get; set; } = false;
        public string usuario { get; set; } = string.Empty;
    }

    public sealed class FndSeguimientoRevisionOmisionCambiarData
    {
        public int? linea_err { get; set; }
        public bool seleccionado { get; set; } = false;
    }

    public sealed class FndSeguimientoRevisionAplicarRequest
    {
        public string cedula { get; set; } = string.Empty;
        public int cod_contrato { get; set; } = 0;
        public string cod_plan { get; set; } = string.Empty;
        public int cod_operadora { get; set; } = 0;
        public string tag_codigo { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}