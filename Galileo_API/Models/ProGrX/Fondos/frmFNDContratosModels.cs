using System.Text.Json.Serialization;

namespace Galileo.Models.ProGrX.Fondos
{
    public class ContratosModels
    {
        [JsonPropertyName("cod_operadora")]
        public int cod_operadora { get; set; }

        [JsonPropertyName("cod_plan")]
        public string? cod_plan { get; set; }

        [JsonPropertyName("cod_contrato")]
        public int cod_contrato { get; set; }

        [JsonPropertyName("cedula")]
        public string? cedula { get; set; }

        [JsonPropertyName("cod_vendedor")]
        public string? cod_vendedor { get; set; }

        [JsonPropertyName("estado")]
        public string? estado { get; set; }

        [JsonPropertyName("fecha_inicio")]
        public DateTime? fecha_inicio { get; set; }

        [JsonPropertyName("plazo")]
        public int? plazo { get; set; }

        [JsonPropertyName("monto")]
        public decimal? monto { get; set; }

        [JsonPropertyName("renueva")]
        public string? renueva { get; set; }

        [JsonPropertyName("inc_anual")]
        public decimal? inc_anual { get; set; }

        [JsonPropertyName("inc_tipo")]
        public string? inc_tipo { get; set; }

        [JsonPropertyName("aportes")]
        public decimal? aportes { get; set; }

        [JsonPropertyName("capexc")]
        public decimal? capexc { get; set; }

        [JsonPropertyName("rendimiento")]
        public decimal? rendimiento { get; set; }

        [JsonPropertyName("saldo_operadora")]
        public decimal? saldo_operadora { get; set; }

        [JsonPropertyName("liq_tipo")]
        public string? liq_tipo { get; set; }

        [JsonPropertyName("liq_fecha")]
        public DateTime? liq_fecha { get; set; }

        [JsonPropertyName("liq_monto")]
        public decimal? liq_monto { get; set; }

        [JsonPropertyName("liq_retiro")]
        public decimal? liq_retiro { get; set; }

        [JsonPropertyName("liq_neto")]
        public decimal? liq_neto { get; set; }

        [JsonPropertyName("ind_deduccion")]
        public bool ind_deduccion { get; set; }

        [JsonPropertyName("operacion")]
        public string? operacion { get; set; }

        [JsonPropertyName("ult_renovacion")]
        public DateTime? ult_renovacion { get; set; }

        [JsonPropertyName("ult_retiro")]
        public DateTime? ult_retiro { get; set; }

        [JsonPropertyName("cod_banco")]
        public string? cod_banco { get; set; }

        [JsonPropertyName("tipo_pago")]
        public string? tipo_pago { get; set; }

        [JsonPropertyName("cuenta_ahorros")]
        public string? cuenta_ahorros { get; set; }

        [JsonPropertyName("ind_comision")]
        public string? ind_comision { get; set; }

        [JsonPropertyName("comision_fecha")]
        public DateTime? comision_fecha { get; set; }

        [JsonPropertyName("comision_tesoreria")]
        public string? comision_tesoreria { get; set; }

        [JsonPropertyName("comision_monto")]
        public decimal? comision_monto { get; set; }

        [JsonPropertyName("rend_corte")]
        public Nullable<DateTime> rend_corte { get; set; }

        [JsonPropertyName("rend_saldo")]
        public decimal? rend_saldo { get; set; }

        [JsonPropertyName("fecha_corte")]
        public DateTime? fecha_corte { get; set; }

        [JsonPropertyName("usuario")]
        public string? usuario { get; set; }

        [JsonPropertyName("albacea_cedula")]
        public string? albacea_cedula { get; set; }

        [JsonPropertyName("albacea_nombre")]
        public string? albacea_nombre { get; set; }

        [JsonPropertyName("plazo_tipo")]
        public string? plazo_tipo { get; set; }

        [JsonPropertyName("inversion")]
        public decimal? inversion { get; set; }

        [JsonPropertyName("tasa_referencia")]
        public decimal? tasa_referencia { get; set; }

        [JsonPropertyName("modifica_fecha")]
        public DateTime? modifica_fecha { get; set; }

        [JsonPropertyName("modifica_usuario")]
        public string? modifica_usuario { get; set; }

