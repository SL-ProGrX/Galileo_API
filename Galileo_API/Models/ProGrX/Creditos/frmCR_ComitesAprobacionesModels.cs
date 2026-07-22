namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrComitesAprobacionesComite
    {
        public int id_comite { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string tipo_aprobacion { get; set; } = string.Empty;
        public int naprobaciones { get; set; }
        public int acta { get; set; }
        public string acta_abierta { get; set; } = string.Empty;
        public bool linea_filtra { get; set; }
    }

    public class CrComitesAprobacionesSolicitud
    {
        public string semaforo { get; set; } = string.Empty;
        public string expediente { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal cuota { get; set; }
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public string estado { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string garantia { get; set; } = string.Empty;
        public string garantia_desc { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesSolicitudesLista
    {
        public int total { get; set; }
        public List<CrComitesAprobacionesSolicitud> lista { get; set; } = new();
    }

    public class CrComitesAprobacionesSolicitudRequest
    {
        public required int id_comite { get; set; }
        public string tipo_caso { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public required DateTime fecha_inicio { get; set; }
        public required DateTime fecha_corte { get; set; }
    }

    public class CrComitesAprobacionesDetalle
    {
        public string caso_id { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string membresia { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string estado_laboral_desc { get; set; } = string.Empty;
        public string estado_persona_desc { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal cuota { get; set; }
        public decimal monto_girado { get; set; }
        public decimal desembolso_monto { get; set; }
        public decimal desembolso_cuota { get; set; }
        public decimal refunde_monto { get; set; }
        public decimal refunde_cuota { get; set; }
        public string lugar_trabajo { get; set; } = string.Empty;
        public decimal ca { get; set; }
        public string cod_categoria_asociado { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesSeguimiento
    {
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesClasificacion
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string razon { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesFiador
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesFiadorDetalle
    {
        public string membresia { get; set; } = string.Empty;
        public string estado_actual { get; set; } = string.Empty;
        public string estado_laboral_desc { get; set; } = string.Empty;
        public string fecha_ingreso { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;
        public string lugar_trabajo { get; set; } = string.Empty;
        public decimal salario_liquido { get; set; }
        public decimal liquidez_simple { get; set; }
        public decimal liquidez_cfianzas { get; set; }
        public decimal devengado_mes { get; set; }
    }

    public class CrComitesAprobacionesPatrimonio
    {
        public decimal aporte_obrero { get; set; }
        public decimal patronal { get; set; }
        public decimal capitalizacion { get; set; }
        public decimal patronal_custodia { get; set; }
        public decimal ahorros_extra { get; set; }
        public DateTime? fecha_corte { get; set; }
        public decimal ahorros_fecha { get; set; }
        public decimal saldo_prestamos { get; set; }
        public decimal disponible_bruto { get; set; }
        public decimal total { get; set; }
        public decimal disponible { get; set; }
    }

    public class CrComitesAprobacionesDeuda
    {
        public string semaforo { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string linea { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal saldo { get; set; }
        public decimal cuota { get; set; }
        public decimal monto_atrasado { get; set; }
        public string primer_deduc { get; set; } = string.Empty;
        public string ultimo_movimiento { get; set; } = string.Empty;
        public string termina { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string operacion_referencia { get; set; } = string.Empty;
        public decimal tasa_original { get; set; }
        public decimal tasa_actual { get; set; }
        public decimal plazo { get; set; }
    }

    public class CrComitesAprobacionesDeudasResponse
    {
        public decimal total_saldo { get; set; }
        public decimal total_cuota { get; set; }
        public decimal deducciones { get; set; }
        public List<CrComitesAprobacionesDeuda> lista { get; set; } = new();
    }

    public class CrComitesAprobacionesFianza
    {
        public string operacion { get; set; } = string.Empty;
        public string linea { get; set; } = string.Empty;
        public string fiador { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal saldo { get; set; }
        public decimal cuota { get; set; }
        public string cedula_deudor { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal mora { get; set; }
        public string clasif_deudor { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesFianzasResponse
    {
        public decimal monto { get; set; }
        public decimal saldo { get; set; }
        public decimal cuota { get; set; }
        public List<CrComitesAprobacionesFianza> lista { get; set; } = new();
    }

    public class CrComitesAprobacionesRefundicion
    {
        public string operacion { get; set; } = string.Empty;
        public string linea { get; set; } = string.Empty;
        public decimal plazo { get; set; }
        public decimal monto { get; set; }
        public decimal refundicion { get; set; }
        public decimal cuota { get; set; }
        public string tipo_movimiento { get; set; } = string.Empty;
        public decimal interes_corriente { get; set; }
        public decimal interes_moratorio { get; set; }
        public decimal principal { get; set; }
        public decimal cargos { get; set; }
        public decimal polizas { get; set; }
        public string garantia { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesDesembolso
    {
        public string concepto { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal cuota { get; set; }
    }

    public class CrComitesAprobacionesCausa
    {
        public string cod_causas { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public bool seleccionada { get; set; }
    }

    public class CrComitesAprobacionesResolucionRequest
    {
        public required int id_comite { get; set; }
        public string acta { get; set; } = string.Empty;
        public string usuario_registra { get; set; } = string.Empty;
        public string tipo_caso { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string acuerdo_jd { get; set; } = string.Empty;
        public required bool confirmar_traslado_salario { get; set; }
        public List<string> usuarios { get; set; } = new();
    }

    public class CrComitesAprobacionesCausasGuardarRequest
    {
        public string tipo_caso { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public List<string> causas { get; set; } = new();
        public string usuario { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesActaActual
    {
        public int id_comite { get; set; }
        public int id_acta { get; set; }
        public string acta { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public List<CrComitesAprobacionesActaAsistencia> asistencia { get; set; } = new();
    }

    public class CrComitesAprobacionesActaGuardarRequest
    {
        public required int id_comite { get; set; }
        public string acta { get; set; } = string.Empty;
        public string sesion { get; set; } = string.Empty;
        public required DateTime fecha { get; set; }
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesActaAsistencia
    {
        public bool seleccionado { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesActaAsistenciaGuardarRequest
    {
        public required int id_comite { get; set; }
        public string acta { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public required bool asistencia { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesActaHistorico
    {
        public int id_comite { get; set; }
        public int id_acta { get; set; }
        public string sesion { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public string comite { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? cierre_fecha { get; set; }
        public string cierre_usuario { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesSocio
    {
        public string cedula { get; set; } = string.Empty;
        public string cedulaR { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrComitesAprobacionesActaResolucion
    {
        public int id_comite { get; set; }
        public int id_acta { get; set; }
        public string sesion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string linea { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
    }
}
