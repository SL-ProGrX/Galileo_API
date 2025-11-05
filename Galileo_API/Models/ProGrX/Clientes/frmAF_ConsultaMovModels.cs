namespace PgxAPI.Models.AF
{
    public class AfiConsultaMovIngresos
    {
        public int consec { get; set; }
        public string cedula { get; set; } = string.Empty;
        public DateTime fecha_ingreso { get; set; }
        public string? id_promotor { get; set; }
        public string? boleta { get; set; }
        public string? usuario { get; set; }
        public Nullable<DateTime> fecha { get; set; }
        public string? cod_oficina { get; set; }
        public string? cod_remesa { get; set; }
        public string? analista_revision { get; set; }
        public string? analista_recepcion { get; set; }
        public string? estado { get; set; }
        public string? afiliacion_digital { get; set; }
        public string? enviado_archivo { get; set; }
        public string? tipo_ing { get; set; }
        public string? promotor { get; set; }
    }

    public class AfiConsultaMovRenuncias
    {
        public int consec { get; set; }
        public string id_causa { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string? tipo { get; set; }
        public string? id_promotor { get; set; }
        public string? id_boleta { get; set; }
        public Nullable<DateTime> fecharena { get; set; }
        public string? tiporen { get; set; }
        public string? nacta { get; set; }
        public string? ncausaren { get; set; }
        public string? renmor { get; set; }
        public string? descripcion { get; set; }
    }

    public class AfiConsultaMovLiquidaciones
    {
        public int consec { get; set; }
        public string cedula { get; set; } = string.Empty;
        public float ahorro { get; set; }
        public float aporte { get; set; }
        public float extra { get; set; }
        public float capitaliza { get; set; }
        public float totalbruto { get; set; }
        public float tneto { get; set; }
        public float retenido { get; set; }
        public string? aplahorro { get; set; }
        public string? aplaporte { get; set; }
        public string? aplextra { get; set; }
        public string? aplcapitalizado { get; set; }
        public Nullable<DateTime> fecliq { get; set; }
        public Nullable<DateTime> fechaingreso { get; set; }
        public string? estadoactliq { get; set; }
        public float montofci { get; set; }
        public float ahorro_liq { get; set; }
        public float aporte_liq { get; set; }
        public float extra_liq { get; set; }
        public float capitalizado_liq { get; set; }
        public string? tdocumento { get; set; }
        public string? ndocumento { get; set; }
        public string? cod_banco { get; set; }
        public string? ubicacion { get; set; }
        public string? estadoasiento { get; set; }
        public string? liq_tcon { get; set; }
        public Nullable<DateTime> fecha_traspaso { get; set; }
        public string? id_causa { get; set; }
        public string? estado { get; set; }
        public string? estadoactual { get; set; }
        public string? mortalidad { get; set; }
        public string? observacion { get; set; }
        public string? usuario { get; set; }
        public string? cod_oficina { get; set; }
        public string? ac_boleta { get; set; }
        public Nullable<DateTime> ac_fecha { get; set; }
        public string? traspaso_usuario { get; set; }
        public string? tesoreria_solicitud { get; set; }
        public string? aplica_reingreso { get; set; }
        public string? cod_remesa { get; set; }
        public string? tes_supervision_usuario { get; set; }
        public Nullable<DateTime> tes_supervision_fecha { get; set; }
        public string? id_token { get; set; }
        public string? analista_revision { get; set; }
        public string? analista_recepcion { get; set; }
        public string? cta_ahorros { get; set; }
        public float excedente_periodo { get; set; }
        public float excedente_ir { get; set; }
        public float excedente_liq { get; set; }
        public float excedente_ir_liq { get; set; }
        public string? apl_excedente { get; set; }
        public float custodia { get; set; }
        public float custodia_liq { get; set; }
        public string? cod_plan { get; set; }
        public string? cod_contrato { get; set; }
        public string? cod_divisa { get; set; }
        public string? tipo_cambio { get; set; }
        public Nullable<DateTime> fecha_pago { get; set; }
        public float impuestos { get; set; }
        public string? retenido_cj { get; set; }
        public float total_intdev { get; set; }
        public string? id_documento { get; set; }
        public string? ind_cbr_jud { get; set; }
        public string? cj_plan { get; set; }
        public string? cj_contrato { get; set; }
        public string? estadopersona { get; set; }
    }

    public class AFCedulaDto
    {
        public string? cedula { get; set; }
        public string? cedulaR { get; set; }
        public string? nombre { get; set; }
    }
}