        [JsonPropertyName("cod_oficina")]
        public string? cod_oficina { get; set; }

        [JsonPropertyName("tasa_tipo")]
        public string? tasa_tipo { get; set; }

        [JsonPropertyName("tasa_ptsadd")]
        public decimal? tasa_ptsadd { get; set; }

        [JsonPropertyName("cupon_frecuencia")]
        public string? cupon_frecuencia { get; set; }

        [JsonPropertyName("cupon_proximo")]
        public DateTime? cupon_proximo { get; set; }

        [JsonPropertyName("cupon_ultimo")]
        public DateTime? cupon_ultimo { get; set; }

        [JsonPropertyName("cupon_consec")]
        public int? cupon_consec { get; set; }

        [JsonPropertyName("cod_app")]
        public string? cod_app { get; set; }

        [JsonPropertyName("rend_numero")]
        public string? rend_numero { get; set; }

        [JsonPropertyName("rend_aplica")]
        public string? rend_aplica { get; set; }

        [JsonPropertyName("rend_monto")]
        public decimal? rend_monto { get; set; }

        [JsonPropertyName("ultimo_mov")]
        public DateTime? ultimo_mov { get; set; }

        [JsonPropertyName("tasa_ajuste")]
        public decimal? tasa_ajuste { get; set; }

        [JsonPropertyName("tasa_original")]
        public decimal? tasa_original { get; set; }

        [JsonPropertyName("analista_revision")]
        public string? analista_revision { get; set; }

        [JsonPropertyName("analista_recepcion")]
        public string? analista_recepcion { get; set; }

        [JsonPropertyName("permite_giro_terceros")]
        public string? permite_giro_terceros { get; set; }

        [JsonPropertyName("sobre_giro")]
        public string? sobre_giro { get; set; }

        [JsonPropertyName("cuenta_cliente")]
        public string? cuenta_cliente { get; set; }

        [JsonPropertyName("tarjeta_numero")]
        public string? tarjeta_numero { get; set; }

        [JsonPropertyName("tarjeta_estado")]
        public string? tarjeta_estado { get; set; }

        [JsonPropertyName("tarjeta_estado_fecha")]
        public DateTime? tarjeta_estado_fecha { get; set; }

        [JsonPropertyName("tarjeta_saldo_update")]
        public bool tarjeta_saldo_update { get; set; }

        [JsonPropertyName("tarjetas_ret_efectivo")]
        public decimal? tarjetas_ret_efectivo { get; set; }

        [JsonPropertyName("monto_transito")]
        public decimal? monto_transito { get; set; }

        [JsonPropertyName("cashback_pts_otorgados")]
        public decimal? cashback_pts_otorgados { get; set; }

        [JsonPropertyName("cashback_pts_redimidos")]
        public decimal? cashback_pts_redimidos { get; set; }

        [JsonPropertyName("cashback_pts_corte")]
        public decimal? cashback_pts_corte { get; set; }

        [JsonPropertyName("bono_salario_fecha")]
        public DateTime? bono_salario_fecha { get; set; }

        [JsonPropertyName("bono_salario_usuario")]
        public string? bono_salario_usuario { get; set; }

        [JsonPropertyName("bono_salario_remesa")]
        public string? bono_salario_remesa { get; set; }

        [JsonPropertyName("bono_salario_venta_id")]
        public string? bono_salario_venta_id { get; set; }

        [JsonPropertyName("tipo_deduc")]
        public string? tipo_deduc { get; set; }

        [JsonPropertyName("porc_deduc")]
        public decimal? porc_deduc { get; set; }

        [JsonPropertyName("int_ajustado")]
        public decimal? int_ajustado { get; set; }

        [JsonPropertyName("cuenta_iban")]
        public string? cuenta_iban { get; set; }

        [JsonPropertyName("retiros_ath")]
        public int? retiros_ath { get; set; }

        [JsonPropertyName("fecha_ult_cashback_vencidos")]
        public DateTime? fecha_ult_cashback_vencidos { get; set; }

        [JsonPropertyName("cashback_pts_vencidos")]
        public decimal? cashback_pts_vencidos { get; set; }

        [JsonPropertyName("pago_cuponescdp")]
        public bool? pago_cuponescdp { get; set; }

