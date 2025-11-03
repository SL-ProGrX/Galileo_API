using PgxAPI.Models.ProGrX.Clientes;
using System.Text.Json.Serialization;

namespace PgxAPI.Models.ProGrX.Credito
{
    public class CrConsultaCrdSociosData
    {
        public string? cedula { get; set; }
        public string? cedular { get; set; }
        public string? nombre { get; set; }
    }

    public class CrConsultaCrdData
    {
        public string? cedulax { get; set; }
        public string? nombre { get; set; }
        public DateTime? fechaingreso { get; set; }
        public string? estadoactual { get; set; }
        public string? notas { get; set; }
        public bool? bloqueo { get; set; }
        public string? nota_user { get; set; }
        public DateTime? nota_fecha { get; set; }

        // Montos / saldos
        public decimal? obrero { get; set; } = 0;
        public decimal? patronal { get; set; } = 0;
        public decimal? custodia { get; set; } = 0;
        public decimal? capitaliza { get; set; } = 0;
        public string? cod_divisa { get; set; }
        public decimal? ahorro { get; set; } = 0;
        public decimal? aporte { get; set; } = 0;
        public decimal? extra { get; set; } = 0;

        // Fechas de movimientos
        public DateTime? fecextra { get; set; }
        public DateTime? fecahorro { get; set; }
        public DateTime? fecaporte { get; set; }
        public DateTime? feccustodia { get; set; }
        public DateTime? feccapitaliza { get; set; }

        public string? clasificacion { get; set; }
        public string? clasificacionCaption { get; set; }
        public string? rating { get; set; }

        // Indicadores
        public int? indmensajes { get; set; }
        public int? indcobro { get; set; }
        public bool? indfianzas { get; set; }
        public int? indadvertencias { get; set; }
        public int? indbeneficiarios { get; set; }

        public string? fianzasCaption { get; set; }
        public string? estadoMensajesCaption { get; set; }
        public string? estadoCobrosCaption { get; set; }
        public string? estadoAdvertenciaCaption { get; set; }

        // Patronal / cajas
        public decimal? cajas_saldo_favor { get; set; }
        public decimal? pat_garantia_total { get; set; }
        public decimal? pat_garantia_saldos { get; set; }
        public string? pat_advertencia { get; set; }
        public decimal? pat_aporte_manual { get; set; }

        public string? pat_tipoSaldo { get; set; }

        // Otros datos
        public string? institucionx { get; set; }
        public string? estadox { get; set; }
        public string? deductora { get; set; }

        // Beneficiarios / consentimiento
        public DateTime? ben_update_fecha { get; set; }
        public string? ben_update_usuario { get; set; }
        public DateTime? consentimiento_contacto_fecha { get; set; }
        public string? consentimiento_contacto_usuario { get; set; }

        public string? estadoConsentimientoToolTip { get; set; }
        public DateTime? consentimientoFecha { get; set; }
        public string? consentimientoUsuario { get; set; }

        // Bancarios / nómina
        public string? tarjeta_numero { get; set; }
        public string? iban { get; set; }
        public string? ibanCaption { get; set; }
        public decimal? salario_traslada { get; set; }
        public string? salarioTrasladaCaption { get; set; }
        public string? tarjetaCaption { get; set; }
        public bool? insolvente { get; set; }

        //Membrecia
        public string? membresiaCaption { get; set; } = string.Empty;
        public string? membresiaToolTip { get; set; } = string.Empty;
        public string? membresiaLabel { get; set; } = string.Empty;

