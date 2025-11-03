namespace PgxAPI.Models.TES
{

    public class TES_TransaccionDTO
    {

        public int nsolicitud { get; set; } = 0;
        public int id_banco { get; set; }
        public string? tipo { get; set; }
        public string? codigo { get; set; }
        public string? beneficiario { get; set; }
        public decimal? monto { get; set; }
        public DateTime? fecha_solicitud { get; set; }
        public string? estado { get; set; }
        public DateTime? fecha_emision { get; set; }
        public DateTime? fecha_anula { get; set; }
        public string? estadoi { get; set; }
        public string? modulo { get; set; }
        public string? cta_ahorros { get; set; }
        public string? ndocumento { get; set; }
        public string? detalle1 { get; set; }
        public string? detalle2 { get; set; }
        public string? detalle3 { get; set; }
        public string? detalle4 { get; set; }
        public string? detalle5 { get; set; }
        public int? referencia { get; set; }
        public string? submodulo { get; set; }
        public string? genera { get; set; }
        public string? actualiza { get; set; }
        public string? ubicacion_actual { get; set; }
        public DateTime? fecha_traslado { get; set; }
        public string? ubicacion_anterior { get; set; }
        public string? entregado { get; set; }
        public string? autoriza { get; set; }
        public DateTime? fecha_asiento { get; set; }
        public DateTime? fecha_asiento2 { get; set; }
        public string estado_asiento { get; set; } = string.Empty;
        public DateTime? fecha_autorizacion { get; set; }
        public string? user_autoriza { get; set; }
        public int? op { get; set; }
        public string? detalle_anulacion { get; set; }
        public string? user_asiento_emision { get; set; }
        public string? user_asiento_anula { get; set; }
        public string? cod_concepto { get; set; }
        public string? cod_unidad { get; set; }
        public string? user_genera { get; set; }
        public string? user_solicita { get; set; }
        public string? user_anula { get; set; }
        public string? user_entrega { get; set; }
        public DateTime? fecha_entrega { get; set; }
        public string? documento_ref { get; set; }
        public string? documento_base { get; set; }
        public string? detalle { get; set; }
        public string? user_hold { get; set; }
        public DateTime? fecha_hold { get; set; }
        public DateTime? firmas_autoriza_fecha { get; set; }
        public string? firmas_autoriza_usuario { get; set; }
        public decimal? tipo_cambio { get; set; }
        public string? cod_divisa { get; set; }
        public int? tipo_beneficiario { get; set; }
        public string? cod_app { get; set; }
        public string? ref_01 { get; set; }
        public string? ref_02 { get; set; }
        public string? ref_03 { get; set; }
        public string? id_token { get; set; }
        public string? remesa_tipo { get; set; }
        public int? remesa_id { get; set; }
        public string? asiento_numero { get; set; }
        public string? asiento_numero_anu { get; set; }
        public int? concilia_id { get; set; }
        public string? concilia_tipo { get; set; }
        public DateTime? concilia_fecha { get; set; }
        public string? concilia_usuario { get; set; }
        public string? cod_plan { get; set; }
        public int? modo_protegido { get; set; }

        //Campos faltantes agregados:
        public bool? reposicion_ind { get; set; }
        public string? reposicion_usuario { get; set; }
        public DateTime? reposicion_fecha { get; set; }
        public string? reposicion_autoriza { get; set; }
        public string? reposicion_nota { get; set; }
        public string? cedula_origen { get; set; }
        public string? cta_iban_origen { get; set; }
        public int tipo_ced_origen { get; set; } = 1;
        public string? correo_notifica { get; set; }
        public string? estado_sinpe { get; set; }
        public string? id_rechazo { get; set; }
        public string? tipo_girosinpe { get; set; }
        public string? id_desembolso { get; set; }
        public int? tipo_ced_destino { get; set; }
        public string? nombre_origen { get; set; }
        public string? referencia_sinpe { get; set; }
        public string? id_banco_destino { get; set; }
        public string? razon_hold { get; set; }
        public string? documento_banco { get; set; }
        public DateTime? fecha_banco { get; set; }
        public string? referencia_bancaria { get; set; }
        public string? cod_concepto_anulacion { get; set; }
        public bool? valida_sinpe { get; set; }
        public string? usuario_autoriza_especial { get; set; }
        public string? estado_desc { get; set; }

        // Ya existentes 
        public string? banco { get; set; }
        public string? conceptox { get; set; }
        public string? unidadx { get; set; }
        public string? documentox { get; set; }
        public string? divisa_id { get; set; }
        public string? divisa_desc { get; set; }
        public string? currency_sim { get; set; }

        // Más campos faltantes
        public string? anula_concepto { get; set; }
        public string? grupo_desc_corta { get; set; }
        public string? grupo_desc { get; set; }
        public string? grupo_sfn { get; set; }
        public string? sinpe_rechazo_motivo { get; set; }
        public string? sinpe_estado { get; set; }
        public string? fondo_apl { get; set; }
        public string? reversa_doc_apl { get; set; }
        public string? cta_origen_mask { get; set; }
        public string? cta_destino_mask { get; set; }
        public string? descripcion { get; set; }

        public List<Tes_Trans_AsientoDTO>? asientoDetalle { get; set; }
    }

    public class Tes_AfectacionDTO
    {
        public string identificacion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_factura { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal total { get; set; }
        public int npago { get; set; }
        public DateTime creacion_fecha { get; set; }
    }

    public class Tes_Trans_AsientoDTO
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string debehaber { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string estado { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string? unidadx { get; set; } = string.Empty;
        public string? cod_cc { get; set; } = string.Empty;
        public string? ccx { get; set; } = string.Empty;
        public int id_banco { get; set; }
        public decimal tipo_cambio { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
    }

    public class Tes_BitacoraDTO
    {
        public int id { get; set; }
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class Tes_LocalizacionDTO
    {
        public DateTime fecha_rec { get; set; }
        public string cod_remesa { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string usuario_rec { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
    }

    public class Tes_ReImpresionesDTO
    {
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string autoriza { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class Tes_CambioFechasDTO
    {
        public DateTime fecha { get; set; }
        public string idx { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class Tes_SolicitudesData 
    {
        public int nsolicitud { get; set; }
        public string tipo { get; set; }
        public string codigo { get; set; }
        public string beneficiario { get; set; }
        public float monto { get; set; }
        public string estado { get; set; }
        public string cod_unidad { get; set; }
    }

    public class Tes_SolicitudDocParametro
    {
        public string tipo { get; set; }
        public int id_banco { get; set; }
        public string documento { get; set; }
        public int contabilidad { get; set; }
    }

    public class TesConsultaAsientos
    {
        public int CodEmpresa { get; set; }
        public int? solicitud { get; set; }
        public int contabilidad { get; set; }
        public float? tipoCambio { get; set; } = 1;
        public string? divisa { get; set; } = "DOL";
        public string? estado { get; set; } = "P";
        public decimal monto { get; set; } = 0;
        public int id_banco = 0;
        public string? cod_unidad { get; set; }
        public string? cod_concepto { get; set; }
        public string? tipo { get; set; } = "";
    }

    public class TesControlDivisas
    {
        public float? gTipoCambio { get; set; } //tc_compra
        public float? gVariacion { get; set; } //rs!variacion
        public string? gDivisaDesc { get; set; } //rs!Divisa_Desc & ""
        public string? gDivisa { get; set; } //rs!cod_Divisa
        public string? gDivisaCurrency { get; set; } //rs!CURRENCY_SIM
        public float? pDivisaLocal { get; set; }//rs!divisa_local
    }

    public class TesControlDivisasData
    {
        public float? tc_compra { get; set; } //tc_compra
        public float? variacion { get; set; } //rs!variacion
        public string? cod_divisa { get; set; } //rs!Divisa_Desc & ""
        public float? divisa_local { get; set; } //rs!cod_Divisa
        public string? divisa_desc { get; set; } //rs!CURRENCY_SIM
        public string? currency_sim { get; set; }//rs!divisa_local

        public string? Descripcion { get; set; }
    }

    public class TesBitacoraTransaccion
    {
        public int cod_bitacora { get; set; }
        public string? usuario { get; set; }
        public Nullable<DateTime> fecha_hora { get; set; }
        public string? movimiento { get; set; }
        public string? detalle { get; set; }
        public string? app_nombre { get; set; }
    }

    public class TesCuentasBancarias
    {
        public string? cuenta_interna { get; set; }
        public string? cuenta_desc { get; set; }
        public string? idx { get; set; }
        public string? itmx { get; set; }
        public string? prioridad { get; set; }
    }

}