        [JsonPropertyName("idcupon_frecuencia")]
        public string? idcupon_frecuencia { get; set; }

        [JsonPropertyName("tipo_ces")]
        public string? tipo_ces { get; set; }

        [JsonPropertyName("id_per_tasa")]
        public string? id_per_tasa { get; set; }

        [JsonPropertyName("num_oport_crm")]
        public string? num_oport_crm { get; set; }

        [JsonPropertyName("cod_fase_crm")]
        public string? cod_fase_crm { get; set; }

        [JsonPropertyName("ind_final_crm")]
        public string? ind_final_crm { get; set; }
        public DateTime? tarjeta_exp_fecha { get; set; }
        public string? tasa_preferencial_aplica { get; set; }
        public string? cod_tasa_ref { get; set; }
        public string? cdp_paga_cupon { get; set; }
        public string? cliente { get; set; }
        public string? vendedor { get; set; }
        public string? plandesc { get; set; }
        public int cuenta_maestra { get; set; }
        public bool tipo_cdp { get; set; }
        public string? cdp_paga_cupon_cfg { get; set; }
        public string? planpermitegt { get; set; }
        public string? cod_moneda { get; set; }
        public int bancoid { get; set; }
        public string? bancodesc { get; set; }
        public int? frecuencia_dias { get; set; }
        public int? frecuencia_meses { get; set; }
        public int? dias_inversion { get; set; }
        public string? plazo_id { get; set; }
        public string? plazo_desc { get; set; }
        public int? plazo_dias { get; set; }
        public int? plazo_meses { get; set; }
        public string? frecuencia_cupon_id { get; set; }
        public string? frecuencia_cupon_desc { get; set; }
        public bool aplicaBeneficiarios { get; set; }
        public decimal intereses { get; set; }
        public decimal tasa { get; set; }
        public bool isNew { get; set; }
        public string?  mTipoDeduc { get; set; }
        public decimal? mPorcRef { get; set; }
        public string? mSubCuentasMax { get; set; }
        public decimal? vMontoMin { get; set; }
        public decimal? vPlazoMin { get; set; }
        public decimal? vInversionMin { get; set; }
        public bool? vTipoCDP { get; set; }
        public bool? vCDPCuponesAplica { get; set; }
        public decimal? vTasaMargenNegociacion { get; set; }
        public int? subcuentasmax { get; set; }
        public decimal? porcentaje { get; set; }
    }

    public class ContratosPlanModels
    {
        public string? descripcion { get; set; }
        public string? tipo_deduc { get; set; }
        public decimal? porc_deduc { get; set; }
        public int? tipo_cdp { get; set; }
        public int? pago_cupones { get; set; }
        public int? deducir_planilla { get; set; }
        public DateTime? web_vence { get; set; }
        public decimal monto_minimo { get; set; }
        public decimal plazo_minimo{ get; set; }
        public int? cuenta_maestra { get; set; }
        public decimal inversion_minimo { get; set; }
        public int? planPermiteGT { get; set; }
        public string? cod_moneda { get; set; }
        public decimal tasa_margen_negociacion { get; set; }
        public DateTime? fechaServidor { get; set; } 
        public int subcuentasmax { get; set; }
    }

    public class FndContratosListaData
    {
        public int total { get; set; }
        public List<FndContratosModels>? lineas { get; set; }
    }

    public class FndContratosModels 
    {
        public string? cod_plan { get; set; }
        public int cod_contrato { get; set; }
        public string? cedula { get; set; }
        public string? fecha_inicio { get; set; }
    }

    public class ValidaContratos
    {
        public int? plazo_valida { get; set; }
        public int? acceso_valida { get; set; }
        public int? destinos { get; set; }
        public int? plazo_minimo { get; set; }
        public int? monto_minimo { get; set; }
        public int? inversion_minimo { get; set; }
    }

    public class FndContratosLiquidacionesListaData
    {
        public int total { get; set; }
        public List<FndContratosLiquidacionesModels>? lineas { get; set; }
    }

    public class FndContratosLiquidacionesModels
    {
        public int consec { get; set; }
        public DateTime fecha { get; set; }
        public decimal aportes_liq { get; set; }
        public decimal rendi_liq { get; set; }
        public string? estado { get; set; }
        public string? usuario { get; set; }
    }

