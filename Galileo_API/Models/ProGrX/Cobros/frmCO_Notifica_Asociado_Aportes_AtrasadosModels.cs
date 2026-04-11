namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoNotificaAsociadoAportesAtrasadosData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public int obrero_pend { get; set; }
        public int aporte_pend { get; set; }
    }

    public class CoNotificaAsociadoAportesAtrasadosListaResult
    {
        public int total { get; set; }
        public List<CoNotificaAsociadoAportesAtrasadosData> lista { get; set; } = new();
    }

    public class CoNotificaAsociadoAportesAtrasadosEnviarRequest
    {
        public List<string> cedulas { get; set; } = new();
        public string usuario_sesion { get; set; } = string.Empty;
    }
}