namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaNotificacionCargarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public long id_solicitud { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
    }

    public class FrmPreaNotificacionCargarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public long id_solicitud { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre_asociado { get; set; } = string.Empty;

        public string estado { get; set; } = string.Empty;
        public string estado_codigo { get; set; } = string.Empty;

        public decimal monto_aprobado { get; set; } = 0;
        public decimal monto_sugerido { get; set; } = 0;

        public string tiquete { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public string celular { get; set; } = string.Empty;

        public bool permite_notificar { get; set; } = false;
        public string mensaje_validacion { get; set; } = string.Empty;
    }

    public class FrmPreaNotificacionEnviarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public long id_solicitud { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;

        public decimal monto_sugerido { get; set; } = 0;
        public string tiquete { get; set; } = string.Empty;
    }

    public class FrmPreaNotificacionEnviarResponse
    {
        public string resultado_notificacion { get; set; } = string.Empty;
        public string tipo_plantilla { get; set; } = string.Empty;
        public string estado_envio { get; set; } = string.Empty;

        public bool envio_correo { get; set; } = false;
        public bool envio_sms { get; set; } = false;

        public string correo { get; set; } = string.Empty;
        public string celular { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    public class FrmPreaNotificacionInfoInterna
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public long id_solicitud { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre_asociado { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal monto_aprobado { get; set; } = 0;
        public decimal monto_sugerido { get; set; } = 0;
        public string tiquete { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public string celular { get; set; } = string.Empty;
    }

    public class FrmPreaNotificacionPlantillaDatos
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre_asociado { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_codigo { get; set; } = string.Empty;
        public decimal monto_aprobado { get; set; } = 0;
        public decimal monto_sugerido { get; set; } = 0;
        public string tiquete { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public string celular { get; set; } = string.Empty;
    }

    public class FrmPreaPlantillaCorreoDto
    {
        public string asunto { get; set; } = string.Empty;
        public string plantilla { get; set; } = string.Empty;
    }

    public class FrmPreaPlantillaMensajeDto
    {
        public string mensaje { get; set; } = string.Empty;
    }

    public class FrmPreaPlantillaSmsDto
    {
        public string mensaje_sms { get; set; } = string.Empty;
    }
}
