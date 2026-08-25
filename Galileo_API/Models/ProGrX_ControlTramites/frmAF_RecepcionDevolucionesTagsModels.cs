namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class AfRecepcionDevolucionesTagsInicializarData
    {
        public string Tag_Aplicado { get; set; } = string.Empty;
        public string Tag_Devolucion { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionDevolucionesTagsData
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public long Consec { get; set; }
    }

    public sealed class AfRecepcionDevolucionesTagsAplicarItem
    {
        public string Cedula { get; set; } = string.Empty;
        public long Consec { get; set; }
    }

    public sealed class AfRecepcionDevolucionesTagsAplicarRequest
    {
        public List<AfRecepcionDevolucionesTagsAplicarItem> Items { get; set; } = [];

        public string Usuario { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionDevolucionesTagsAplicarData
    {
        public int Registros_Aplicados { get; set; }
    }
}
