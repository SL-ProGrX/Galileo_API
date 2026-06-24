namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrCatalogoCreditoData
    {
        public string codigo { get; set; } = string.Empty;
        public string codigoa { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public required bool activo { get; set; }
        public required bool linea_interna { get; set; }
        public required bool deduc_codigo_alter { get; set; }
        public required bool filtra_refundibles { get; set; }
        public required bool permite_persona_en_cbr_jud { get; set; }
        public string convenio { get; set; } = "N";
        public string poliza { get; set; } = "N";
        public string refunde { get; set; } = "N";
        public string retencion { get; set; } = "N";
        public string aceptarefun { get; set; } = "N";
        public string primer_cuota { get; set; } = "N";
        public string pidecheque { get; set; } = "N";
        public required bool retencion_muestra_saldo { get; set; }
        public required bool cobertura { get; set; }
        public required bool genera_mora { get; set; }
        public required bool movcajas { get; set; }
        public string tramite { get; set; } = "C";
        public string requisitos_tipo { get; set; } = "L";
        public required int id_comite { get; set; }
        public string comitedesc { get; set; } = string.Empty;
        public string cod_institucion { get; set; } = string.Empty;
        public string instituciondesc { get; set; } = string.Empty;
        public string divisaid { get; set; } = string.Empty;
        public string divisadesc { get; set; } = string.Empty;
        public required int tramitedias { get; set; }
        public required int operaciones_activas { get; set; }
        public required int membresia_meses { get; set; }
        public required decimal refunde_porc { get; set; }
        public string refunde_tipo { get; set; } = "P";
        public required decimal porc_cargo_cancelacion { get; set; }
        public required int anticipo_meses { get; set; }
        public string liq_tipoaumento { get; set; } = "F";
        public required decimal liq_valor { get; set; }
        public string base_calculo { get; set; } = string.Empty;
        public string cobro_tipo_aplicacion { get; set; } = "V";
        public required bool fecha_corte_alterna { get; set; }
        public DateTime? fechacorte { get; set; }
        public required bool tasa_destino { get; set; }
        public required bool tbp_utiliza { get; set; }
        public required decimal tbp_adicional { get; set; }
        public string tasa_mora_tipo { get; set; } = "N/A";
        public required decimal tasa_mora_add { get; set; }
        public required bool tasa_fija_x_tbp { get; set; }
        public required decimal tasa_fija_x_tbp_puntos_add { get; set; }
        public required int plazo_tasa_fija { get; set; }
        public required bool oficina_linea { get; set; }
        public string oficina { get; set; } = string.Empty;
        public string oficina_desc { get; set; } = string.Empty;
        public required bool website { get; set; }
        public required bool visible_ec { get; set; }
        public required bool forma_pago_pos { get; set; }
        public required bool forma_pago_web { get; set; }
        public required bool auto_gestion_lmax { get; set; }
        public required decimal giro_max_transac { get; set; }
        public required bool giro_automatico { get; set; }
        public required decimal giro_monto_base { get; set; }
        public required decimal giro_minimo { get; set; }
        public string auto_gestion_tipo { get; set; } = "C";
        public required bool refunde_auto { get; set; }
        public required bool refunde_aumenta_base { get; set; }
        public required bool ind_notifica_cli_formaliza { get; set; }
        public required bool ind_notifica_cli_cancela { get; set; }
        public required bool ind_mov_aplica_bonif { get; set; }
        public required bool ind_pago_op_aplicacion { get; set; }
        public required bool ind_readecua { get; set; }
        public required bool ind_monto_max { get; set; }
        public required bool id_req_supervision { get; set; }
        public required decimal monto_supervision { get; set; }
        public required decimal porc_anticipo_ext { get; set; }
        public required bool ind_edad_pension_est { get; set; }
        public required bool ind_edad_pension_for { get; set; }
        public required bool mov_sinpe { get; set; }
        public required int mov_sinpe_tipos { get; set; } = 3;
        public required bool cph1 { get; set; }
        public required bool cph2 { get; set; }
        public required bool cph3 { get; set; }
        public required bool reserva_aplica { get; set; }
        public required bool reserva_facial_flat { get; set; }
        public required bool reserva_mora_apl { get; set; }
        public string reserva_codigo { get; set; } = string.Empty;
        public string reserva_plan_desc { get; set; } = string.Empty;
        public required decimal reserva_monto_minimo { get; set; }
        public required bool revolutiva { get; set; }
        public required bool revolutiva_tope_retiros { get; set; }
        public required bool revolutiva_estudio { get; set; }
        public required bool revolutiva_plan_ahorro_utiliza { get; set; }
        public string revolutiva_plan_ahorro { get; set; } = string.Empty;
        public string plan_ahorro_desc { get; set; } = string.Empty;
        public string df_descripcion_linea { get; set; } = string.Empty;
        public string df_uso_destino_linea { get; set; } = "Consumo";
        public string df_logo_url { get; set; } = "https://www.progrxweb.com/Credito_Consumo_128.png";
        public string df_etiqueta_aprobacion { get; set; } = "Sin Estudio de CrÃ©dito";
        public string df_etiqueta_monto_max { get; set; } = "Hasta 000,000.00 segÃºn disponible";
        public string df_etiqueta_plazo_tasa { get; set; } = "Plazo de xx meses con tasa del 00.00%";
        public string df_etiqueta_deposito { get; set; } = "DepÃ³sito en 24 hrs hÃ¡biles";
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
        public required bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionCargoData
    {
        public string cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public required decimal valor { get; set; }
        public required bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionRequisitoData
    {
        public string requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool opcional { get; set; }
        public required bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionRecursoData
    {
        public string recurso { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionCarteraData
    {
        public string cartera { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool asignado { get; set; }
    }

    public class CrCatalogoCreditoAsignacionRefundibleData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool refunde { get; set; }
    }

    public class CrCatalogoCreditoAdjuntoData
    {
        public string id { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool opcional { get; set; }
        public required bool asignado { get; set; }
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
        public required bool asignado { get; set; }
        public required bool opcional { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrCatalogoCreditoRangoBaseData
    {
        public required int consec { get; set; }
        public required decimal de { get; set; }
        public required decimal hasta { get; set; }
        public required int plazo { get; set; }
        public required decimal intc_soc { get; set; }
        public required decimal intm_soc { get; set; }
        public required decimal intc_nsoc { get; set; }
        public required decimal intm_nsoc { get; set; }
    }

    public class CrCatalogoCreditoRangoPlazoData
    {
        public required int consec { get; set; }
        public required int desde { get; set; }
        public required int hasta { get; set; }
        public required decimal tasa { get; set; }
    }

    public class CrCatalogoCreditoRangoGarantiaData
    {
        public string garantia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool utiliza_tasa_garantia { get; set; }
        public required decimal tasa_garantia { get; set; }
        public required bool utiliza_tasa_piso { get; set; }
        public required decimal tasa_piso { get; set; }
        public required bool utiliza_tasa_techo { get; set; }
        public required decimal tasa_techo { get; set; }
        public required bool utiliza_maximos { get; set; }
        public required decimal max_monto { get; set; }
        public required decimal liquidez_minima { get; set; }
    }

    public class CrCatalogoCreditoRangosBaseData
    {
        public List<CrCatalogoCreditoRangoBaseData> rangos { get; set; } = [];
        public List<CrCatalogoCreditoRangoPlazoData> tasasPlazos { get; set; } = [];
        public List<CrCatalogoCreditoRangoGarantiaData> garantias { get; set; } = [];
    }

    public class CrCatalogoCreditoLiquidezBonoData
    {
        public required int id { get; set; }
        public decimal? pago_inicial { get; set; }
        public decimal? pago_final { get; set; }
        public decimal? puntos_bonificacion { get; set; }
    }

    public class CrCatalogoCreditoLiquidezCapacidadData
    {
        public required int id { get; set; }
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
        public required CrCatalogoCreditoRangoBaseData rango { get; set; }
    }

    public class CrCatalogoCreditoRangoPlazoGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public required CrCatalogoCreditoRangoPlazoData rango { get; set; }
    }

    public class CrCatalogoCreditoRangoGarantiaGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public required CrCatalogoCreditoRangoGarantiaData garantia { get; set; }
    }

    public class CrCatalogoCreditoLiquidezBonoGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public required CrCatalogoCreditoLiquidezBonoData rango { get; set; }
    }

    public class CrCatalogoCreditoLiquidezCapacidadGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public required CrCatalogoCreditoLiquidezCapacidadData rango { get; set; }
    }

    public class CrCatalogoCreditoComiteEstudioData
    {
        public required int id { get; set; }
        public string linea { get; set; } = string.Empty;
        public required int id_comite { get; set; }
        public string comite { get; set; } = string.Empty;
        public required decimal porcentaje { get; set; }
    }

    public class CrCatalogoCreditoComiteEstudioGuardarRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public required CrCatalogoCreditoComiteEstudioData comite { get; set; }
    }

    public class CrCatalogoCreditoCuentaData
    {
        public string rubro { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool es_titulo { get; set; }
    }
}
