namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class PresAlertaJustificacionGuardarRequest
    {
        public int cod_conta { get; set; }
        public string cod_modelo { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public int anio { get; set; }
        public int mes { get; set; }
        public string tipo_alerta { get; set; } = string.Empty;
        public string alerta_descripcion { get; set; } = string.Empty;
        public string justificacion { get; set; } = string.Empty;
        public bool justificada { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class PresAlertaJustificacionBitacoraData
    {
        public int id_bitacora { get; set; }
        public string accion { get; set; } = string.Empty;
        public bool justificada { get; set; }
        public string justificacion { get; set; } = string.Empty;
        public DateTime fecha_registro { get; set; }
        public string usuario_registro { get; set; } = string.Empty;
    }
}
