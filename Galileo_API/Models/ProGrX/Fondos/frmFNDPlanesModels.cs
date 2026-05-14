namespace Galileo.Models.ProGrX.Fondos
{
    public class FndPlanesCombosDto
    {
        public List<DropDownListaGenericaModel> TiposPlan { get; set; } = new();
        public List<DropDownListaGenericaModel> Grupos { get; set; } = new();
        public List<DropDownListaGenericaModel> Divisas { get; set; } = new();
        public List<DropDownListaGenericaModel> Operadoras { get; set; } = new();
        public List<DropDownListaGenericaModel> Lineas { get; set; } = new();
        public List<DropDownListaGenericaModel> Planes { get; set; } = new();
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
        public required string codplan { get; set; }
        public int codoperadora { get; set; }
        public required string descripcion { get; set; }
        public required string notas { get; set; }
        public required string estado { get; set; }
        public required string cod_moneda { get; set; }
        public string? tipoplan { get; set; }
        public required int codtipoplan { get; set; }
        public string? grupo { get; set; }
        public string? codgrupo { get; set; }
        public string? tipodeduc { get; set; }
        public required decimal porc_deduc { get; set; }
        public required int plazo_minimo { get; set; }
        public required string plazo_tipo { get; set; }
        public required decimal monto_minimo { get; set; }
        public required decimal inversion_minimo { get; set; }
        public string? linea_codigo { get; set; }
        public string? linea_descripcion { get; set; }
        public required bool deducir_planilla { get; set; }
        public required bool genera_mora { get; set; }
        public required bool cdp { get; set; }
        public required bool controla_saldo { get; set; }
        public required bool cuenta_maestra { get; set; }
        public required int subcuentas_max { get; set; }
        public required decimal tasa_margen_negociacion { get; set; }
        public required bool requiere_beneficiarios { get; set; }
        public required bool deduce_independiente { get; set; }
        public required bool calcula_rend { get; set; }
        public required int base_calculo { get; set; }
        public required decimal tasa_base { get; set; }
        public required bool tasa_fluctuante { get; set; }
        public required bool capitaliza_rend { get; set; }
        public required bool utiliza_tbp { get; set; }
        public required bool sirve_garantia { get; set; }
        public required decimal garantia_porc_disp { get; set; }
        public required decimal garantia_tasa_ad { get; set; }
        public required bool garantia_integrada { get; set; }
        public required bool mov_cajas { get; set; }
        public string? mov_sinpe_tipos { get; set; }
        public required bool retiros_cajas { get; set; }
        public required bool giro_terceros { get; set; }
        public required bool website { get; set; }
        public required bool web_liquida { get; set; }
        public DateTime? web_vence { get; set; }
        public required bool renta_global { get; set; }
        public string? patrimonio_tipo { get; set; }
        public string? lineadesc { get; set; }
        public string? codigo_ase { get; set; }
        public required bool mov_sinpe { get; set; }
        public string resumen_cont_tasa { get; set; } = "";
        public required int num_contratos_activos { get; set; }
        public DateTime? ulttasa { get; set; }
        public required int contratos_activos_vb6 { get; set; }
        public DateTime? ultima_tasa_vb6 { get; set; }
        public int? contador_tasas_vb6 { get; set; }
        public decimal? ulttasa_vb6 { get; set; }
        public decimal? consecutivo { get; set; }
        public required bool tipo_cdp { get; set; }
        public required bool pago_cupones { get; set; }
        public required bool sinpe_cuenta { get; set; }
        public required bool apl_rend_automatico { get; set; }
        public required bool utiliza_tasa_fluctuante { get; set; }
        public required bool capitaliza_rendimientos { get; set; }
        public required bool tasa_ajuste_vencimiento { get; set; }
        public required decimal tasa_ajuste { get; set; }
        public required bool permite_mov_cajas { get; set; }
        public required bool forma_pago_pos { get; set; }
        public required bool permite_retiros_cajas { get; set; }
        public required bool permite_giro_terceros { get; set; }
        public required bool mov_entre_fondos { get; set; }
        public required bool mov_entre_fondos_terceros { get; set; }
        // ====== GENERAL ======
        public required bool apl_liq_socio { get; set; }
        public required bool liq_desde_ahorros { get; set; }
        public required bool permite_retiros_terceros { get; set; }
        public required bool visible_ec { get; set; }

        // ====== AUTO GESTIÓN ======
        //public bool vence_plan { get; set; }                  // Checkbox "Vence?"
        public required bool web_crear { get; set; }                   // Crear nuevos contratos
        public required bool web_modifica_couta { get; set; }          // Modifica cuota
        public required bool permite_ret_parcial { get; set; }         // Permite retiros parciales

        // ====== SINPE / PATRIMONIO ======
        public required bool patrimonio_enlace { get; set; }           // Enlazado a Patrimonio
        public required bool patrimonio_unifica { get; set; }          // Unificar en estado de cuenta patrimonio

        // ====== COMISIONES ======
        public required decimal tasa_comision_aportes { get; set; }
        public required decimal impuesto_rendimientos { get; set; }
        public required decimal tasa_comision_rend { get; set; }

        // ====== OTROS ======
        public required decimal comision_vta_inv { get; set; }
        public required decimal comision_vta_monto { get; set; }
        // ====== CAMPOS SINPE EXTRA ======
        public string sinpe_producto { get; set; } = "";
        // ====== CAMPOS DE RESUMEN VB6 ======
        public required bool sif_liquida { get; set; }
        public DateTime? fechaserver { get; set; }
        public required decimal impuesto_renta { get; set; }
        public required bool aplicar_tasa_cont_vencidos { get; set; }
        public required bool aplicar_en_procs_contrs_vencidos { get; set; }
        public required bool vence_renueva { get; set; }
        public required bool vence_notifica { get; set; }
        public string vence_accion { get; set; } = "";
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
        public int id { get; set; }
        public int desde { get; set; }
        public int hasta { get; set; }
        public decimal porcentaje { get; set; }
        public string aplicar { get; set; } = string.Empty;
        public int cod_operadora { get; set; }
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
        public int cod_operadora { get; set; }
        public string? cod_plan { get; set; }
        public string? vigente { get; set; }
        public string? tipo { get; set; }
        public string? fecha_referencia { get; set; }
        public string? justificacion { get; set; }
        public string? usuario { get; set; }
    }

    public class FndPlanPuntoDetalleDto
    {
        public int id { get; set; } // COD_TABLA_AUM
        public int cod_operadora { get; set; }
        public required string cod_plan { get; set; }
        public string tipo_tasa { get; set; } = "";
        public int desde { get; set; }
        public int hasta { get; set; }
        public decimal plus { get; set; }
        public int id_per_tasa { get; set; } // regla padre
        public string usuario { get; set; } = "";
    }

    public class FndPlanDestinoGuardarDto
    {
        public int id { get; set; }
        public string cod_plan { get; set; } = "";
        public string descripcion { get; set; } = "";
        public bool activo { get; set; }
        public string usuario { get; set; } = "";
    }

    public class FndPlanDestinoAsociadoDto
    {
        public required string id_destino { get; set; }
        public string cod_plan { get; set; } = "";
        public int asociado { get; set; }  // 1 = asociar, 0 = desasociar
        public string usuario { get; set; } = "";
        public int cod_operadora { get; set; }  // 1 = asociar, 0 = desasociar
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
        public int cod_operadora { get; set; }
        public string usuario { get; set; } = "";
        public List<EstadoAsignadoDto> estados { get; set; } = new();
        public List<PlazoAsignadoDto> plazos { get; set; } = new();
    }

    public class FndReglaActivarDto
    {
        public int id_regla { get; set; }
        public string usuario { get; set; } = "";
    }
}