using Galileo.Models;

namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class AfRecepcionBeneficiosTagsInicializarResponse
    {
        public string tag_recepcion { get; set; } = string.Empty;
        public string tag_devolucion { get; set; } = string.Empty;
        public string tag_recepcion_devolucion { get; set; } = string.Empty;
        public List<DropDownListaGenericaModel> beneficios { get; set; } = [];
        public List<DropDownListaGenericaModel> usuarios { get; set; } = [];
    }

    public sealed class AfRecepcionBeneficiosTagsBeneficioResponse
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public long consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionBeneficiosTagsPendienteResponse
    {
        public long consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? registra_fecha { get; set; }
        public string registra_user { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionBeneficiosTagsAplicarItem
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public long consec { get; set; }
    }

    public sealed class AfRecepcionBeneficiosTagsAplicarRequest
    {
        public string movimiento { get; set; } = "RECEPCION";
        public string usuario { get; set; } = string.Empty;
        public List<AfRecepcionBeneficiosTagsAplicarItem> beneficios
        {
            get;
            set;
        } = [];
    }

    public sealed class AfRecepcionBeneficiosTagsAplicarResponse
    {
        public int registros_aplicados { get; set; }
    }

    public sealed class AfRecepcionBeneficiosTagsHistorialRequest
    {
        public string? cod_beneficio { get; set; }
        public long? consec { get; set; }
        public string? usuario { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
    }

    public sealed class AfRecepcionBeneficiosTagsHistorialResponse
    {
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
    }
}
