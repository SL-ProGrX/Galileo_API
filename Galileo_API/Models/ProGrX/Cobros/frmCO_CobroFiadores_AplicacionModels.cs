namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoCobroFiadoresAplicacionPendientesDto
    {
        public int pendientes { get; set; }
    }
    public class CoCobroFiadoresAplicacionProcesarRequest
    {
        public string usuario_sesion { get; set; } = string.Empty;
    }
    public class CoCobroFiadoresAplicacionProcesarResponse
    {
        public int pendientes_iniciales { get; set; }
        public int pendientes_finales { get; set; }
        public int iteraciones { get; set; }
        public bool completado { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }
    public class CoCobroFiadoresAplicacionCancelarRequest
    {
        public string usuario_sesion { get; set; } = string.Empty;
    }
}