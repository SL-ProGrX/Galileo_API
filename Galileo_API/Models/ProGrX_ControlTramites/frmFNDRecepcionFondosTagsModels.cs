using Galileo.Models;

namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class FndRecepcionFondosTagsInicializarResponse
    {
        public string tag_recepcion { get; set; } = string.Empty;
        public string tag_devolucion { get; set; } = string.Empty;
        public string tag_recepcion_devolucion { get; set; } = string.Empty;
        public List<DropDownListaGenericaModel> usuarios { get; set; } = [];
    }

    public sealed class FndRecepcionFondosTagsContratoResponse
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public int cod_operadora { get; set; }
        public long cod_contrato { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    public sealed class FndRecepcionFondosTagsContratoBusquedaResponse
    {
        public long cod_contrato { get; set; }
        public int cod_operadora { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public sealed class FndRecepcionFondosTagsPendienteResponse
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public string usuario { get; set; } = string.Empty;
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public long cod_contrato { get; set; }
    }

    public sealed class FndRecepcionFondosTagsAplicarItem
    {
        public string cod_plan { get; set; } = string.Empty;
        public long cod_contrato { get; set; }
    }

    public sealed class FndRecepcionFondosTagsAplicarRequest
    {
        public string movimiento { get; set; } = "RECEPCION";
        public string usuario { get; set; } = string.Empty;
        public List<FndRecepcionFondosTagsAplicarItem> contratos { get; set; } = [];
    }

    public sealed class FndRecepcionFondosTagsAplicarResponse
    {
        public int registros_aplicados { get; set; }
    }

    public sealed class FndRecepcionFondosTagsHistorialRequest
    {
        public string? cod_plan { get; set; }
        public long? cod_contrato { get; set; }
        public string? usuario { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
    }

    public sealed class FndRecepcionFondosTagsHistorialResponse
    {
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }
}
