namespace Galileo.Models.ProGrX.Fondos
{
    public class FndPlanesCombosDto
    {
        public List<DropDownListaGenericaModel> TiposPlan { get; set; } = new();
        public List<DropDownListaGenericaModel> Grupos { get; set; } = new();
        public List<DropDownListaGenericaModel> Divisas { get; set; } = new();
        public List<DropDownListaGenericaModel> Operadoras { get; set; } = new();
        public List<DropDownListaGenericaModel> Lineas { get; set; } = new();
        public List<FndPlanListaDto> Planes { get; set; } = new();
    }

    public class PlanEstadoDto
    {
        public required string codestado { get; set; }
        public string? descripcion { get; set; }
        public bool asignado { get; set; }
    }

    public class PlanPlazoDto
    {
        public int plazo { get; set; }
        public string? descripcion { get; set; }
        public bool asignado { get; set; }
    }

    public class FndPlanDto
    {
        // Los campos opcionales conservan los valores históricos al omitirse en el JSON.
        public required string codplan { get; set; }
        public int? codoperadora { get; set; } = 0;
        public required string descripcion { get; set; }
        public string? notas { get; set; }
        public string? estado { get; set; }
        public string? cod_moneda { get; set; }
        public string? tipoplan { get; set; }
        public int? codtipoplan { get; set; } = 0;
        public string? grupo { get; set; }
        public string? codgrupo { get; set; }
        public string? tipodeduc { get; set; }
        public decimal? porc_deduc { get; set; } = 0;
        public int? plazo_minimo { get; set; } = 0;
        public string? plazo_tipo { get; set; }
        public decimal? monto_minimo { get; set; } = 0;
        public decimal? inversion_minimo { get; set; } = 0;
        public string? linea_codigo { get; set; }
        public string? linea_descripcion { get; set; }
        public bool? deducir_planilla { get; set; } = false;
        public bool? genera_mora { get; set; } = false;
        public bool? cdp { get; set; } = false;
        public bool? controla_saldo { get; set; } = false;
        public bool? cuenta_maestra { get; set; } = false;
        public int? subcuentas_max { get; set; } = 0;
        public decimal? tasa_margen_negociacion { get; set; } = 0;
        public bool? requiere_beneficiarios { get; set; } = false;
        public bool? deduce_independiente { get; set; } = false;
        public bool? calcula_rend { get; set; } = false;
        public int? base_calculo { get; set; } = 0;
        public decimal? tasa_base { get; set; } = 0;
        public bool? tasa_fluctuante { get; set; } = false;
        public bool? capitaliza_rend { get; set; } = false;
        public bool? utiliza_tbp { get; set; } = false;
        public bool? sirve_garantia { get; set; } = false;
        public decimal? garantia_porc_disp { get; set; } = 0;
        public decimal? garantia_tasa_ad { get; set; } = 0;
        public bool? garantia_integrada { get; set; } = false;
        public bool? mov_cajas { get; set; } = false;
        public string? mov_sinpe_tipos { get; set; }
        public bool? sinpe_proc_enlinea { get; set; } = false;
        public bool? sinpe_proc_interno { get; set; } = false;
        public bool? retiros_cajas { get; set; } = false;
        public bool? giro_terceros { get; set; } = false;
        public bool? website { get; set; } = false;
        public bool? web_liquida { get; set; } = false;
        public DateTime? web_vence { get; set; }
        public bool? renta_global { get; set; } = false;
        public string? patrimonio_tipo { get; set; }
        public string? lineadesc { get; set; }
        public string? codigo_ase { get; set; }
        public bool? mov_sinpe { get; set; } = false;
        public string? resumen_cont_tasa { get; set; } = "";
        public int? num_contratos_activos { get; set; } = 0;
        public decimal? ulttasa { get; set; }
        public int? contratos_activos_vb6 { get; set; } = 0;
        public DateTime? ultima_tasa_vb6 { get; set; }
        public int? contador_tasas_vb6 { get; set; }
        public decimal? ulttasa_vb6 { get; set; }
        public decimal? consecutivo { get; set; }
        public bool? tipo_cdp { get; set; } = false;
        public bool? pago_cupones { get; set; } = false;
        public bool? sinpe_cuenta { get; set; } = false;
        public bool? apl_rend_automatico { get; set; } = false;
        public bool? utiliza_tasa_fluctuante { get; set; } = false;
        public bool? capitaliza_rendimientos { get; set; } = false;
        public bool? tasa_ajuste_vencimiento { get; set; } = false;
        public decimal? tasa_ajuste { get; set; } = 0;
        public bool? permite_mov_cajas { get; set; } = false;
        public bool? forma_pago_pos { get; set; } = false;
        public bool? permite_retiros_cajas { get; set; } = false;
        public bool? permite_giro_terceros { get; set; } = false;
        public bool? mov_entre_fondos { get; set; } = false;
        public bool? mov_entre_fondos_terceros { get; set; } = false;
        // ====== GENERAL ======
        public bool? apl_liq_socio { get; set; } = false;
        public bool? liq_desde_ahorros { get; set; } = false;
        public bool? permite_retiros_terceros { get; set; } = false;
        public bool? visible_ec { get; set; } = false;

