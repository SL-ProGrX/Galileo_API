namespace Galileo.Models.ProGrX_Procesos
{
    public class CCPlanillaListaData
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class CCPlanillaDetalleData
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
        public bool marcado { get; set; }
    }

    public class CCPlanillaAplicaRequest
    {
        public string modo { get; set; } = string.Empty;
        public string dato { get; set; } = string.Empty;
        public string item { get; set; } = string.Empty;
        public bool marcado { get; set; }
    }

    public class CCPlanillaTodosRequest
    {
        public string modo { get; set; } = string.Empty;
        public string dato { get; set; } = string.Empty;
        public bool todos { get; set; }
    }
}