using System.Text.Json.Serialization;

namespace Galileo.Models.FSL
{
    public class FslExpedienteDatos
    {
        public long cod_expediente { get; set; } = 0;
        public string cod_plan { get; set; } = string.Empty;
        public string cod_causa { get; set; } = string.Empty;
        public string cod_comite { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string referencia_documento { get; set; } = string.Empty;
        public string referencia_numero { get; set; } = string.Empty;
        public string presenta_cedula { get; set; } = string.Empty;
        public string presenta_nombre { get; set; } = string.Empty;
        public string presenta_notas { get; set; } = string.Empty;
        public int membresia_meses { get; set; } = 0;
        public decimal membresia_porcentaje { get; set; } = 0;
        public DateTime? fecha_establece_causa { get; set; }
        public string notas { get; set; } = string.Empty;
        public string enfermedad_notas { get; set; } = string.Empty;
        public string enfermedad_usuario { get; set; } = string.Empty;
        public DateTime? enfermedad_fecha { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public string resolucion_estado { get; set; } = string.Empty;
        public string resolucion_notas { get; set; } = string.Empty;
        public DateTime? resolucion_fecha { get; set; }
        public string resolucion_usuario { get; set; } = string.Empty;
        public decimal total_disponible { get; set; } = 0;
        public decimal total_aplicado { get; set; } = 0;
        public decimal total_sobrante { get; set; } = 0;
        public string tipo_desembolso { get; set; } = string.Empty;
        public string tesoreria_solicitud { get; set; } = string.Empty;
        public DateTime? tesoreria_fecha { get; set; }
        public string tesoreria_usuario { get; set; } = string.Empty;
        public string tesoreria_remesa { get; set; } = string.Empty;
        public string cod_enfermedad { get; set; } = string.Empty;
        public string apl_tipo_doc { get; set; } = string.Empty;
        public string apl_num_doc { get; set; } = string.Empty;
        public string fnd_plan { get; set; } = string.Empty;
        public string fnd_contrato { get; set; } = string.Empty;
        public string fnd_tipo_doc { get; set; } = string.Empty;
        public string fnd_num_doc { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string plan { get; set; } = string.Empty;
        public string causa { get; set; } = string.Empty;
        public string enfermedad { get; set; } = string.Empty;
        public string comite { get; set; } = string.Empty;
    }

    public class FslExpedienteGuardarRequest
    {
        [JsonRequired]
        public long cod_expediente { get; set; } = 0;

        public string cedula { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string cod_causa { get; set; } = string.Empty;
        public string cod_comite { get; set; } = string.Empty;
        public string cod_enfermedad { get; set; } = string.Empty;
        public string referencia_documento { get; set; } = string.Empty;
        public string referencia_numero { get; set; } = string.Empty;
        public string presenta_cedula { get; set; } = string.Empty;
        public string presenta_nombre { get; set; } = string.Empty;
        public string presenta_notas { get; set; } = string.Empty;

        [JsonRequired]
        public DateTime? fecha_establece_causa { get; set; }

        public string notas { get; set; } = string.Empty;
        public string enfermedad_notas { get; set; } = string.Empty;

        [JsonRequired]
        public DateTime? enfermedad_fecha { get; set; }

        public string registro_usuario { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
    }

    public class FslExpedienteGuardarResultado
    {
        public long cod_expediente { get; set; } = 0;
    }

    public class FslExpedienteRequisitoData
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool estado { get; set; } = false;
        public bool opcional { get; set; } = false;
    }

    public class FslExpedienteOperacionData
    {
        public long id_solicitud { get; set; } = 0;
        public string referencia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal prideduc { get; set; } = 0;
        public decimal montoapr { get; set; } = 0;
        public decimal saldo_corte { get; set; } = 0;
        public decimal monto_base { get; set; } = 0;
        public decimal porc_relacion { get; set; } = 0;
        public string tipo_tabla { get; set; } = string.Empty;
        public decimal porcentaje { get; set; } = 0;
        public decimal monto_reconocimiento { get; set; } = 0;
        public int tiempo_trans { get; set; } = 0;
        public string base_calculo { get; set; } = string.Empty;
    }

    public class FslExpedienteResolucionMiembroData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string asignado { get; set; } = string.Empty;
    }

    public class FslExpedienteResolucionValidacionesData
    {
        public bool cumple_requisitos { get; set; } = false;
        public bool cumple_tiempo { get; set; } = false;
        public bool cumple_registro { get; set; } = false;
    }

    public class FslExpedienteGestionData
    {
        public string descripcion { get; set; } = string.Empty;
        public int linea { get; set; } = 0;
        public long cod_expediente { get; set; } = 0;
        public string cod_gestion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FslExpedienteApelacionData
    {
        public string descripcion { get; set; } = string.Empty;
        public int linea { get; set; } = 0;
        public long cod_expediente { get; set; } = 0;
        public string cod_apelacion { get; set; } = string.Empty;
        public DateTime? fecha_apelacion { get; set; }
        public string presenta_identificacion { get; set; } = string.Empty;
        public string presenta_nombre { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string resolucion { get; set; } = string.Empty;
        public string registra_usuario { get; set; } = string.Empty;
        public DateTime? registra_fecha { get; set; }
        public DateTime? resolucion_fecha { get; set; }
        public string resolucion_usuario { get; set; } = string.Empty;
        public string resolucion_notas { get; set; } = string.Empty;
    }

    public class FslExpedienteRequisitoActualizarRequest
    {
        [JsonRequired]
        public bool estado { get; set; } = false;

        [JsonRequired]
        public long cod_expediente { get; set; } = 0;

        public string cod_requisito { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FslExpedienteResolucionGuardarRequest
    {
        [JsonRequired]
        public long cod_expediente { get; set; } = 0;

        public string cod_comite { get; set; } = string.Empty;
        public string resolucion_notas { get; set; } = string.Empty;
        public string resolucion_usuario { get; set; } = string.Empty;
        public string resolucion_estado { get; set; } = string.Empty;
        public List<FslExpedienteResolucionMiembroRequest> miembros { get; set; } = [];
    }

    public class FslExpedienteResolucionMiembroRequest
    {
        public string cedula { get; set; } = string.Empty;
    }

    public class FslExpedienteMiembroValidarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string clave { get; set; } = string.Empty;
    }

    public class FslExpedienteAplicarRequest
    {
        [JsonRequired]
        public long cod_expediente { get; set; } = 0;

        public string usuario { get; set; } = string.Empty;
    }

    public class FslExpedienteAplicarResultado
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string numero_documento { get; set; } = string.Empty;
    }
}