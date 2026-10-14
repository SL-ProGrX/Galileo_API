namespace Galileo_API.Models.ProGrX.Convenios
{
    public class CrConvenioTipoData
    {
        public string tipo_convenio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public bool isNew { get; set; } = false;
    }

    public class CrConvenioTipoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public CrConvenioTipoData tipo { get; set; } = new();
    }

    public class CrConvenioTipoEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string tipo_convenio { get; set; } = string.Empty;
    }
}