    public class FndContratosCuponesData
    {
        public int cupon_id { get; set; }
        public string? cod_operadora { get; set; }
        public string? cod_plan { get; set; }
        public string? cod_contrato { get; set; }
        public int consec { get; set; }
        public decimal monto_base { get; set; }
        public decimal tasa_aplicada { get; set; }
        public decimal cupon_monto { get; set; }
        public DateTime registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public int dias { get; set; }
        public string? estado { get; set; }
        public decimal rendimiento { get; set; }
        public decimal principal { get; set; }
        public DateTime fecha_vence { get; set; }
        public DateTime? fecha_pago { get; set; }
        public string? tcon { get; set; }
        public string? ncon { get; set; }
        public decimal rendimiento_diario { get; set; }
        public string? cedula { get; set; }
        public string? id_alterno { get; set; }
        public string? nombre { get; set; }
        public string? plan_desc { get; set; }
        public string? cod_moneda { get; set; }
        public string? currency_sim { get; set; }
        public string? divisa_desc { get; set; }
        public string? estado_desc { get; set; }
        public decimal total_girar { get; set; }
        public decimal multa_retiro { get; set; }
        public decimal otros_rebajos { get; set; }
        public decimal isr_porc { get; set; }
        public decimal isr_mnt_gravable { get; set; }
        public decimal isr_monto { get; set; }
        public string? tesoreria_id { get; set; }
        public string? bancos_tipo { get; set; }
        public string? bancos_estado { get; set; }
        public DateTime? bancos_fecha_emite { get; set; }
        public string? tes_documento { get; set; }
        public string? iban { get; set; }
    }

    public class FndContratoBitacoraData
    {
        public DateTime fecha { get; set; }
        public string? usuario { get; set; }
        public string? movimientodesc { get; set; }
        public string? detalle { get; set; }
        public string? revisado_usuario { get; set; }
        public DateTime revisado_fecha { get; set; }
    }

    public class FndContratoSubCuentasData
    {
        public int idx { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public decimal cuota { get; set; }

        public int cod_operadora { get; set; }
        public string? cod_plan { get; set; }
        public long cod_contrato { get; set; }
        public bool isNew { get; set; }
    }

    public class FndContratoBeneficiariosData
    {
        public string? cedulabn { get; set; }
        public string? nombre { get; set; }
        public decimal porcentaje { get; set; }
        public string? parentesco { get; set; }
        public string? parentesco_desc { get; set; }
    }

    public class FndContratoDestinoData
    {
        public int id_destino { get; set; }
        public string? descripcion { get; set; }
        public int? id_registro { get; set; }
        public string? cod_plan { get; set; }
        public long cod_contrato { get; set; }
        public string? observaciones { get; set; }
        public DateTime fec_registro { get; set; }
        public string? usu_registro { get; set; }
        public Nullable<DateTime> fec_modifica { get; set; }
        public string? usu_modifica { get; set; }
        public int? cod_operadora { get; set; }
    }

    public class FndContratoTasaPreferencial
    {
        public decimal? tasa_calculada { get; set; } = 0;
        public decimal? margen_maximo { get; set; } = 0;
        public decimal? tasa_solicitada { get; set; } = 0;
        public string? id_tp { get; set; }
        public string? estado_desc { get; set; }
        public int? operadora { get; set; }
        public string? cod_plan { get; set; }
        public long? contrato { get; set; }
        public string? cedula { get; set; }
        public string? usuario { get; set; }
        public decimal? inversion { get; set; } = 0;
        public int? frecuencia { get; set; } = 0;
        public int? plazo { get; set; } = 0;
        public string? notas { get; set; }
    }

    public class FndCambios
    {
        public decimal vPlazo { get; set; }
        public decimal vCuota { get; set; }
        public decimal vInversion { get; set; }
        public string? vDescPlazo { get; set; }
        public bool vDedPlanilla { get; set; }
    }

    public class FndSociosListaData
    {
        public int total { get; set; }
        public List<DropDownListaGenericaModel>? socios { get; set; }
    }

    public class FndSolicitudTpData
    {
        public long? gestion_id { get; set; }
        public string? gestion_estado { get; set; }
    }
}