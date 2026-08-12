using Galileo.Models;

namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class FndRecepcionLiqFondosTagsInicializarResponse
    {
        public string tag_recepcion { get; set; } = string.Empty;

        public string tag_devolucion { get; set; } = string.Empty;

        public string tag_recepcion_devolucion { get; set; } = string.Empty;

        public List<DropDownListaGenericaModel> usuarios { get; set; } = new List<DropDownListaGenericaModel>();
    }

    public sealed class FndRecepcionLiqFondosTagsBoletaResponse
    {
        public int consec { get; set; } = 0;

        public string cod_plan { get; set; } = string.Empty;

        public string cod_contrato { get; set; } = string.Empty;

        public string cedula { get; set; } = string.Empty;

        public string nombre { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class FndRecepcionLiqFondosTagsPendientesRequest
    {
        public string movimiento { get; set; } = "RECEPCION";

        public string caso { get; set; } = "TODOS";

        public string usuario { get; set; } = string.Empty;
    }

    public sealed class FndRecepcionLiqFondosTagsPendienteResponse
    {
        public int consec { get; set; } = 0;

        public string cod_plan { get; set; } = string.Empty;

        public string cod_contrato { get; set; } = string.Empty;

        public string cedula { get; set; } = string.Empty;

        public string nombre { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;

        public DateTime? fecha { get; set; }

        public string usuario { get; set; } = string.Empty;
    }

    public sealed class FndRecepcionLiqFondosTagsAplicarRequest
    {
        public string movimiento { get; set; } = "RECEPCION";

        public string usuario { get; set; } = string.Empty;

        public List<long> consecutivos { get; set; } = [];
    }

    public sealed class FndRecepcionLiqFondosTagsAplicarResponse
    {
        public int registros_aplicados { get; set; }
    }

    public sealed class FndRecepcionLiqFondosTagsHistorialResponse
    {
        public string descripcion { get; set; } = string.Empty;

        public string notas { get; set; } = string.Empty;

        public DateTime? registro_fecha { get; set; }

        public string registro_usuario { get; set; } = string.Empty;
    }

    public sealed class FndRecepcionLiqFondosTagsHistorialRequest
    {
        public long? numero_boleta { get; set; }

        public string usuario { get; set; } = string.Empty;

        public DateTime? fecha_inicio { get; set; }

        public DateTime? fecha_fin { get; set; }
    }
}