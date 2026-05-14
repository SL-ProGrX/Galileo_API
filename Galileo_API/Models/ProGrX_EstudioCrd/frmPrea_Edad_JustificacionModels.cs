namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaEdadJustificacionCargarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaEdadJustificacionCargarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public int edad_aplica { get; set; } = 0;
        public string edad_justificacion { get; set; } = string.Empty;
        public int edad_cuotas { get; set; } = 0;
    }

    public class FrmPreaEdadJustificacionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public int edad_aplica { get; set; } = 1;
        public string edad_justificacion { get; set; } = string.Empty;
        public int edad_cuotas { get; set; } = 0;
    }

    public class FrmPreaEdadJustificacionGuardarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public int edad_aplica { get; set; } = 0;
        public string edad_justificacion { get; set; } = string.Empty;
        public int edad_cuotas { get; set; } = 0;
        public string mensaje { get; set; } = string.Empty;
    }

    internal class FrmPreaEdadJustificacionData
    {
        public int edad_aplica { get; set; } = 0;
        public string edad_justificacion { get; set; } = string.Empty;
        public int edad_cuotas { get; set; } = 0;
    }
}
