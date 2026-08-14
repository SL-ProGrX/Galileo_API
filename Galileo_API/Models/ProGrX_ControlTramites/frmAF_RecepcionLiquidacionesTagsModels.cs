using System.Text.Json.Serialization;
using Galileo.Models;

namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class AfRecepcionLiquidacionesTagInicializarResponse
    {
        public string tag_recepcion { get; set; } =
            string.Empty;

        public string tag_devolucion { get; set; } =
            string.Empty;

        public string tag_recepcion_devolucion { get; set; } =
            string.Empty;

        public List<DropDownListaGenericaModel> usuarios
        {
            get;
            set;
        } = [];
    }

    public sealed class AfRecepcionLiquidacionesTagLiquidacionResponse
    {
        public string cedula { get; set; } =
            string.Empty;

        public string nombre { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public long consec { get; set; }
    }

    public sealed class AfRecepcionLiquidacionesTagPendientesRequest
    {
        public string movimiento { get; set; } =
            "RECEPCION";
    }

    public sealed class AfRecepcionLiquidacionesTagPendienteResponse
    {
        public long consec { get; set; }

        public string cedula { get; set; } =
            string.Empty;

        public string nombre { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public DateTime? fecliq { get; set; }

        public string usuario { get; set; } =
            string.Empty;
    }

    public sealed class AfRecepcionLiquidacionesTagAplicarRequest
    {
        public string movimiento { get; set; } =
            "RECEPCION";

        public List<long> boletas { get; set; } = [];

        public string usuario { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionLiquidacionesTagAplicarResponse
    {
        public int registros_aplicados { get; set; }
    }

    public sealed class AfRecepcionLiquidacionesTagHistorialRequest
    {
        public long? documento { get; set; }

        public string usuario { get; set; } =
            string.Empty;

        public DateTime? fecha_inicio { get; set; }

        public DateTime? fecha_fin { get; set; }
    }

    public sealed class AfRecepcionLiquidacionesTagHistorialResponse
    {
        public string descripcion { get; set; } =
            string.Empty;

        public DateTime? registro_fecha { get; set; }

        public string registro_usuario { get; set; } =
            string.Empty;
    }
}