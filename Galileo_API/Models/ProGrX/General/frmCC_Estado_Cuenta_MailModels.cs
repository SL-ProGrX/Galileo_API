using Galileo.Models;

namespace Galileo_API.Models.ProGrX.General
{
    public sealed class CcEstadoCuentaMailInicialData
    {
        public string email { get; set; } = string.Empty;
        public List<DropDownListaGenericaModel> periodos { get; set; } = new();
    }

    public sealed class CcEstadoCuentaMailEnviarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public DateTime? fecha_corte { get; set; }
    }

    internal sealed class CcEstadoCuentaMailPeriodoData
    {
        public object? idx { get; set; }
        public string itmx { get; set; } = string.Empty;
    }
}