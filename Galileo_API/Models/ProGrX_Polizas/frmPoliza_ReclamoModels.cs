namespace Galileo_API.Models.ProGrX_Polizas
{
    public class PolizaReclamoFormularioResponse
    {
        public int reclamoId { get; set; }
        public int operacionId { get; set; }
        public int polizaId { get; set; }
        public string polizaCodigo { get; set; } = string.Empty;

        public string cedula { get; set; } = string.Empty;
        public string apellido1 { get; set; } = string.Empty;
        public string apellido2 { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? fechaNacimiento { get; set; }
        public string sexo { get; set; } = string.Empty;

        public string estadoDescripcion { get; set; } = string.Empty;
        public string polizaDescripcion { get; set; } = string.Empty;
        public string tipoPoliza { get; set; } = string.Empty;

        public string finca { get; set; } = string.Empty;
        public int? edad { get; set; }

        public DateTime? registroFecha { get; set; }
        public string registroUsuario { get; set; } = string.Empty;
        public string registroObservaciones { get; set; } = string.Empty;

        public DateTime? recepcionFecha { get; set; }
        public string recepcionUsuario { get; set; } = string.Empty;
        public string recepcionObservaciones { get; set; } = string.Empty;
        public bool recepcionAplicada { get; set; }

        public decimal montoAprobado { get; set; }
        public decimal montoOperacion { get; set; }
        public string plan { get; set; } = string.Empty;
        public int? contrato { get; set; }

        public bool fondoGenerado { get; set; }
        public bool aportacionAplicada { get; set; }

        public int? estadoActualId { get; set; }
        public int? formaDesembolsoId { get; set; }
        public int? metodoPagoId { get; set; }
        public int? motivoId { get; set; }
        public int? enfermedadId { get; set; }
        public int? siniestroId { get; set; }
        public int? causaId { get; set; }

        public bool mostrarVida { get; set; }
        public bool mostrarIncendio { get; set; }
        public bool esNuevo { get; set; }
    }

    public class PolizaReclamoRequestNuevo
    {
        public string cedula { get; set; } = string.Empty;
        public int operacion { get; set; } = 0;
        public int poliza { get; set; } = 0;
        public string polizaCodigo { get; set; } = string.Empty;
    }

    public class PolizaReclamoLoadDbModel
    {
        public int ID { get; set; }
        public int ID_SOLICITUD_POLIZA { get; set; }
        public string CODIGO_POLIZA { get; set; } = string.Empty;
        public int ID_SOLICITUD { get; set; }
        public string CODIGO { get; set; } = string.Empty;
        public string CEDULA { get; set; } = string.Empty;
        public string NOMBRE { get; set; } = string.Empty;
        public string PRIMER_APELLIDO { get; set; } = string.Empty;
        public string SEGUNDO_APELLIDO { get; set; } = string.Empty;
        public string SEXO { get; set; } = string.Empty;
        public DateTime? FECHA_NACIMIENTO { get; set; }
        public decimal? MONTO_APROBADO { get; set; }
        public int? ESTADO_ACTUAL { get; set; }
        public string FINCA { get; set; } = string.Empty;
        public int? TIPO_SINIESTRO { get; set; }
        public int? CAUSA_SINIESTRO { get; set; }
        public int? MOTIVO_RECLAMO { get; set; }
        public int? ENFERMEDAD { get; set; }
        public int? EDAD { get; set; }
        public int? FORMA_DESEMBOLSO { get; set; }
        public int? METODO_PAGO { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
        public string REGISTRO_OBSERVACIONES { get; set; } = string.Empty;
        public DateTime? RECEPCION_FECHA { get; set; }
        public string RECEPCION_USUARIO { get; set; } = string.Empty;
        public string RECEPCION_OBSERVACIONES { get; set; } = string.Empty;
        public bool? FONDO_GENERADO { get; set; }
        public int? CODIGO_FONDO { get; set; }
        public bool? APORTACION_APLICADA { get; set; }
        public bool? DEPOSITO_RECIBIDO { get; set; }
        public DateTime? RECEPCION_DEPOSITO_FECHA { get; set; }
        public string PLACA { get; set; } = string.Empty;
        public int? TIPO_SINIESTRO_VHC { get; set; }
        public int? CAUSA_SINIESTRO_VHC { get; set; }
        public string COD_PLAN { get; set; } = string.Empty;
        public int? I_FONDO_GENERADO { get; set; }
        public int? I_APORTACION_APLICADA { get; set; }
        public string NOMBRE_COMPLETO { get; set; } = string.Empty;
        public string ESTADO_DESC { get; set; } = string.Empty;
        public string ENFERMEDAD_DESC { get; set; } = string.Empty;
        public string SEXO_DESC { get; set; } = string.Empty;
        public string FORMA_DESEMBOLSO_DESC { get; set; } = string.Empty;
        public string METODO_PAGO_DESC { get; set; } = string.Empty;
        public int? MOTIVO_ID { get; set; }
        public string MOTIVO_RECLAMO_DESC { get; set; } = string.Empty;
        public string TIPO_SINIESTRO_DESC { get; set; } = string.Empty;
        public int? CAUSA_ID { get; set; }
        public string CAUSA_DESC { get; set; } = string.Empty;
        public decimal? SALDO_CREDITO { get; set; }
        public decimal? MONTO_CREDITO { get; set; }
        public decimal? SALDO_FONDO { get; set; }
        public string POLIZA_DESC { get; set; } = string.Empty;
        public string POLIZA_GRUPO_DESC { get; set; } = string.Empty;
        public string TIPO_APLICACION { get; set; } = string.Empty;
        public string TIPO_POLIZA { get; set; } = string.Empty;
    }

    public class PolizaReclamoNuevoDbModel
    {
        public int ID { get; set; }
        public string CEDULA { get; set; } = string.Empty;
        public string CEDULAR { get; set; } = string.Empty;
        public string NOMBRE { get; set; } = string.Empty;
        public string APELLIDO1 { get; set; } = string.Empty;
        public string APELLIDO2 { get; set; } = string.Empty;
        public string NOMBREV2 { get; set; } = string.Empty;
        public string ESTADOACTUAL { get; set; } = string.Empty;
        public DateTime? FECHA_NAC { get; set; }
        public string SEXO { get; set; } = string.Empty;
        public string SEXO_DESC { get; set; } = string.Empty;
        public string FORMA_DESEMBOLSO_DESC { get; set; } = string.Empty;
        public string METODO_PAGO_DESC { get; set; } = string.Empty;
        public decimal? SALDO_CREDITO { get; set; }
        public string POLIZA_DESC { get; set; } = string.Empty;
        public string POLIZA_GRUPO_DESC { get; set; } = string.Empty;
        public string TIPO_APLICACION { get; set; } = string.Empty;
        public string TIPO_POLIZA { get; set; } = string.Empty;
        public int ID_SOLICITUD { get; set; }
        public string POLIZA_CODIGO { get; set; } = string.Empty;
        public int POLIZA_ID { get; set; }
        public int? EDAD { get; set; }
        public int Reclamo_Id { get; set; }
        public string Finca { get; set; } = string.Empty;
    }

    /// <summary>
    /// Línea del histórico de seguimiento de un reclamo.
    /// </summary>
    public class PolizaReclamoSeguimientoItemResponse
    {
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
    }

    public class PolizaReclamoFondoItemResponse
    {
        public string ncon { get; set; } = string.Empty;
        public string tipo_documento_desc { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string concepto_desc { get; set; } = string.Empty;
        public string referencia_01 { get; set; } = string.Empty;
        public string referencia_02 { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class PolizaReclamoDesembolsoItemResponse
    {
        public int consec { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public decimal total_girar { get; set; }
        public int? pago_tercero_tipo { get; set; }
        public string pago_tercero_id { get; set; } = string.Empty;
        public string pago_tercero_nombre { get; set; } = string.Empty;
        public string solicitud_tesoreria { get; set; } = string.Empty;
        public string tes_tipo { get; set; } = string.Empty;
        public string tes_banco { get; set; } = string.Empty;
        public string tes_documento { get; set; } = string.Empty;
        public string tes_estado { get; set; } = string.Empty;
        public DateTime? tes_fecha { get; set; }
    }

    public class PolizaReclamoEtiquetaItemResponse
    {
        public int id_etiqueta { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
    }

    public class PolizaReclamoActualizarVidaRequest
    {
        public int reclamo_id { get; set; }
        public int motivo_id { get; set; }
        public int enfermedad_id { get; set; }
        public int edad { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class PolizaReclamoActualizarIncendioRequest
    {
        public int reclamo_id { get; set; }
        public int siniestro_id { get; set; }
        public int causa_id { get; set; }
        public string finca { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class PolizaReclamoActualizarRecepcionRequest
    {
        public int reclamo_id { get; set; }
        public DateTime fecha { get; set; }
        public string observaciones { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class PolizaReclamoSeguimientoManualAddRequest
    {
        public int reclamo_id { get; set; }
        public int estado_id { get; set; }
        public string observaciones { get; set; } = string.Empty;
        public int i_correo { get; set; }
        public string destinatarios { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class PolizaReclamoFondoCrearRequest
    {
        public int reclamo_id { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class PolizaReclamoFondoCrearResponse
    {
        public string cod_plan { get; set; } = string.Empty;
        public int? codigo_fondo { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    public class PolizaReclamoFondoAportacionRequest
    {
        public int reclamo_id { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class PolizaReclamoFondoAportacionResponse
    {
        public string cod_plan { get; set; } = string.Empty;
        public int? codigo_fondo { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    public class PolizaReclamoDesembolsoAplicaRequest
    {
        public int reclamo_id { get; set; }
        public decimal monto { get; set; }
        public string plan { get; set; } = string.Empty;
        public int contrato { get; set; }
        public int banco_id { get; set; }
        public string cuenta { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class PolizaReclamoDesembolsoAplicaResponse
    {
        public string mensaje { get; set; } = string.Empty;
        public string movimiento { get; set; } = string.Empty;
    }

    public class PolizaReclamoEtiquetaManualAddRequest
    {
        public int reclamo_id { get; set; }
        public string observaciones { get; set; } = string.Empty;
        public int i_correo { get; set; }
        public string destinatarios { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public static class PolizaReclamoConstFrm
    {
        public const string valRequest = "Request inválido.";
        public const string valReclamo = "Debe indicar el reclamo.";
        public const string valUsuario = "Debe indicar el usuario.";
    }
}