        public string? cod_Renuncia { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string? registro_user { get; set; } = string.Empty;

        public string? estado { get; set; } = string.Empty;
        public string? tipo { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;

        public decimal? patrimonio { get; set; } = 0;

        //Mensajes
        public int? pendientes { get; set; }
        public string? pendientesCaption { get; set; }
        public int? advertencias { get; set; }
        public string? advertenciasCaption { get; set; }
        public int? generales { get; set; }
        public string? generalesCaption { get; set; }
        public int? morosidad { get; set; }
        public string? morosidadCaption { get; set; }
        public int? bloqueos { get; set; }
        public string? bloqueosCaption { get; set; }

        public bool? vMora { get; set; }
        public string? vMoraCaption { get; set; }


    }


    public sealed class CrConsultaCrd_CreditosData
    {
        public int id_solicitud { get; set; }
        public string? codigo { get; set; }
        public string? prideduc { get; set; }
        public string? fecult { get; set; }
        public string? procesoCod { get; set; }

        public decimal? montoApr { get; set; }
        public decimal? saldo { get; set; }
        public decimal? cuota { get; set; }

        public string? estado { get; set; }
        public string? proceso { get; set; }
        public string? lineaX { get; set; }

        public string? documento_referido { get; set; }
        public string? nDocumento { get; set; }
        public string? referencia { get; set; }

        public decimal? interesv { get; set; }
        public int? plazo { get; set; }
        public decimal? tasaOriginal { get; set; }

        public string? garantia { get; set; }

        public decimal? moraCuota { get; set; }
        public decimal? moraInt { get; set; }
        public decimal? moraPrincipal { get; set; }
        public decimal? moraCargos { get; set; }
        public decimal? moraPoliza { get; set; }

        public string? moraAntigua { get; set; }
        public string? moraUltima { get; set; }

        public string? antiguedad { get; set; }
        public DateTime? termina { get; set; }

        public string? observacion_proceso { get; set; }
        public DateTime? fecha_enviaProceso { get; set; }
        public DateTime? fechaForp { get; set; }

        public string? userFor { get; set; }
        public string? cod_oficina_r { get; set; }
        public string? oficinaX { get; set; }

        public int? indicadorCbr { get; set; }
        public string? garantiaDetalle { get; set; }

        public int? deductoraCod { get; set; }
        public string? deductora { get; set; }

        public decimal? poliza_cuota { get; set; }
        public string? poliza_numero { get; set; }

        public string? cod_divisa { get; set; }
        public string? divisa_desc { get; set; }
        public string? currency_sim { get; set; }

        public string? ind_deduce_planilla { get; set; }
        public string? base_calculo { get; set; }

        public string? canal_tipo { get; set; }
        public string? cod_canal { get; set; }
        public string? actividadDesc { get; set; }
        public string? canalDesc { get; set; }

        public DateTime? ctaFechaUltCorte { get; set; }
        public string? iban { get; set; }

        public int? cbrExterno { get; set; }
        public int? cobroFiador { get; set; }

        public string? salida_tipo { get; set; }
        public string? salida_desc { get; set; }
        public int? alerta_pago { get; set; }


    }

    public sealed class CrConsultaCrd_solicitudData
    {
        public int? id_solicitud { get; set; }
        public string? codigo { get; set; }
        public string? cedula { get; set; }
        public DateTime? fechasol { get; set; }
        public decimal? montosol { get; set; }
        public string? estadosol { get; set; }
        public string? estado { get; set; }
        public string? proceso { get; set; }
        public string? observacion { get; set; }
        public string? lineax { get; set; }
        public string? userrec { get; set; }
        public string? cod_oficina_r { get; set; }
        public string? oficinax { get; set; }
        public string? garantia { get; set; }
        public int? indicadorcbr { get; set; }
        public string? garantiadetalle { get; set; }
        public string? cod_divisa { get; set; }
        public string? divisa_desc { get; set; }
        public string? currency_sim { get; set; }
    }

    public sealed class CrConsultaCrd_preanalisisData
    {
        public int? operacion { get; set; }
        public string? cod_preanalisis { get; set; }
        public string? cod_linea { get; set; }
        public decimal? monto { get; set; }
        public DateTime? fecha_creacion { get; set; }
        public string? usuario { get; set; }
        public string? tipo { get; set; }
        public string? estado { get; set; }
    }

    public sealed class CrConsultaCrd_incobrableData
    {
        public int? cod_incobrable { get; set; }
        public int? id_solicitud { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }

        public decimal? saldo { get; set; }
        public decimal? intcor { get; set; }
        public decimal? intmor { get; set; }
        public string? estado { get; set; }

        public string? modifica_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }

        public decimal? reactivacion_recargo { get; set; }   // si fuese texto, cámbiala a string?
        public string? genera_documento { get; set; }
        public string? reversa_documento { get; set; }

        public string? notas_registro { get; set; }
        public string? notas_reversion { get; set; }

        public decimal? cargos { get; set; }
        public decimal? poliza { get; set; }
        public decimal? principal { get; set; }

        public string? tipo_documento { get; set; }
        public string? cod_transaccion { get; set; }

        public decimal? recaudado { get; set; }
        public int? cxc_operacion { get; set; }

        public string? codigo { get; set; }
        public string? estadox { get; set; }
    }

