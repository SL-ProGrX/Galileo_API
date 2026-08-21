namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class FndRecepcionDevolucionesLiqInicializarData
    {
        public string tag_recepcion_devolucion { get; set; } = string.Empty;
    }

    public sealed class FndRecepcionDevolucionesLiqData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public long consec { get; set; }
    }

    public sealed class FndRecepcionDevolucionesLiqAplicarRequest
    {
        public List<long> consecutivos { get; set; } = [];
        public string usuario { get; set; } = string.Empty;
    }

    public sealed class FndRecepcionDevolucionesLiqAplicarData
    {
        public int registros_aplicados { get; set; }
    }
}
