namespace PgxAPI.Models.ProGrX.Fondos
{
    public class FndConsultaDetalleData
    {
        public string cod_operadora { get; set; }
        public string cod_plan { get; set; }
        public int? cod_contrato { get; set; }
        public string cedula { get; set; }
        public string cod_vendedor { get; set; }
        public string estado { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public int? plazo { get; set; }
        public decimal monto { get; set; }
        public string renueva { get; set; }
        public decimal inc_anual { get; set; }
        public string inc_tipo { get; set; }
        public decimal aportes { get; set; }
        public decimal capexc { get; set; }
        public decimal rendimiento { get; set; }
        public decimal saldo_operadora { get; set; }
        public string liq_tipo { get; set; }
        public DateTime? liq_fecha { get; set; }
        public decimal liq_monto { get; set; }
        public decimal liq_retiro { get; set; }
        public decimal liq_neto { get; set; }
        public int? ind_deduccion { get; set; }
        public int? operacion { get; set; }
        public DateTime? ult_renovacion { get; set; }
        public DateTime? ult_retiro { get; set; }
        public string cod_banco { get; set; }
        public string tipo_pago { get; set; }
        public string cuenta_ahorros { get; set; }
        public int? ind_comision { get; set; }
        public DateTime? comision_fecha { get; set; }
        public string comision_tesoreria { get; set; }
        public decimal comision_monto { get; set; }
        public DateTime? rend_corte { get; set; }
        public decimal rend_saldo { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string usuario { get; set; }
        public string albacea_cedula { get; set; }
        public string albacea_nombre { get; set; }
        public string plazo_tipo { get; set; }
        public decimal inversion { get; set; }
        public decimal tasa_referencia { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string modifica_usuario { get; set; }
        public string cod_oficina { get; set; }
        public string tasa_tipo { get; set; }
        public decimal tasa_ptsadd { get; set; }
        public string cupon_frecuencia { get; set; }
        public DateTime? cupon_proximo { get; set; }
        public DateTime? cupon_ultimo { get; set; }
        public int? cupon_consec { get; set; }
        public string cod_app { get; set; }
        public double? rend_numero { get; set; }
        public int? rend_aplica { get; set; }
        public decimal rend_monto { get; set; }
        public DateTime? ultimo_mov { get; set; }
        public decimal tasa_ajuste { get; set; }
        public decimal tasa_original { get; set; }
        public string analista_revision { get; set; }
        public string analista_recepcion { get; set; }
        public int? permite_giro_terceros { get; set; }
        public decimal sobre_giro { get; set; }
        public string cuenta_cliente { get; set; }
        public string tarjeta_numero { get; set; }
        public string tarjeta_estado { get; set; }
        public DateTime? tarjeta_estado_fecha { get; set; }
        public decimal tarjeta_saldo_update { get; set; }
        public decimal tarjetas_ret_efectivo { get; set; }
        public decimal monto_transito { get; set; }
        public decimal cashback_pts_otorgados { get; set; }
        public decimal cashback_pts_redimidos { get; set; }
        public decimal cashback_pts_corte { get; set; }
        public DateTime? bono_salario_fecha { get; set; }
        public string bono_salario_usuario { get; set; }
        public string bono_salario_remesa { get; set; }
        public string bono_salario_venta_id { get; set; }
        public string tipo_deduc { get; set; }
        public decimal porc_deduc { get; set; }
        public decimal int_ajustado { get; set; }
        public string cuenta_iban { get; set; }
        public decimal retiros_ath { get; set; }
        public DateTime? fecha_ult_cashback_vencidos { get; set; }
        public decimal cashback_pts_vencidos { get; set; }
        public int? pago_cuponescdp { get; set; }
        public string idcupon_frecuencia { get; set; }
        public string tipo_ces { get; set; }
        public string id_per_tasa { get; set; }
        public int? num_oport_crm { get; set; }
        public string cod_fase_crm { get; set; }
        public int? ind_final_crm { get; set; }
        public DateTime? tarjeta_exp_fecha { get; set; }
        public string tasa_preferencial_aplica { get; set; }
        public string cod_tasa_ref { get; set; }
        public decimal disponible { get; set; }
        public string nombre { get; set; }
        public string operadora { get; set; }
        public string planx { get; set; }
    }

    public class FndConsultaContratoDetallesData
    {
        public int cod_fnd_detalle { get; set; }
        public string cod_operadora { get; set; }
        public string cod_plan { get; set; }
        public int? cod_contrato { get; set; }
        public decimal monto { get; set; }
        public string fecha_proceso { get; set; }  // Parece un periodo tipo "201804", por eso lo dejo como string
        public DateTime? fecha_acredita { get; set; }
        public DateTime? fecha { get; set; }
        public string tcon { get; set; }
        public string ncon { get; set; }
        public decimal rendimiento { get; set; }
        public string cod_app { get; set; }
        public string usuario { get; set; }
        public string cod_concepto { get; set; }
        public string cod_caja { get; set; }
        public string ref_01 { get; set; }
        public string ext_tipo_mov { get; set; }
        public int? ext_sync_asiento { get; set; }
        public DateTime? ext_fecha { get; set; }
        public string ext_ref_interna { get; set; }
        public decimal ext_comision { get; set; }
        public int? conciliacion_sat { get; set; }
        public string detalle_01 { get; set; }
        public string fp_orden { get; set; }
        public string fp_bene_id { get; set; }
        public string fp_liq { get; set; }
        public decimal aportes { get; set; }
        public decimal rnd_tasa { get; set; }
        public int? rnd_dias { get; set; }
        public string docdesc { get; set; }
        public string conceptodesc { get; set; }
    }

    public class FndConsultaSubCuentasData
    {
        public int idx { get; set; }
        public string cod_operadora { get; set; }
        public string cod_plan { get; set; }
        public int? cod_contrato { get; set; }
        public string cod_beneficiario { get; set; }
        public string estado { get; set; }
        public decimal cuota { get; set; }
        public decimal aportes { get; set; }
        public decimal rendimiento { get; set; }
        public string cedula { get; set; }
        public string nombre { get; set; }
        public DateTime? fechanac { get; set; }
        public string telefono1 { get; set; }
        public string telefono2 { get; set; }
        public string email { get; set; }
        public string direccion { get; set; }
        public string apto_postal { get; set; }
        public string notas { get; set; }
        public string parentesco { get; set; }
    }

    public class FndConsultaSubCuentasDetalleData
    {
        public int cod_fnd_detalle { get; set; }
        public int idx { get; set; }
        public string cod_operadora { get; set; }
        public string cod_plan { get; set; }
        public int? cod_contrato { get; set; }
        public decimal monto { get; set; }
        public string fecha_proceso { get; set; }  // formato tipo "201804", lo dejo como string
        public DateTime? fecha_acredita { get; set; }
        public DateTime? fecha { get; set; }
        public string tcon { get; set; }
        public string ncon { get; set; }
        public decimal rendimiento { get; set; }
        public string docdesc { get; set; }
        public string conceptodesc { get; set; }
        public string usuario { get; set; }
    }

    public class FndConsultaBeneficiarioDetalle
    {
        public string cedulabn { get; set; }
        public string nombre { get; set; }
        public decimal porcentaje { get; set; }
        public string parentesco { get; set; }
        public DateTime? fechanac { get; set; }
    }

    public class FndConsultaMovTransitoData
    {
        public int cod_transito { get; set; }
        public string cedula { get; set; }
        public string cuenta_cliente { get; set; }
        public string banco_origen { get; set; }
        public string banco_origen_desc { get; set; }
        public string cedula_origen { get; set; }
        public string cuenta_cliente_origen { get; set; }
        public string cod_referencia { get; set; }
        public string cod_servicio { get; set; }
        public string cod_moneda { get; set; }
        public decimal monto { get; set; }
        public decimal monto_comision { get; set; }
        public string accion { get; set; }
        public string transac_tipo { get; set; }
        public string transac_desc { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; }
        public string comprobante_interno { get; set; }
        public string rechazo_codigo { get; set; }
        public string rechazo_desc { get; set; }
        public string estado { get; set; }
        public DateTime? fecha_actualiza { get; set; }
    }
}
