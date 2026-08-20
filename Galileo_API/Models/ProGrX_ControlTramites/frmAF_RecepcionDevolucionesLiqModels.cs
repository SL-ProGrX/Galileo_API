using System.Text.Json.Serialization;

namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class AfRecepcionDevolucionesLiqInicializarData
    {
        public string tag_recepcion_devolucion { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionDevolucionesLiqData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public long consec { get; set; }
    }

    public sealed class AfRecepcionDevolucionesLiqAplicarRequest
    {
        public List<long> consecutivos { get; set; } = [];

        public string usuario { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionDevolucionesLiqAplicarData
    {
        public int registros_aplicados { get; set; }
    }
}