namespace PgxAPI.Models.FSL
{
    public class FslExpedienteDatos
    {
        public long cod_expediente { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public string cod_causa { get; set; } = string.Empty;
        public string cod_comite { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string referencia_documento { get; set; } = string.Empty;
        public string referencia_numero { get; set; } = string.Empty;
        public string presenta_cedula { get; set; } = string.Empty;
        public string presenta_nombre { get; set; } = string.Empty;
        public string presenta_notas { get; set; } = string.Empty;
        public int membresia_meses { get; set; }
        public decimal membresia_porcentaje { get; set; }
        public DateTime fecha_establece_causa { get; set; }
        public string notas { get; set; } = string.Empty;
        public string enfermedad_notas { get; set; } = string.Empty;
        public string enfermedad_usuario { get; set; } = string.Empty;
        public DateTime enfermedad_fecha { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime modifica_fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public string resolucion_estado { get; set; } = string.Empty;
        public string resolucion_notas { get; set; } = string.Empty;
        public DateTime resolucion_fecha { get; set; }
        public string resolucion_usuario { get; set; } = string.Empty;
        public decimal total_disponible { get; set; }
        public decimal total_aplicado { get; set; }
        public decimal total_sobrante { get; set; }
        public string tipo_desembolso { get; set; } = string.Empty;
        public string tesoreria_solicitud { get; set; } = string.Empty;
        public DateTime tesoreria_fecha { get; set; }
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

    public class FslMenusData
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FslRequisitosExp
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool estado { get; set; }
        public bool opcional { get; set; }
    }

    public class FslOperacionesDatos
    {
        public long id_solicitud { get; set; }
        public string referencia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal prideduc { get; set; }
        public decimal montoapr { get; set; }
        public decimal saldo_corte { get; set; }
        public decimal monto_base { get; set; }
        public double porc_relacion { get; set; }
        public string tipo_tabla { get; set; } = string.Empty;
        public double porcentaje { get; set; }
        public decimal monto_reconocimiento { get; set; }
        public int tiempo_trans { get; set; }
        public string _base { get; set; } = string.Empty;
    }

    public class FslResolucionDatos
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string asignado { get; set; } = string.Empty;
    }

    public class FslResolucionValidacionesDatos
    {
        public bool CumpleRequisitos { get; set; }
        public bool CumpleTiempo { get; set; }
        public bool CumpleRegistro { get; set; }
    }

    public class FslExpGestiones
    {
        public string descripcion { get; set; } = string.Empty;
        public int linea { get; set; }
        public long cod_expediente { get; set; }
        public string cod_gestion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FslApelacionDatos
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Linea { get; set; } = string.Empty;
        public long Cod_Expediente { get; set; }
        public string Cod_Apelacion { get; set; } = string.Empty;
        public DateTime Fecha_Apelacion { get; set; }
        public string Presenta_Identificacion { get; set; } = string.Empty;
        public string Presenta_Nombre { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string Resolucion { get; set; } = string.Empty;
        public string Registra_Usuario { get; set; } = string.Empty;
        public DateTime Registra_Fecha { get; set; }
        public DateTime Resolucion_Fecha { get; set; }
        public string Resolucion_Usuario { get; set; } = string.Empty;
        public string Resolucion_Notas { get; set; } = string.Empty;
    }

    public class FslExpedienteData
    {
        public long cod_expediente { get; set; }
        public string cedula { get; set; } = string.Empty;
    }

    public class FslExpedienteListaData
    {
        public int Total { get; set; }
        public List<FslExpedienteData> expediente { get; set; } = new List<FslExpedienteData>();
    }

    public class FslExpedienteUpdate
    {
        public bool estado { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public long cod_expediente { get; set; }
        public string cod_Requisito { get; set; } = string.Empty;
    }

    public class FslResolucionGuardar
    {
        public string cod_comite { get; set; } = string.Empty;
        public List<FslResolucionDatos> miembros { get; set; } = new List<FslResolucionDatos>();
        public string resolucion_notas { get; set; } = string.Empty;
        public string resolucion_usuario { get; set; } = string.Empty;
        public string resolucion_estado { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public long cod_expediente { get; set; }
    }

    public class FslMiembroValida
    {
        public string usuario { get; set; } = string.Empty;
        public string clave { get; set; } = string.Empty;
    }
}