        // ====== AUTO GESTIÓN ======
        //public bool vence_plan { get; set; }                  // Checkbox "Vence?"
        public bool? web_crear { get; set; } = false;                   // Crear nuevos contratos
        public bool? web_modifica_couta { get; set; } = false;          // Modifica cuota
        public bool? permite_ret_parcial { get; set; } = false;         // Permite retiros parciales

        // ====== SINPE / PATRIMONIO ======
        public bool? patrimonio_enlace { get; set; } = false;           // Enlazado a Patrimonio
        public bool? patrimonio_unifica { get; set; } = false;          // Unificar en estado de cuenta patrimonio

        // ====== COMISIONES ======
        public decimal? tasa_comision_aportes { get; set; } = 0;
        public decimal? impuesto_rendimientos { get; set; } = 0;
        public decimal? tasa_comision_rend { get; set; } = 0;

        // ====== OTROS ======
        public decimal? comision_vta_inv { get; set; } = 0;
        public decimal? comision_vta_monto { get; set; } = 0;
        // ====== CAMPOS SINPE EXTRA ======
        public string? sinpe_producto { get; set; } = "";
        // ====== CAMPOS DE RESUMEN VB6 ======
        public bool? sif_liquida { get; set; } = false;
        public DateTime? fechaserver { get; set; }
        public decimal? impuesto_renta { get; set; } = 0;
        public bool? aplicar_tasa_cont_vencidos { get; set; } = false;
        public bool? aplicar_en_procs_contrs_vencidos { get; set; } = false;
        public bool? vence_renueva { get; set; } = false;
        public bool? vence_notifica { get; set; } = false;
        public bool? ind_aplicar_amora { get; set; } = false;
        public string? vence_accion { get; set; } = "";
        public string? ctaplan { get; set; }
        public string? ctaplandesc { get; set; }
        public string? ctarnd { get; set; }
        public string? ctarnddesc { get; set; }
        public string? ctagasto { get; set; }
        public string? ctagastodesc { get; set; }
        public string? ctacomisionadm { get; set; }
        public string? ctacomisionadmdesc { get; set; }
        public string? ctaingretiros { get; set; }
        public string? ctaingretirosdesc { get; set; }
        public string? ctagstcomision { get; set; }
        public string? ctagstcomisiondesc { get; set; }
        public string? ctaimpuesto { get; set; }
        public string? ctaimpuestodesc { get; set; }
        public string? vence_plan { get; set; }

    }