    public class CrConsultaCobroDTO
    {
        public int cod_seg { get; set; }
        public string cod_gestion { get; set; }
        public string usuario { get; set; }
        public string cedula { get; set; }
        public DateTime fecha { get; set; }
        public decimal monto { get; set; }
        public string observacion { get; set; }
        public int dias { get; set; }
        public int estado { get; set; }
        public string cod_causa { get; set; }
        public string cod_arreglo { get; set; }
        public string hora { get; set; }
        public string minuto { get; set; }
        public DateTime fecha_compromiso { get; set; }
        public DateTime fecha_registro { get; set; }
        public string gestion { get; set; }
        public string causa { get; set; }
        public string arreglo { get; set; }
        public string notas { get; set; }
        public DateTime comision_vence { get; set; }

        public string tiempo_resolucion { get; set; }

    }

    public class CrConsultaAsignacionCobroData
    {
        public string usuario { get; set; }
        public string cedula { get; set; }
        public DateTime fecha_asignacion { get; set; }
        public int mantener { get; set; }
        public int rebajo_doble { get; set; }
        public int aplica_mora { get; set; }
    }

    public class CrConsultaContratosData
    {
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; }
        public string cod_contrato { get; set; }
        public string cedula { get; set; }
        public string cod_vendedor { get; set; }
        public string estado { get; set; }
        public DateTime fecha_inicio { get; set; }
        public int plazo { get; set; }
        public decimal monto { get; set; }
        public string moneda { get; set; }
        public decimal saldo { get; set; }
        public string tipo_cuenta { get; set; }
        public decimal interes { get; set; }
        public decimal mora { get; set; }
        public decimal comision { get; set; }
        public decimal otros_cargos { get; set; }
        public string referencia { get; set; }
        public int oficina { get; set; }
        public string ejecutivo { get; set; }
        public int estado_contrato { get; set; }
        public string tipo_producto { get; set; }
        public DateTime fecha_vencimiento { get; set; }
        public decimal monto_vencido { get; set; }
        public string situacion { get; set; }
        public string observaciones { get; set; }
        public string canal { get; set; }
        public string usuario { get; set; }
        public string nombre_cliente { get; set; }
        public string producto_nombre { get; set; }
        public string operadora { get; set; }
        public string estado_actual { get; set; }
        public decimal aportes { get; set; }
        public decimal rendimiento { get; set; }
        public decimal total { get; set; }
        public string plan_desc { get; set; }
        public string operadora_desc { get; set; }

    }

    public class CrContratosMovimientosData
    {
        public DateTime fecha { get; set; }
        public string fecha_proceso { get; set; }
        public decimal monto { get; set; }
        public string docdesc { get; set; }
        public string ncon { get; set; }
        public string condesc { get; set; }
        public string usuario { get; set; }
        public string detalle_01 { get; set; }
    }

    public class CrContratosCuponesData
    {
        public int cupon_id { get; set; }
        public DateTime fecha_vence { get; set; }
        public decimal monto_base { get; set; }
        public decimal tasa_aplicada { get; set; }
        public decimal cupon_monto { get; set; }
        public decimal rendimiento { get; set; }
        public decimal principal { get; set; }
        public int dias { get; set; }
        public string estado_desc { get; set; }
        public string consec { get; set; }
        public decimal isr_porc { get; set; }
        public decimal isr_mnt_gravable { get; set; }
        public decimal isr_monto { get; set; }
        public decimal total_girar { get; set; }
        public string tesoreria_id { get; set; }
        public string tes_documento { get; set; }
        public string bancos_estado { get; set; }
        public string iban { get; set; }
    }

