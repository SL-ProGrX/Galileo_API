using Galileo.Models;

namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class AfRecepcionAfiliacionesTagsInicializarResponse
    {
        public string tag_recepcion { get; set; } = string.Empty;
        public string tag_devolucion { get; set; } = string.Empty;
        public string tag_recepcion_devolucion { get; set; } = string.Empty;
        public List<DropDownListaGenericaModel> usuarios { get; set; } = [];
    }

    public sealed class AfRecepcionAfiliacionesTagsMantenimientoResponse
    {
        public bool proceso_ejecutado { get; set; }
    }

    public sealed class AfRecepcionAfiliacionesTagsBoletaResponse
    {
        public long consec { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha_ingreso { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionAfiliacionesTagsAfiliacionResponse
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public long consec { get; set; }
    }

    public sealed class AfRecepcionAfiliacionesTagsPendienteResponse
    {
        public long consec { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionAfiliacionesTagsAplicarItem
    {
        public string cedula { get; set; } = string.Empty;
        public long consec { get; set; }
    }

    public sealed class AfRecepcionAfiliacionesTagsAplicarRequest
    {
        public string movimiento { get; set; } = "RECEPCION";
        public string usuario { get; set; } = string.Empty;
        public List<AfRecepcionAfiliacionesTagsAplicarItem> afiliaciones
        {
            get;
            set;
        } = [];
    }

    public sealed class AfRecepcionAfiliacionesTagsAplicarResponse
    {
        public int registros_aplicados { get; set; }
    }

    public sealed class AfRecepcionAfiliacionesTagsHistorialRequest
    {
        public string? cedula { get; set; }
        public long? documento { get; set; }
        public string? usuario { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
    }

    public sealed class AfRecepcionAfiliacionesTagsHistorialResponse
    {
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
    }
}
