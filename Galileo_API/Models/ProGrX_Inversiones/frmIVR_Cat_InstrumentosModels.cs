namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class IvrCatInstrumentoData
    {
        public string cod_instrumento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string tipo_form { get; set; } = string.Empty;
        public decimal base_intereses { get; set; }
        public int base_dias { get; set; }
        public bool valoriza { get; set; }
        public int dias_valoriza { get; set; }
        public bool activo { get; set; }
        public bool isNew { get; set; }
    }

    public sealed class IvrCatInstrumentoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public IvrCatInstrumentoData instrumento { get; set; } = new();
    }

    public sealed class IvrCatInstrumentoEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_instrumento { get; set; } = string.Empty;
    }
}
