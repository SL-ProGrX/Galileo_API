namespace Galileo.Models.INV
{
    /// <summary>
    /// Datos recibidos desde la ventana Orden del Reporte (frmInvTransacReporteOrden).
    /// Equivale a InvTransacRep del VB6 más el orden del detalle seleccionado.
    /// </summary>
    public class InvTransacReporteOrdenRequest
    {
        public string boleta { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string? reporte { get; set; }
        public string? orden { get; set; }
    }

    /// <summary>
    /// Configuración del reporte de boleta resuelta por el API.
    /// </summary>
    public class InvTransacReporteOrdenReporteDto
    {
        public string nombre_reporte { get; set; } = string.Empty;
        public string titulo { get; set; } = string.Empty;
        public string boleta { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string orden { get; set; } = string.Empty;
    }
}
