namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrCatalogoCreditoData
    {
        public string codigo { get; set; } = string.Empty;
        public string codigoa { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public bool activo { get; set; }
        public bool linea_interna { get; set; }
        public bool deduc_codigo_alter { get; set; }
        public bool filtra_refundibles { get; set; }
        public bool permite_persona_en_cbr_jud { get; set; }
        public string convenio { get; set; } = "N";
        public string poliza { get; set; } = "N";
        public string refunde { get; set; } = "N";
        public string retencion { get; set; } = "N";
        public string aceptarefun { get; set; } = "N";
        public string primer_cuota { get; set; } = "N";
        public string pidecheque { get; set; } = "N";
        public bool retencion_muestra_saldo { get; set; }
        public bool cobertura { get; set; }
        public bool genera_mora { get; set; }
        public bool movcajas { get; set; }
        public string tramite { get; set; } = "C";
        public string requisitos_tipo { get; set; } = "L";
        public int id_comite { get; set; }
        public string comitedesc { get; set; } = string.Empty;
        public string cod_institucion { get; set; } = string.Empty;
        public string instituciondesc { get; set; } = string.Empty;
        public string divisaid { get; set; } = string.Empty;
        public string divisadesc { get; set; } = string.Empty;
        public int tramitedias { get; set; }
        public int operaciones_activas { get; set; }
        public int membresia_meses { get; set; }
        public decimal refunde_porc { get; set; }
        public string refunde_tipo { get; set; } = "P";
        public decimal porc_cargo_cancelacion { get; set; }
        public int anticipo_meses { get; set; }
        public string liq_tipoaumento { get; set; } = "F";
        public decimal liq_valor { get; set; }
        public string base_calculo { get; set; } = string.Empty;
        public string cobro_tipo_aplicacion { get; set; } = "V";
        public bool fecha_corte_alterna { get; set; }
        public DateTime? fechacorte { get; set; }
        public bool tasa_destino { get; set; }
        public bool tbp_utiliza { get; set; }
        public decimal tbp_adicional { get; set; }
        public string tasa_mora_tipo { get; set; } = "N/A";
        public decimal tasa_mora_add { get; set; }
        public bool tasa_fija_x_tbp { get; set; }
        public decimal tasa_fija_x_tbp_puntos_add { get; set; }
        public int plazo_tasa_fija { get; set; }
        public bool oficina_linea { get; set; }
        public string oficina { get; set; } = string.Empty;
        public string oficina_desc { get; set; } = string.Empty;
        public bool website { get; set; }
        public bool visible_ec { get; set; }
        public bool forma_pago_pos { get; set; }
        public bool forma_pago_web { get; set; }
        public bool auto_gestion_lmax { get; set; }
        public decimal giro_max_transac { get; set; }
        public bool giro_automatico { get; set; }
        public decimal giro_monto_base { get; set; }
        public decimal giro_minimo { get; set; }
        public string auto_gestion_tipo { get; set; } = "C";
        public bool refunde_auto { get; set; }
        public bool refunde_aumenta_base { get; set; }
        public bool ind_notifica_cli_formaliza { get; set; }
        public bool ind_notifica_cli_cancela { get; set; }
        public bool ind_mov_aplica_bonif { get; set; }
        public bool ind_pago_op_aplicacion { get; set; }
        public bool ind_readecua { get; set; }
        public bool ind_monto_max { get; set; }
        public bool id_req_supervision { get; set; }
        public decimal monto_supervision { get; set; }
        public decimal porc_anticipo_ext { get; set; }
        public bool ind_edad_pension_est { get; set; }
        public bool ind_edad_pension_for { get; set; }
        public bool mov_sinpe { get; set; }
        public int mov_sinpe_tipos { get; set; } = 3;
        public bool cph1 { get; set; }
        public bool cph2 { get; set; }
        public bool cph3 { get; set; }
        public bool reserva_aplica { get; set; }
        public bool reserva_facial_flat { get; set; }
        public bool reserva_mora_apl { get; set; }
        public string reserva_codigo { get; set; } = string.Empty;
        public string reserva_plan_desc { get; set; } = string.Empty;
        public decimal reserva_monto_minimo { get; set; }
        public bool revolutiva { get; set; }
        public bool revolutiva_tope_retiros { get; set; }
        public bool revolutiva_estudio { get; set; }
        public bool revolutiva_plan_ahorro_utiliza { get; set; }
        public string revolutiva_plan_ahorro { get; set; } = string.Empty;
        public string plan_ahorro_desc { get; set; } = string.Empty;
        public string df_descripcion_linea { get; set; } = string.Empty;
        public string df_uso_destino_linea { get; set; } = "Consumo";
        public string df_logo_url { get; set; } = "https://www.progrxweb.com/Credito_Consumo_128.png";
        public string df_etiqueta_aprobacion { get; set; } = "Sin Estudio de Crédito";
        public string df_etiqueta_monto_max { get; set; } = "Hasta 000,000.00 según disponible";
        public string df_etiqueta_plazo_tasa { get; set; } = "Plazo de xx meses con tasa del 00.00%";
        public string df_etiqueta_deposito { get; set; } = "Depósito en 24 hrs hábiles";
        public string df_color_caja { get; set; } = "#415CBF";
    }

    public class CrCatalogoCreditoGuardarRequest : CrCatalogoCreditoData
    {
        public string usuario { get; set; } = string.Empty;
    }

    public class CrCatalogoCreditoPeLGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string df_descripcion_linea { get; set; } = string.Empty;
        public string df_uso_destino_linea { get; set; } = string.Empty;
        public string df_logo_url { get; set; } = string.Empty;
        public string df_etiqueta_aprobacion { get; set; } = string.Empty;
        public string df_etiqueta_monto_max { get; set; } = string.Empty;
        public string df_etiqueta_plazo_tasa { get; set; } = string.Empty;
        public string df_etiqueta_deposito { get; set; } = string.Empty;
        public string df_color_caja { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrCatalogoCreditoAsignacionDestinoData
    {
        public string destino { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionCargoData
    {
        public string cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public decimal valor { get; set; }
        public bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionRequisitoData
    {
        public string requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool opcional { get; set; }
        public bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionRecursoData
    {
        public string recurso { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionCarteraData
    {
        public string cartera { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionRefundibleData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool refunde { get; set; }
    }

    public class CrCatalogoCreditoAdjuntoData
    {
        public string id { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool opcional { get; set; }
        public bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionesData
    {
        public List<CrCatalogoCreditoAsignacionDestinoData> destinos { get; set; } = [];
        public List<CrCatalogoCreditoAsignacionCargoData> cargos { get; set; } = [];
        public List<CrCatalogoCreditoAsignacionRequisitoData> requisitos { get; set; } = [];
        public List<CrCatalogoCreditoAsignacionRecursoData> recursos { get; set; } = [];
        public List<CrCatalogoCreditoAsignacionCarteraData> cartera { get; set; } = [];
        public List<CrCatalogoCreditoAsignacionRefundibleData> refundibles { get; set; } = [];
    }

    public class CrCatalogoCreditoAsignacionGuardarRequest
    {
        public string tipo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string codigo_asignacion { get; set; } = string.Empty;
        public bool asignado { get; set; }
        public bool opcional { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrCatalogoCreditoRangoBaseData
    {
        public int consec { get; set; }
        public decimal de { get; set; }
        public decimal hasta { get; set; }
        public int plazo { get; set; }
        public decimal intc_soc { get; set; }
        public decimal intm_soc { get; set; }
        public decimal intc_nsoc { get; set; }
        public decimal intm_nsoc { get; set; }
    }

    public class CrCatalogoCreditoRangoPlazoData
    {
        public int consec { get; set; }
        public int desde { get; set; }
        public int hasta { get; set; }
        public decimal tasa { get; set; }
    }

    public class CrCatalogoCreditoRangoGarantiaData
    {
        public string garantia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool utiliza_tasa_garantia { get; set; }
        public decimal tasa_garantia { get; set; }
        public bool utiliza_tasa_piso { get; set; }
        public decimal tasa_piso { get; set; }
        public bool utiliza_tasa_techo { get; set; }
        public decimal tasa_techo { get; set; }
        public bool utiliza_maximos { get; set; }
        public decimal max_monto { get; set; }
        public decimal liquidez_minima { get; set; }
    }

    public class CrCatalogoCreditoRangosBaseData
    {
        public List<CrCatalogoCreditoRangoBaseData> rangos { get; set; } = [];
        public List<CrCatalogoCreditoRangoPlazoData> tasasPlazos { get; set; } = [];
        public List<CrCatalogoCreditoRangoGarantiaData> garantias { get; set; } = [];
    }

    public class CrCatalogoCreditoLiquidezBonoData
    {
        public int id { get; set; }
        public decimal? pago_inicial { get; set; }
        public decimal? pago_final { get; set; }
        public decimal? puntos_bonificacion { get; set; }
    }

    public class CrCatalogoCreditoLiquidezCapacidadData
    {
        public int id { get; set; }
        public decimal? capacidad_inicio { get; set; }
        public decimal? capacidad_corte { get; set; }
        public decimal? porc_giro_maximo { get; set; }
        public decimal? porcentaje_olgura { get; set; }
    }

    public class CrCatalogoCreditoRangosLiquidezData
    {
        public List<CrCatalogoCreditoLiquidezBonoData> bono { get; set; } = [];
        public List<CrCatalogoCreditoLiquidezCapacidadData> capacidad { get; set; } = [];
    }

    public class CrCatalogoCreditoRangoBaseGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public CrCatalogoCreditoRangoBaseData rango { get; set; } = new();
    }

    public class CrCatalogoCreditoRangoPlazoGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public CrCatalogoCreditoRangoPlazoData rango { get; set; } = new();
    }

    public class CrCatalogoCreditoRangoGarantiaGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public CrCatalogoCreditoRangoGarantiaData garantia { get; set; } = new();
    }

    public class CrCatalogoCreditoLiquidezBonoGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public CrCatalogoCreditoLiquidezBonoData rango { get; set; } = new();
    }

    public class CrCatalogoCreditoLiquidezCapacidadGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public CrCatalogoCreditoLiquidezCapacidadData rango { get; set; } = new();
    }

    public class CrCatalogoCreditoComiteEstudioData
    {
        public int id { get; set; }
        public string linea { get; set; } = string.Empty;
        public int id_comite { get; set; }
        public string comite { get; set; } = string.Empty;
        public decimal porcentaje { get; set; }
    }

    public class CrCatalogoCreditoComiteEstudioGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public CrCatalogoCreditoComiteEstudioData comite { get; set; } = new();
    }

    public class CrCatalogoCreditoCuentaData
    {
        public string rubro { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool es_titulo { get; set; }
    }
}
