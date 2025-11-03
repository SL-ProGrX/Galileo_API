namespace PgxAPI.Models.AF
{
    public class AfBeneficioIntegralDropsLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class AfBeneficioIntegralGenericLista
    {
        public int idx { get; set; }
        public string itmx { get; set; } = string.Empty;
    }

    public class AfBeneIntegralCuentasLista
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class BeneficiosLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public float monto { get; set; }
    }

    public class AfiBeneProductos
    {
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal costo_unidad { get; set; }
    }

    public class Afi_Bene_Integral_OrP
    {
        public string cedula { get; set; } = string.Empty;
        public int consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public float monto { get; set; }
        public int? cod_banco { get; set; }
        public string tipo_emision { get; set; } = string.Empty;
        public string? cta_bancaria { get; set; }
        public string? tesoreria { get; set; }
        public string estado { get; set; } = string.Empty;
        public string? envio_user { get; set; }
        public DateTime? envio_fecha { get; set; }
        public string? tes_supervision_usuario { get; set; }
        public DateTime? tes_supervision_fecha { get; set; }
        public string? id_token { get; set; }
        public string t_identificacion { get; set; } = string.Empty;
        public string t_beneficiario { get; set; } = string.Empty;
        public string t_email { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public int? cod_remesa { get; set; }
        public string? cod_producto { get; set; }
        public int? id_pago { get; set; }
        public int? plan_id { get; set; }
        public int? id_beneficio { get; set; }
    }

    public class AfiBenePagoProyecta
    {
        public int plan_id { get; set; }
        public string cedula { get; set; } = string.Empty;
        public int consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public DateTime fecha_vence { get; set; }
        public decimal monto { get; set; }
        public int? cod_banco { get; set; }
        public string tipo_emision { get; set; } = string.Empty;
        public string? cta_bancaria { get; set; }
        public string estado { get; set; } = string.Empty;
        public string activa_usuario { get; set; } = string.Empty;
        public DateTime? activa_fecha { get; set; }
        public string? t_identificacion { get; set; }
        public string? t_beneficiario { get; set; }
        public string? t_email { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public int? cod_remesa { get; set; }
        public string? cod_producto { get; set; }
        public int? id_pago { get; set; }
    }

    #region Datos Persona

    public class AFIBeneTelefono
    {
        public int id { get; set; }
        public int id_telefono { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public int tipo { get; set; }
        public string telefono { get; set; } = string.Empty;
        public string ext { get; set; } = string.Empty;
        public string contacto { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string? modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
    }

    public class AFIBeneTelefonoGuardar
    {
        public int id { get; set; }
        public int id_telefono { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public AfBeneficioIntegralDropsLista tipo { get; set; } = new AfBeneficioIntegralDropsLista();
        public string telefono { get; set; } = string.Empty;
        public string ext { get; set; } = string.Empty;
        public string contacto { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;

        public string user_registra { get; set; } = string.Empty;

    }

    public class AfiBeneficioIntegralPersonaData
    {
        public string apellido1 { get; set; } = string.Empty;
        public string apellido2 { get; set; } = string.Empty;
        public string nombrev2 { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estadocivil { get; set; } = string.Empty;
        public string sexo { get; set; } = string.Empty;
        public DateTime fecha_nac { get; set; }
        public DateTime fechaingreso { get; set; }
        public string lugar_trabajo { get; set; } = string.Empty;
        public string nivel_academico { get; set; } = string.Empty;
        public string profesion { get; set; } = string.Empty;
        public string cod_nacionalidad { get; set; } = string.Empty;
        public string cod_pais_nac { get; set; } = string.Empty;
        public string af_email { get; set; } = string.Empty;
        public string email_02 { get; set; } = string.Empty;
        public string apto { get; set; } = string.Empty;
        public string provincia { get; set; } = string.Empty;
        public string canton { get; set; } = string.Empty;
        public string distrito { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string estadoactual { get; set; } = string.Empty;
        public string membresia { get; set; } = string.Empty;

        public string estadolaboral { get; set; } = string.Empty;

    }

    #endregion

    #region Generales

    public class BeneConsultaFiltros
    {
        public int codCliente { get; set; }
        public string? cedula { get; set; }
        public string? tipoFecha { get; set; }
        public string? fechaInicio { get; set; }
        public string? fechaCorte { get; set; }
        public string? estado { get; set; }
        public string? usuario { get; set; }
        public string? noExpediente { get; set; }
        public string categoria { get; set; } = string.Empty;

        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }

        public bool? todasFechas { get; set; }

    }

    public class BeneficioGeneralDatos
    {
        //Datos del Beneficio
        public int id_beneficio { get; set; }
        public int? consec { get; set; }
        public AfBeneficioIntegralDropsLista cod_beneficio { get; set; } = new AfBeneficioIntegralDropsLista();
        public AfBeneficioIntegralDropsLista tipo { get; set; } = new AfBeneficioIntegralDropsLista();
        public string? notas { get; set; }
        public string? crece_grupo { get; set; }
        public string? cedula { get; set; }
        public string? solicita { get; set; }
        public string? nombre { get; set; }

        //Monto Beneficio
        public float? monto { get; set; }
        public float? monto_aplicado { get; set; }
        public string? modifica_monto { get; set; }
        public string? observaciones_monto { get; set; }

        public List<AfiBenProductoDTO>? productos { get; set; }
        //Estado Beneficio
        public AfBeneficioIntegralDropsLista? estado { get; set; }
        public string? estadoObservaciones { get; set; }

        //Beneficio desastre
        public string? desa_nombre { get; set; }
        public string? desa_descripcion { get; set; }

        //Beneficio Sepelio
        public string? sepelio_identificacion { get; set; }
        public string? sepelio_nombre { get; set; }
        public Nullable<DateTime> sepelio_fecha_fallecimiento { get; set; }
        public AfBeneficioIntegralDropsLista? cod_motivo { get; set; }

        //Beneficio Crece
        public Nullable<DateTime> registra_fecha { get; set; }
        public string registra_user { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> modifica_fecha { get; set; }

        public BeneApreLista? id_profesional { get; set; } 
        public BeneApreLista? id_apt_categoria { get; set; } 

        public bool requiere_justificacion { get; set; } = false;
        public bool aplica_mora { get; set; } = false;
        public bool aplica_pago_masivo { get; set; } = false;

        public string? cod_categoria { get; set; }
    }


    public class BeneConsultaDatosLista
    {
        public int total { get; set; }
        public List<BeneConsultaDatos> lista { get; set; } = new List<BeneConsultaDatos>();
    }

    public class BeneConsultaDatos
    {
        public string expediente { get; set; } = string.Empty;
        public DateTime registra_fecha { get; set; }
        public DateTime autoriza_fecha { get; set; }
        public DateTime pago_fecha { get; set; }
        public long id_beneficio { get; set; }
        public long consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string beneficio_desc { get; set; } = string.Empty;
        public float monto { get; set; }
        public float monto_aplicado { get; set; }
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre_beneficiario { get; set; } = string.Empty;
        public string sepelio_identificacion { get; set; } = string.Empty;
        public string categoria_desc { get; set; } = string.Empty;
        public string estado_persona { get; set; } = string.Empty;
        public string grupo { get; set; } = string.Empty;
        public string capacitacion_completa { get; set; } = string.Empty;
        public string aplica_producto_fin { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string tipoDesc { get; set; } = string.Empty;
        public bool? pagos_multiples { get; set; }
        public float monto_ejecutado { get; set; }
        public bool requiere_justificacion { get; set; } = false;
        public string? provincia { get; set; }
        public string? canton { get; set; }
        public string? distrito { get; set; }
        public string? genero { get; set; }
        public string? af_email { get; set; }
        public string? valida_beneficio { get; set; } 
        public int? id_pago { get; set; }
        public int? cod_remesa { get; set; }
        public string registra_user { get; set; } = string.Empty;

        public string? int_desk { get; set; }

    }

    public class BeneficioGuadar
    {
        public int codCliente { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string cod_beneficio { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string? notas { get; set; }
        public string tipoBeneficio { get; set; } = string.Empty;

        public long? id_beneficio { get; set; }

        //Monto
        public float? montoAprobado { get; set; }
        public float? montoAplicado { get; set; }
        public string? montoObservaciones { get; set; }

        //Estado
        public string? estado { get; set; }
        public string? estadoObservaciones { get; set; }
        public long? consec { get; set; }

    }

    public class BeneficioDTO
    {
        public long consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public float monto { get; set; }
        public string modifica_monto { get; set; } = string.Empty;
        public DateTime registra_fecha { get; set; }
        public string registra_user { get; set; } = string.Empty;
        public string autoriza_user { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime autoriza_fecha { get; set; }
        public string notas { get; set; } = string.Empty;
        public string solicita { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string analista_revision { get; set; } = string.Empty;
        public string analista_recepcion { get; set; } = string.Empty;
        public string cod_remesa { get; set; } = string.Empty;
        public string cod_oficina { get; set; } = string.Empty;
        public long id_beneficio { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime modifica_fecha { get; set; }
        public float monto_aplicado { get; set; }
        public string fena_nombre { get; set; } = string.Empty;
        public string fena_descripcion { get; set; } = string.Empty;
        public string sepelio_identificacion { get; set; } = string.Empty;
        public string sepelio_nombre { get; set; } = string.Empty;
        public string apt_cod_motivo { get; set; } = string.Empty;
        public string apt_motivo_nota { get; set; } = string.Empty;
        public string id_profesional { get; set; } = string.Empty;
        public string id_apt_categoria { get; set; } = string.Empty;
        public string nombre_beneficiario { get; set; } = string.Empty;
        public string estado_persona { get; set; } = string.Empty;
        public string empresa_desc { get; set; } = string.Empty;
        public string departamento_desc { get; set; } = string.Empty;
        public string beneficio_desc { get; set; } = string.Empty;
        public string oficina_desc { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public DateTime pago_fecha { get; set; }
        public string cod_institucion { get; set; } = string.Empty;
        public string cod_departamento { get; set; } = string.Empty;
        public string estadoactual { get; set; } = string.Empty;
        public string solicita_nombre { get; set; } = string.Empty;
        public string grupo_desc { get; set; } = string.Empty;
        public string categoria_desc { get; set; } = string.Empty;

    }

    public class BeneIntNucleoFamLista
    {
        public long id_socio_familia { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string parentesco { get; set; } = string.Empty;
        public string apellido_1 { get; set; } = string.Empty;
        public string apellido_2 { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string nacionalidad { get; set; } = string.Empty;
        public string cedula_pariente { get; set; } = string.Empty;
        public int edad { get; set; }
        public string estado_civil { get; set; } = string.Empty;
        public string actividad_realiza { get; set; } = string.Empty;
        public string ocupacion { get; set; } = string.Empty;
        public string desempleo { get; set; } = string.Empty;
        public string condicion_aseguramiento { get; set; } = string.Empty;
        public float ingreso_bruto { get; set; }
        public string pension_tipo { get; set; } = string.Empty;
        public string discapacidad_tipo { get; set; } = string.Empty;
        public string discapacidad_desc { get; set; } = string.Empty;
        public string centro_educativo { get; set; } = string.Empty;
        public string grado_academico { get; set; } = string.Empty;
        public bool estudiante_becado { get; set; }
        public string ejerce_cuido { get; set; } = string.Empty;
        public float pago_x_cuido { get; set; }
        public string observaciones { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime modifica_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class BeneIntNucleoFamDTO
    {
        public long id_socio_familia { get; set; }
        public string cedula { get; set; } = string.Empty;
        public AfBeneficioIntegralDropsLista parentesco { get; set; } = new AfBeneficioIntegralDropsLista();
        public string? apellido_1 { get; set; }
        public string? apellido_2 { get; set; }
        public string? nombre { get; set; }
        public AfBeneficioIntegralDropsLista? nacionalidad { get; set; }
        public string? cedula_pariente { get; set; }
        public int? edad { get; set; }
        public AfBeneficioIntegralDropsLista? estado_civil { get; set; }
        public AfBeneficioIntegralDropsLista? actividad_realiza { get; set; }
        public string? ocupacion { get; set; }
        public AfBeneficioIntegralDropsLista? desempleo { get; set; }
        public AfBeneficioIntegralDropsLista? condicion_aseguramiento { get; set; }
        public float? ingreso_bruto { get; set; }
        public AfBeneficioIntegralDropsLista? pension_tipo { get; set; }
        public AfBeneficioIntegralDropsLista? discapacidad_tipo { get; set; }
        public string? discapacidad_desc { get; set; }
        public string? centro_educativo { get; set; }
        public AfBeneficioIntegralDropsLista? grado_academico { get; set; }
        public bool estudiante_becado { get; set; }
        public AfBeneficioIntegralDropsLista? ejerce_cuido { get; set; }
        public float? pago_x_cuido { get; set; }
        public string? observaciones { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public bool activo { get; set; }
    }

    public class BeneficioSepelio
    {
        public int codCliente { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string cod_beneficio { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string? notas { get; set; }
        public string tipoBeneficio { get; set; } = string.Empty;
        public long? id_beneficio { get; set; }

        public int sepelio_identificacion { get; set; }
        public string sepelio_nombre { get; set; } = string.Empty;

        public Nullable<DateTime> sepelio_fecha_fallecimiento { get; set; }

    }

    #endregion

    #region Apremiantes 

    public class AfiBeneSocioFinanzasGuardar
    {
        public int id { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public AfBeneficioIntegralDropsLista id_concepto { get; set; } = new AfBeneficioIntegralDropsLista();
        public string concepto { get; set; } = string.Empty;
        public float monto { get; set; }
        public string? observaciones { get; set; }
        public string? acreedor { get; set; }
        public string? deudor { get; set; }
        public float? cuota { get; set; }
        public float? saldo { get; set; }
        public float? morosidad { get; set; }
        public string registra_Usuario { get; set; } = string.Empty;
        public DateTime registra_Fecha { get; set; }
        public string modifica_Usuario { get; set; } = string.Empty;
        public DateTime? modifica_Fecha { get; set; }
        public bool activo { get; set; }
    }

    public class AfiBeneSocioFinanzas
    {
        public int id { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string id_concepto { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public float monto { get; set; }
        public string? observaciones { get; set; }
        public string? acreedor { get; set; }
        public string? deudor { get; set; }
        public float? cuota { get; set; }
        public float? saldo { get; set; }
        public float? morosidad { get; set; }
        public string registra_Usuario { get; set; } = string.Empty;
        public DateTime registra_Fecha { get; set; }
        public string modifica_Usuario { get; set; } = string.Empty;
        public DateTime? modifica_Fecha { get; set; }
        public bool activo { get; set; }
    }

    public class AfiBeneSintesisFinanzas
    {
        public string cedula { get; set; } = string.Empty;
        public float ingresos { get; set; }
        public float gastos { get; set; }
        public float gasto_especial { get; set; }
        public int miembros { get; set; }
        public float manutencion { get; set; }
        public float endeudamiento { get; set; }
    }

    public class AfiBeneSocioRegistro
    {
        public int id_registros { get; set; }
        public string cedula { get; set; } = string.Empty;
        public Nullable<DateTime> ingreso_fecha { get; set; }
        public Nullable<DateTime> renuncia_fecha { get; set; }
        public string? motivo { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class AfiBeneApreJustificacion
    {
        public int id_justificacion { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string just_list_id { get; set; } = string.Empty;
        public string? justificacion { get; set; }
        public string? advertencia { get; set; }
        public string estado { get; set; } = string.Empty;
        public string tipo_beneficio { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

    }

    public class AfiBeneApreJustificacionGuardar
    {
        public int id_justificacion { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public string cedula { get; set; } = string.Empty;
        public AfBeneficioIntegralDropsLista just_list_id { get; set; } = new AfBeneficioIntegralDropsLista();
        public string? justificacion { get; set; }
        public string? advertencia { get; set; }
        public string estado { get; set; } = string.Empty;
        public string tipo_beneficio { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

    }

    public class AfiBeneDatosCorreo
    {
        public string nombre { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string beneficio { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string expediente { get; set; } = string.Empty;
    }

    public class AfiBeneCompFinanciero
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public Nullable<DateTime> ultaportepatronal { get; set; }
        public Nullable<DateTime> ultaporteobrero { get; set; }
        public string disponible_excedentes { get; set; } = string.Empty;
        public string disponible_ahorros { get; set; } = string.Empty;
        public string mora_financiera { get; set; } = string.Empty;
        public string mora_legal { get; set; } = string.Empty;
        public string cobrojudicial { get; set; } = string.Empty;
        public string cuotas_creditos { get; set; } = string.Empty;
        public string cuotas_retenciones { get; set; } = string.Empty;
        public string cuotas_total { get; set; } = string.Empty;

        public string total_deuda { get; set; } = string.Empty;

    }

    public class BeneApreLista
    {
        public string item { get; set; }
        public string descripcion { get; set; }
    }

    public class ValidaMetodoRequiere
    {
        public int? maximo_otorga { get; set; } = 0;
        public int? modifica_monto { get; set; } = 0;
        public int? modifica_diferencia { get; set; } = 0;
        public int? vigencia_meses { get; set; } = 0;
    }

    #endregion

    #region Observaciones

    public class AfiBeneObservaciones
    {
        public int id_observacion { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public string observacion { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    #endregion

    #region Requisitos

    #endregion

    #region Orden de Pago
    public class AfiBeneOtorgaFiltros
    {
        public int? consec { get; set; }
        public string? cedula { get; set; }
        public string? categoria { get; set; }
        public string? cod_beneficio { get; set; }
    }
    #endregion

    #region Bitacora
    public class BitacoraBeneficioIntegralDTO
    {
        public int id_bitacora { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }

        public string movimiento { get; set; } = string.Empty;

        public string detalle { get; set; } = string.Empty;

        public DateTime registro_fecha { get; set; }

        public string registro_usuario { get; set; } = string.Empty;


    }
    #endregion

    #region Sanciones

    #endregion

    #region Reconocimientos
    public class AfiBeneReconocimientos
    {
        public int id_reconocimiento { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public int id_beneficio { get; set; }
        public string cedula_estudiante { get; set; } = string.Empty;
        public Nullable<DateTime> fecha_nacimiento { get; set; } = null;
        public int? edad { get; set; }
        public AfBeneficioIntegralDropsLista? genero { get; set; }
        public string? primer_apellido { get; set; }
        public string? segundo_apellido { get; set; }
        public string? nombre { get; set; }
        public AfBeneficioIntegralDropsLista? tipo_centro { get; set; }
        public string? centro_educativo { get; set; }
        public AfBeneficioIntegralDropsLista? nivel_academico { get; set; }
        public AfBeneficioIntegralDropsLista? grado { get; set; }
        public string? observaciones { get; set; }
        public AfBeneficioIntegralDropsLista? tipo_reconocimiento { get; set; }
        public int? matematicas { get; set; }
        public int? ciencias { get; set; }
        public int? estudios_sociales { get; set; }
        public int? espanol { get; set; }
        public int? idioma { get; set; }
        public AfBeneficioIntegralDropsLista? rango { get; set; }
        public AfBeneficioIntegralDropsLista? reconocimiento_etapa { get; set; }
        public Nullable<DateTime> reconocimiento_fecha { get; set; }
        public AfBeneficioIntegralDropsLista? reconocimiento_nivel { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }

    public class AfiBeneReconocimientosDatos
    {
        public int id_reconocimiento { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public int id_beneficio { get; set; }
        public string cedula_estudiante { get; set; } = string.Empty;
        public DateTime fecha_nacimiento { get; set; }
        public int edad { get; set; }
        public string? genero { get; set; }
        public string primer_apellido { get; set; } = string.Empty;
        public string? segundo_apellido { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string? tipo_centro { get; set; }
        public string? centro_educativo { get; set; }
        public string? nivel_academico { get; set; }
        public string? grado { get; set; }
        public string? observaciones { get; set; }
        public string tipo_reconocimiento { get; set; } = string.Empty;
        public int? matematicas { get; set; }
        public int? ciencias { get; set; }
        public int? estudios_sociales { get; set; }
        public int? espanol { get; set; }
        public int? idioma { get; set; }
        public string? rango { get; set; }
        public string? reconocimiento_etapa { get; set; }
        public Nullable<DateTime> reconocimiento_fecha { get; set; }
        public string? reconocimiento_nivel { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }
    #endregion

    #region Crece
    public class AfiBeneSocioCreceDTO
    {
        public int id_crece { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public bool capacitacion_cmp { get; set; }
        public bool aplica_producto { get; set; }
        public float couta_inicial { get; set; } = 0;
        public float couta_aplicar { get; set; } = 0;
        public float ahorro { get; set; } = 0;
        public float liquidez { get; set; } = 0;
        public string? observaciones_prod { get; set; }
        public string? observaciones_bene { get; set; }
        public bool aplica_bene { get; set; }
        public float monto_primera_tarjeta { get; set; } = 0;
        public bool entrega_primera_tarjeta { get; set; }
        public float monto_segunda_tarjeta { get; set; } = 0;
        public bool entrega_segunda_tarjeta { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public Nullable<DateTime> fecha_cuota_inicial { get; set; }
        public Nullable<DateTime> fecha_cuota_aplicar { get; set; }
        public Nullable<DateTime> fecha_ahorro { get; set; }
    }

    public class AfiBeneSocioCreceSesionesDTO
    {
        public int id_sesion { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public string? sesion { get; set; }
        public bool asistencia { get; set; }
        public bool tarea { get; set; }
        public string? notas { get; set; }
        public Nullable<DateTime> sesion_fecha { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? regsitro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

    }
    #endregion

    // Nuevos Metodos Nuevo Modelo

    public class BeneficioGeneral
    {
        //Datos del Beneficio
        public int id_beneficio { get; set; }
        public int? consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string? notas { get; set; }
        public string? crece_grupo { get; set; }
        public string? cedula { get; set; }
        public string? solicita { get; set; }
        public string? nombre { get; set; }

        //Monto Beneficio
        public float? monto { get; set; }
        public float? monto_aplicado { get; set; }
        public string? modifica_monto { get; set; }
        public string? observaciones_monto { get; set; }

        public List<AfiBeneficioPago>? productos { get; set; }
        //Estado Beneficio
        public string? estado { get; set; }
        public string? estadoObservaciones { get; set; }

        //Beneficio FENA
        //Beneficio desastre
        public string? desa_nombre { get; set; }
        public string? desa_descripcion { get; set; }

        //Beneficio Sepelio
        public string? sepelio_identificacion { get; set; }
        public string? sepelio_nombre { get; set; }
        public Nullable<DateTime> sepelio_fecha_fallecimiento { get; set; }
        public string? cod_motivo { get; set; }

        //Beneficio Crece
        public Nullable<DateTime> registra_fecha { get; set; }
        public string registra_user { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> modifica_fecha { get; set; }

        public int id_profesional { get; set; } = 0;
        public int id_apt_categoria { get; set; } = 0;

        public bool requiere_justificacion { get; set; } = false;
        public bool pagos_multiples { get; set; } = false;
        public bool aplica_mora { get; set; } = false;
        public bool aplica_pago_masivo { get; set; } = false;
    }

    public class AfiBenProductoDTO
    {
        public int? consec { get; set; }
        public string? cod_beneficio { get; set; }
        public string? cod_producto { get; set; }
        public float? cantidad { get; set; }
        public float? costo_unidad { get; set; }
        public string? prodDesc { get; set; }
    }

    public class BeneficiosSancionesLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public float plazo { get; set; } = 0;
        public string codigo_cobro { get; set; } = string.Empty;
    }

    public class AfiBeneSancionesDTO
    {
        public int sancion_id { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string tipo_sancion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string? notas { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public float? monto { get; set; }
        public string? codigo_cobro { get; set; }
        public int? plazo { get; set; }
        public int? n_operacion { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string? modifica_usuario { get; set; }
        public DateTime modifica_fecha { get; set; }
        public string? cod_beneficio { get; set; } = string.Empty;
        public int? consec { get; set; }

        public int? plazocredito { get; set; }

    }
    public class AfiBeneFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
        public string? cod_grupo{ get; set; }
        public int? mes { get; set; }
        public int? periodo { get; set; }
    }
    public class BitacoraBeneficioDTO
    {
        public int id_bitacora { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }

        public string movimiento { get; set; } = string.Empty;

        public string detalle { get; set; } = string.Empty;

        public DateTime registro_fecha { get; set; }

        public string registro_usuario { get; set; } = string.Empty;

    }

    public class BeneRegistroMoraDTO
    {
        public int id_mora { get; set; } 
        public string acuerdo { get; set; } = string.Empty;
        public Nullable<DateTime> acuerdo_fecha { get; set; }
        public decimal? cancelacion_mora { get; set; }
        public string? mes_cancelacion { get; set; } = string.Empty;
        public decimal? adelanto_cuota { get; set; }
        public string? mes_adelanto { get; set; } = string.Empty;
        public decimal? cancelacion_total_operacion { get; set; }
        public string? numero_operacion { get; set; } = string.Empty;

    }

    public class BeneRegistroMoraGuardar
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public int consec { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string acuerdo { get; set; } = string.Empty;
        public DateTime acuerdo_fecha { get; set; }
        public decimal? cancelacion_mora { get; set; }
        public string? mes_cancelacion { get; set; } = string.Empty;
        public decimal? adelanto_cuota { get; set; }
        public string? mes_adelanto { get; set; } = string.Empty;
        public decimal? cancelacion_total_operacion { get; set; }
        public string? numero_operacion { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class AfiBeneTicketsLista
    {
        public int total { get; set; }
        public List<AfiBeneTicketsDatos> lista { get; set; } = new List<AfiBeneTicketsDatos>();

        public int valorPendiente { get; set; }
        public int valorError { get; set; }
        public int valorConsultado { get; set; }
        public int valorIngresado { get; set; }

        public List<AfiBeneTicketTipos> tiposTramite { get; set; } = new List<AfiBeneTicketTipos>();
    }

    public class AfiBeneTicketTipos
    {
        public string tipoTramite { get; set; }
        public int total { get; set; }
    }

    public class AfiBeneTicketsDatos
    {
        public string id_zoho { get; set; }
        public DateTime fecha_creacion { get; set; }
        public string estado_zoho { get; set; }
        public string web_url { get; set; }
        public string categoria { get; set; }
        public string tipo_tramite { get; set; }
        public string cedula { get; set; }
        public string n_expediente { get; set; }
        public string consec { get; set; }
        public string cod_beneficio { get; set; }
        public int id_beneficio { get; set; }
        public string msj_interface { get; set; }
        public string estado { get; set; }
        public string caso_id { get; set; }
        public bool i_visto { get; set; }
        public bool i_pendiente { get; set; }

        public string visto_por { get; set; }
        public string incluido_por { get; set; }
        public string entrada { get; set; }

        public Nullable<DateTime> visto_fecha { get; set; }
        public Nullable<DateTime> incluido_fecha { get; set; }
    }

    public class AfiBeneTicketFiltros
    {
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string? filtro { get; set; }
        public DateTime? fechaInicio { get; set; } = DateTime.Now;
        public DateTime? fechaFin { get; set; } = DateTime.Now;
        public string? estado { get; set; } = "T";
    }

    public class TicketRequisitos
    {
        public string cod_requisito { get; set; }
        public string campo_homologado { get; set; }
        public string tipo_documento { get; set; }
    }

    public class TrdDocumentosModel
    {
        public string CodDocumento { get; set; }
        public string Consecutivo { get; set; }
        public int? IdSobre { get; set; } // Puede ser null
        public int IdEstado { get; set; }
        public short ConfirmaRecepcion { get; set; }
        public Nullable<DateTime> FechaActualiza { get; set; }
        public string UsuarioActualiza { get; set; }
        public Nullable<DateTime> FechaInserta { get; set; }
        public string UsuarioInserta { get; set; }
        public string CodBarras { get; set; }
        public string Descripcion { get; set; }
        public short Resultado { get; set; } = 1; // Valor por defecto
    }

    public class ZohoTicketAdd
    {
        public int CodEmpresa { get; set; }
        public string ticket { get; set; }
        public string usuario { get; set; }
        public string? justificacion { get; set; }
    }

}