    public class FndPlanListaDto
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int codoperadora { get; set; }
        public bool auto_gestionable { get; set; }
        public bool activo { get; set; }
        public bool certificado_plazo { get; set; }
        public bool ts { get; set; }
    }

    public class FndHistorialRendDto
    {
        public DateTime corte { get; set; }
        public decimal tasa { get; set; }
        public decimal tcp { get; set; }
        public string usuario { get; set; } = "";
        public DateTime fecha_sys { get; set; }
    }

    public class FndPlanRetiroDto
    {
        public required int id { get; set; }
        public required int desde { get; set; }
        public required int hasta { get; set; }
        public required decimal porcentaje { get; set; }
        public string aplicar { get; set; } = string.Empty;
        public required int cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? actualiza_usuario { get; set; }
        public DateTime? actualiza_fecha { get; set; }
    }

    public class FndPlanesDestinoAhorroDto
    {
        public int id_destino { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class FndDestinoAsociadoDto
    {
        public string cod_destino { get; set; } = "";
        public string descripcion { get; set; } = "";
        public bool asignado { get; set; }
    }

    public class FndReglaTasaDto
    {
        public int id_per_tasa { get; set; }
        public string tipo_desc { get; set; } = string.Empty;
        public string vigente_desc { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public string obs_usuario { get; set; } = string.Empty;
        public string usr_registra { get; set; } = string.Empty;
        public string fec_registra { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
        public string modifica_fecha { get; set; } = string.Empty;
        public string activa_usuario { get; set; } = string.Empty;
        public string activa_fecha { get; set; } = string.Empty;
    }

    public class FndReglaTasaDetalleDto
    {
        public int cod_tabla_aum { get; set; }
        public string tipo_tasa { get; set; } = "";
        public int desde { get; set; }
        public int hasta { get; set; }
        public decimal plus { get; set; }
    }

    public class FndRetirosDto
    {
        public int id { get; set; }
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public int desde { get; set; }
        public int hasta { get; set; }
        public decimal multa { get; set; }
        public string aplicar { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FndPlanPuntoDto
    {
        public int? id { get; set; }
        public required int cod_operadora { get; set; }
        public string? cod_plan { get; set; }
        public string? vigente { get; set; }
        public string? tipo { get; set; }
        public string? fecha_referencia { get; set; }
        public string? justificacion { get; set; }
        public string? usuario { get; set; }
    }

    public class FndPlanPuntoDetalleDto
    {
        public required int id { get; set; } // COD_TABLA_AUM
        public required int cod_operadora { get; set; }
        public required string cod_plan { get; set; }
        public string tipo_tasa { get; set; } = "";
        public required int desde { get; set; }
        public required int hasta { get; set; }
        public required decimal plus { get; set; }
        public required int id_per_tasa { get; set; } // regla padre
        public string usuario { get; set; } = "";
    }

    public class FndPlanDestinoGuardarDto
    {
        public required int id { get; set; }
        public string cod_plan { get; set; } = "";
        public string descripcion { get; set; } = "";
        public required bool activo { get; set; }
        public string usuario { get; set; } = "";
    }

    public class FndPlanDestinoAsociadoDto
    {
        public required string id_destino { get; set; }
        public string cod_plan { get; set; } = "";
        public required int asociado { get; set; }  // 1 = asociar, 0 = desasociar
        public string usuario { get; set; } = "";
        public required int cod_operadora { get; set; }  // 1 = asociar, 0 = desasociar
    }

    public class EstadoAsignadoDto
    {
        public string cod_estado { get; set; } = "";
        public bool asignado { get; set; }
    }

    public class PlazoAsignadoDto
    {
        public int plazo { get; set; }
        public bool asignado { get; set; }
    }

    public class FndPlanesVencimientosGuardarDto
    {
        public string cod_plan { get; set; } = "";
        public required int cod_operadora { get; set; }
        public string usuario { get; set; } = "";

        [System.Text.Json.Serialization.JsonRequired]
        public bool ind_aplicar_amora { get; set; }
        public List<EstadoAsignadoDto> estados { get; set; } = new();
        public List<PlazoAsignadoDto> plazos { get; set; } = new();
    }

    public class FndReglaActivarDto
    {
        public required int id_regla { get; set; }
        public string usuario { get; set; } = "";
    }
}
