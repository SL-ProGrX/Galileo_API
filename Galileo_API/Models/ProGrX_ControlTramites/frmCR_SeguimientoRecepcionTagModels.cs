using System.Text.Json.Serialization;
using Galileo.Models;

namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class CrSeguimientoRecepcionTagInicializarResponse
    {
        public string tag_recepcion { get; set; } =
            string.Empty;

        public string tag_devolucion { get; set; } =
            string.Empty;

        public List<DropDownListaGenericaModel> usuarios
        {
            get;
            set;
        } = [];
    }

    public sealed class CrSeguimientoRecepcionTagOperacionResponse
    {
        public long id_solicitud { get; set; }

        public string codigo { get; set; } =
            string.Empty;

        public string cedula { get; set; } =
            string.Empty;

        public DateTime? fechaforf { get; set; }

        public string descripcion { get; set; } =
            string.Empty;
    }

    public sealed class CrSeguimientoRecepcionTagPendientesRequest
    {
        public string movimiento { get; set; } =
            "RECEPCION";
    }

    public sealed class CrSeguimientoRecepcionTagPendienteResponse
    {
        public long id_solicitud { get; set; }

        public DateTime? fechaforp { get; set; }

        public string cedula { get; set; } =
            string.Empty;

        public string nombre { get; set; } =
            string.Empty;

        public string codigo { get; set; } =
            string.Empty;

        public string descripcion { get; set; } =
            string.Empty;

        public decimal montosol { get; set; }

        public string userfor { get; set; } =
            string.Empty;

        public string usuario_revision { get; set; } =
            string.Empty;

        public long remesa { get; set; }

        public string usuario_remesa { get; set; } =
            string.Empty;
    }

    public sealed class CrSeguimientoRecepcionTagAplicarRequest
    {
        public string movimiento { get; set; } =
            "RECEPCION";

        [JsonIgnore]
        public string usuario { get; set; } =
            string.Empty;

        public List<long> operaciones { get; set; } = [];
    }

    public sealed class CrSeguimientoRecepcionTagAplicarResponse
    {
        public int registros_aplicados { get; set; }
    }

    public sealed class CrSeguimientoRecepcionTagHistorialRequest
    {
        public long? id_solicitud { get; set; }

        public string usuario { get; set; } =
            string.Empty;

        public DateTime? fecha_inicio { get; set; }

        public DateTime? fecha_fin { get; set; }
    }

    public sealed class CrSeguimientoRecepcionTagHistorialResponse
    {
        public string descripcion { get; set; } =
            string.Empty;

        public string notas { get; set; } =
            string.Empty;

        public DateTime? registro_fecha { get; set; }

        public string registro_usuario { get; set; } =
            string.Empty;
    }
}