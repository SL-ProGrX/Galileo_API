namespace Galileo_API.Models.Procesos
{
    /// <summary>
    /// Tipos de proceso registrados en la tabla general GEN_PROCESO.
    /// </summary>
    public static class GenProcesoTipos
    {
        public const string CoAplExc = "COBROS_APL_EXC";
        public const string CoAplFnd = "COBROS_APL_FND";
    }

    /// <summary>
    /// Estados válidos en GEN_PROCESO.
    /// </summary>
    public static class GenProcesoEstados
    {
        public const string Pendiente = "Pendiente";
        public const string EnProceso = "EnProceso";
        public const string Terminado = "Terminado";
        public const string Error = "Error";
    }

    public class GenProcesoIniciadoResult
    {
        public Guid ProcesoId { get; set; }
    }

    public class GenProcesoEstadoResult
    {
        public Guid ProcesoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int ItemsProcesados { get; set; }
        public int ItemsConError { get; set; }
        public string? Mensaje { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
