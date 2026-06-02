namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrCausasSeguimientoData
    {
        public string cod_causas { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool estado { get; set; } = false;
    }

    public class CrCausasSeguimientoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public CrCausasSeguimientoData causa { get; set; } = new();
    }

    public class CrCausasSeguimientoEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string cod_causas { get; set; } = string.Empty;
    }
}