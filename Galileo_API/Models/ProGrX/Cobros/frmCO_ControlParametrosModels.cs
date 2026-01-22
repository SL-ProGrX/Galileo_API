namespace Galileo.Models.ProGrX.Cobros
{
    public class CoControlParametrosData
    {
        public string cod_parametro { get; set; } = "";
        public string descripcion { get; set; } = "";
        public string valor { get; set; } = "";
        public bool isNew { get; set; } = false;
    }

    public class CoControlParametrosListaResult
    {
        public int total { get; set; } = 0;
        public List<CoControlParametrosData> lista { get; set; } = new();
    }

    public class CoControlParametrosGuardarRequest
    {
        public string cod_parametro { get; set; } = "";
        public string valor { get; set; } = "";
        public string usuario_sesion { get; set; } = "";
    }
}