    public class CrContratosBitacoraData
    {
        public int id_bitacora { get; set; }
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; }
        public long cod_contrato { get; set; }
        public string usuario { get; set; }
        public DateTime fecha { get; set; }
        public string movimiento { get; set; }
        public string detalle { get; set; }
        public string revisado_usuario { get; set; }
        public DateTime? revisado_fecha { get; set; }
        public string cedula { get; set; }
        public string nombre { get; set; }
        public string movimientodesc { get; set; }
        public int revisado { get; set; }
    }

    public class CrContratosCierresData
    {
        public int anio { get; set; }
        public int mes { get; set; }
        public decimal aportes { get; set; }
        public decimal rendimientos { get; set; }
        public decimal total { get; set; }
        public decimal monto_transito { get; set; }
        public decimal sobre_giro { get; set; }
        public string rend_corte { get; set; }
        public string ind_deduccion { get; set; }
        public string tipo_deduc { get; set; }
        public decimal? porc_deduc { get; set; }
        public decimal? monto { get; set; }
        public decimal? inversion { get; set; }
        public string cashback_pts_corte { get; set; }
        public decimal cashback_pts_otorgados { get; set; }
        public decimal cashback_pts_redimidos { get; set; }
        public string cod_plan { get; set; }
        public long cod_contrato { get; set; }
    }

    public class CrPatrimonioData
    {
        public long consec { get; set; }
        public string cedula { get; set; }
        public string tipo { get; set; }
        public decimal monto { get; set; }
        public DateTime fecha { get; set; }
        public string fechaproc { get; set; }
        public string estado { get; set; }
        public string numcom { get; set; }
        public string tcon { get; set; }
        public string ncon { get; set; }
        public DateTime? fecajusteahorro { get; set; }
        public string usuario { get; set; }
        public string cod_caja { get; set; }
        public string cod_concepto { get; set; }
        public string docdesc { get; set; }
        public string condesc { get; set; }
    }


    public class ExcPeriodosVisiblesData
    {
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
        public int id_periodo { get; set; }
        public string cedula { get; set; }
        public decimal excedente_bruto { get; set; }
        public decimal capitalizado { get; set; }
        public decimal renta_total { get; set; }
        public decimal renta { get; set; }
        public decimal? renta_retenida { get; set; }
        public decimal excedente_neto { get; set; }
        public decimal donacion { get; set; }
        public decimal excedente_neto2 { get; set; }
        public decimal ajuste_cargado { get; set; }
        public decimal ajuste_aplicado { get; set; }
        public decimal excedente_posajuste { get; set; }
        public decimal mora_cargada { get; set; }
        public decimal mora_aplicada { get; set; }
        public decimal exc_posmora { get; set; }
        public decimal moraopcf_cargada { get; set; }
        public decimal moraopcf_aplicada { get; set; }
        public decimal exc_posmoraopcf { get; set; }
        public decimal capitalizado_individual { get; set; }
        public decimal saldos_ase_cargado { get; set; }
        public decimal saldos_ase_aplicados { get; set; }
        public decimal exc_possaldos_ase { get; set; }
        public decimal excedente_final { get; set; }
        public decimal? excedente_final_alt { get; set; }
        public int ind_act_mora { get; set; }
        public int ind_act_capind { get; set; }
        public int ind_act_capgen { get; set; }
        public int ind_act_ajustes { get; set; }
        public int? ind_act_moraopcf { get; set; }
        public int? ind_act_creditosase { get; set; }
        public string estadoactual { get; set; }
        public string salida_codigo { get; set; }
        public DateTime? salida_fecha { get; set; }
        public string salida_usuario { get; set; }
        public string cuenta_bancaria { get; set; }
        public decimal? reserva { get; set; }
        public decimal? salidafnd { get; set; }
        public decimal? extraordinario_apl { get; set; }
        public int? ind_insolvente { get; set; }
    }


    public class AfiBeneficiosConsultaData
    {
        public int consec { get; set; }
        public string cod_beneficio { get; set; }
        public string cedula { get; set; }
        public decimal? monto { get; set; }
        public string modifica_monto { get; set; }
        public DateTime registra_fecha { get; set; }
        public string registra_user { get; set; }
        public string autoriza_user { get; set; }
        public string estado { get; set; }
        public DateTime? autoriza_fecha { get; set; }
        public string notas { get; set; }
        public string solicita { get; set; }
        public string nombre { get; set; }
        public string tipo { get; set; }
        public string analista_revision { get; set; }
        public string analista_recepcion { get; set; }
        public int? cod_remesa { get; set; }
        public string cod_oficina { get; set; }
        public int? id_beneficio { get; set; }
        public string modifica_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public decimal? monto_aplicado { get; set; }
        public string fena_nombre { get; set; }
        public string fena_descripcion { get; set; }
        public string sepelio_identificacion { get; set; }
        public string sepelio_nombre { get; set; }
        public string apt_cod_motivo { get; set; }
        public string apt_motivo_nota { get; set; }
        public int? id_profesional { get; set; }
        public int? id_apt_categoria { get; set; }
        public DateTime? sepelio_fecha_fallecimiento { get; set; }
        public string crece_grupo { get; set; }
        public int? requiere_justificacion { get; set; }
        public int? aplica_pago_masivo { get; set; }
        public int? aplica_mora { get; set; }
        public string nombre_beneficiario { get; set; }
        public string beneficio_desc { get; set; }
        public string remesa_estado { get; set; }
        public DateTime? remesa_fecha { get; set; }
        public string estado_desc { get; set; }
        public string tipo_benefico { get; set; }
        public string sol_cedula { get; set; }
        public string sol_nombre { get; set; }
    }

    public class AfiRenunciaTransitoData
    {
        public int cod_renuncia { get; set; }
        public string descripcion { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_user { get; set; }
        public string tipo { get; set; }
        public string estado { get; set; }
    }

    public class AfiRenunciasConsultaData
    {
        public int cod_renuncia { get; set; }
        public string causa_desc { get; set; }
        public string cedula { get; set; }
        public string nombre { get; set; }
        public string estado_desc { get; set; }
        public string estado_persona_actual { get; set; }
        public DateTime vencimiento { get; set; }
        public string tipo_renuncia { get; set; }
        public string notas { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; }
        public DateTime? resuelto_fecha { get; set; }
        public string resuelto_usuario { get; set; }
        public string desea_volver { get; set; }
        public string aplica_reingreso { get; set; }
        public string ejecutivo_desc { get; set; }
        public string institucion_desc { get; set; }
        public string provincia_desc { get; set; }
        public string email { get; set; }
        public int liquida_id { get; set; }
        public DateTime? liquida_fecha { get; set; }
        public string liquida_usuario { get; set; }
        public string liquida_estado { get; set; }
    }

    public class AfiSociosMensajesData
    {
        public DateTime fecha { get; set; }
        public string cedula { get; set; }
        public string mensaje { get; set; }
        public DateTime vencimiento { get; set; }
        public string usuario { get; set; }
        public string tipo { get; set; }
        public string resolucion { get; set; }
        public DateTime? resolucion_fecha { get; set; }
        public string? resolucion_usuario { get; set; }
    }

    public class SysMailLoadData
    {
        public int id_email { get; set; }
        public string cod_smtp { get; set; }
        public string para { get; set; }
        public string asunto { get; set; }
        public string estado { get; set; }
        public DateTime fecha { get; set; }
        public DateTime? fecha_envio { get; set; }
        public string usuario { get; set; }
        public string estadodesc { get; set; }
    }

    public class CR_liquidacionDto
    {
        public int consec { get; set; }
        public DateTime fecliq { get; set; }

        public string estadoactliq { get; set; }
        public string estadoactliqdesc { get; set; }

        public string estadoactual { get; set; }
        public string estadoactualdesc { get; set; }

        public string estadopersona { get; set; }

        public string tdocumento { get; set; }
        public string ubicacion { get; set; }
        public string ubicaciondesc { get; set; }

        public decimal tneto { get; set; }
        public string tneto_format { get; set; }

        public string estado { get; set; }
        public string estadodesc { get; set; }
    }

    public class AF_PersonaDetalleDto
    {
        public string? direccion { get; set; }
        public string? provincia { get; set; }
        public string? canton { get; set; }
        public string? distrito { get; set; }

        public string? sexo { get; set; }
        public DateTime? fecha_nac { get; set; }

        public string? estadocivil { get; set; }
        public string? estadocivil_desc { get; set; }

        public string? email_01 { get; set; }
        public string? email_02 { get; set; }

        public string? facebook { get; set; }
        public string? twitter { get; set; }
        public string? linkedin { get; set; }

        public string? estadopersona { get; set; }
        public DateTime? fechaingreso { get; set; }

        public string? nacionalidad { get; set; }
        public int edad { get; set; }
    }

    public class AF_PersonaEstadoLaboralDto
    {
        public string? institucion { get; set; }
        public string? departamento { get; set; }
        public string? seccion { get; set; }

        public DateTime? fecha { get; set; }
        public int anioslaborados { get; set; }

        public string? estadolaboral { get; set; }
    }

    public class AFPersonaBenePolizaDTO
    {
        public int linea { get; set; }
        public string? tipo_id { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? cod_parentesco { get; set; }
        public string? porcentaje { get; set; }
        public string? parentesco { get; set; }
        public string? tipo_id_desc { get; set; }
        public string? poliza { get; set; }
        public string? poliza_desc { get; set; }
    }

    public class CR_PersonaLiquidacionDTO
    {
        public int Consec { get; set; }
        public DateTime? Fecliq { get; set; }
        public string? estadoactliq { get; set; }
        public string? estadoactliqdesc { get; set; }
        public string? estadoactual { get; set; }
        public string? estadoactualdesc { get; set; }
        public string? estadopersona { get; set; }
        public string? tdocumento { get; set; }
        public string? ubicacion { get; set; }
        public string? ubicaciondesc { get; set; }
        public decimal tneto { get; set; }
        public string? tneto_format { get; set; }
        public string? estado { get; set; }
        public string? estadodesc { get; set; }
    }


    public class CR_ConsultasInfoDTO
    {
        public List<AF_TelefonosDTO> Telefonos { get; set; }
        public List<AF_CuentaBancariaDTO> CuentasBancarias { get; set; }
        public List<AF_PersonaBeneficiarioDTO> Beneficiarios { get; set; }
        public List<AF_TarjetaDTO> Tarjetas { get; set; }
        public List<AF_DireccionDTO> Localizaciones { get; set; }
        public List<AF_PersonaIngresoDTO> Ingresos { get; set; } = new();
        public List<AF_PersonaRenunciaDTO> Renuncias { get; set; } = new();
        public List<CR_PersonaLiquidacionDTO> Liquidaciones { get; set; } = new();
        public List<AF_PersonaNombramientoDTO> Nombramientos { get; set; } = new();
        public List<AF_PersonaSalarioDTO> Salarios { get; set; } = new();
        public List<AF_PersonaEmailDTO> Emails { get; set; } = new();
        public List<AF_MotivosDTO> Motivos { get; set; } = new();
        public List<AF_CanalesDTO> Canales { get; set; } = new();
        public List<CrPreferenciaDTO> Preferencias { get; set; } = new();
        public List<AF_BienDTO> Bienes { get; set; } = new();
        public List<AF_EscolaridadDTO> Escolaridad { get; set; } = new();
        public List<AF_PersonaRelacionDTO> Relaciones { get; set; } = new();
        public List<AF_PersonaDetalleDto> Contacto { get; set; } = new();
        public List<AF_PersonaEstadoLaboralDto> EstadoLaboral { get; set; } = new();
        public List<AFPersonaBenePolizaDTO> BenePolizas { get; set; } = new();


    }

    public class CrConsultaCreditosData
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; }
        public string cedula { get; set; }
        public DateTime fechasol { get; set; }
        public decimal montoSol { get; set; }
        public string estadosol { get; set; }
        public string estado { get; set; }
        public string proceso { get; set; }
        public string observacion { get; set; }
        public string linea_x { get; set; }
        public string userrec { get; set; }
        public string cod_oficina_r { get; set; }
        public string oficinaX { get; set; }
        public string garantia { get; set; }
        public int indicador_cbr { get; set; }
        public string garantia_detalle { get; set; }
        public string cod_divisa { get; set; }
        public string divisa_desc { get; set; }
        public string currency_sim { get; set; }
    }

    public class CR_PreferenciaDTO
    {
        public string Cedula { get; set; } = default!;
        public string CodPreferencia { get; set; }
        public bool Asignado { get; set; }
        public string Usuario { get; set; } = default!;
    }

    public class CrPreferenciaDTO
    {
        public int cod_preferencia { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; } = string.Empty;
    }

    public class SocioCierresData
    {
        public string email { get; set; } = string.Empty;
        public List<DropDownListaGenericaModel> periodos { get; set; } = new List<DropDownListaGenericaModel>();
    }

    public class SociosPeriodoData
    {
        public string itmx { get; set; }
        public string idx { get; set; }
    }


}





