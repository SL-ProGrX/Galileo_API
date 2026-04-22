using Galileo.Models.ProGrX_Contabilidad;

namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class PresAlertaJustificacionGuardarRequest
    {
        public int cod_conta { get; set; } = 0;
        public string cod_modelo { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public int anio { get; set; } = 1900;
        public int mes { get; set; } = 1;
        public string tipo_alerta { get; set; } = string.Empty;
        public string alerta_descripcion { get; set; } = string.Empty;
        public string justificacion { get; set; } = string.Empty;
        public bool justificada { get; set; } = false;
        public string usuario { get; set; } = string.Empty;
        public string cod_tp_justificacion { get; set; } = string.Empty;
        public string justificacion_detalle { get; set; } = string.Empty;
    }

    public class PresAlertaJustificacionBitacoraData
    {
        public int id_bitacora { get; set; } = 0;
        public string accion { get; set; } = string.Empty;
        public bool justificada { get; set; } = false;
        public string justificacion { get; set; } = string.Empty;
        public DateTime fecha_registro { get; set; } = DateTime.MinValue;
        public string usuario_registro { get; set; } = string.Empty;
    }

    public class PresAlertaJustificacionBitRequest
    {
        public int codEmpresa { get; set; } = 0;
        public int codConta { get; set; } = 0;
        public string codModelo { get; set; } = string.Empty;
        public string codUnidad { get; set; } = string.Empty;
        public string codCentroCosto { get; set; } = string.Empty;
        public string codCuenta { get; set; } = string.Empty;
        public int anio { get; set; } = 1900;
        public int mes { get; set; } = 1;
        public string tipoAlerta { get; set; } = string.Empty;
    }

    public class PresVistaPresupuestoAlertasResponse
    {
        public List<PresVistaPresupuestoAlertasData> lista { get; set; } = new();
        public bool permitir_justificar { get; set; } = false;
        public bool usa_exclusiones { get; set; } = false;
        public string mensaje { get; set; } = string.Empty;
    }

}
