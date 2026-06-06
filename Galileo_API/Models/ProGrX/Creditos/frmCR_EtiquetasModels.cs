namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrEtiquetaData
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_requisito { get; set; } = string.Empty;
        public string requisito_descripcion { get; set; } = string.Empty;
        public int nota_largo { get; set; } = 0;
        public bool espera_activa { get; set; } = false;
        public bool espera_desactiva { get; set; } = false;
        public bool activo { get; set; } = false;
    }

    public class CrEtiquetaGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public CrEtiquetaData etiqueta { get; set; } = new();
    }

    public class CrEtiquetaEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string tag_codigo { get; set; } = string.Empty;
    }

    public class CrEtiquetaNotificacionData
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string para_tag { get; set; } = string.Empty;
        public string para_email { get; set; } = string.Empty;
        public string cc_tag { get; set; } = string.Empty;
        public string cc_email { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    public class CrEtiquetaNotificacionGuardarRequest
    {
        public CrEtiquetaNotificacionData notificacion { get; set; } = new();
    }

    public class CrEtiquetaNotificacionEliminarRequest
    {
        public string tag_codigo { get; set; } = string.Empty;
    }
}