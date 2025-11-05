namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AfInstitucionEmpresasDto
    {
        public int cod_institucion { get; set; }
        public string desc_corta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class AfInstitucionDepartamentosDto
    {
        public int cod_institucion { get; set; }
        public string cod_departamento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class AfInstitucionSeccionesDto
    {
        public int cod_institucion { get; set; }
        public string cod_departamento { get; set; } = string.Empty;
        public string cod_seccion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class AfInstitucionesCodigosDto
    {
        public int cod_institucion { get; set; }
        public string cod_deduccion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class AfInstitucionesCodigosLineasDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class AfInstitucionDto
    {
        public int cod_institucion { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string? cod_cuenta { get; set; }
        public string? direccion { get; set; }
        public string? planilla { get; set; }
        public decimal? porc_ahorro { get; set; }
        public decimal? porc_aporte { get; set; }
        public string? tipoasiento { get; set; }
        public string? cta_credito { get; set; }
        public string? cta_obrero { get; set; }
        public string? cta_patronal { get; set; }
        public string? codigo_aportes { get; set; }
        public string? codigo_creditos { get; set; }
        public string? cta_inconsistencia { get; set; }
        public DateTime? pr_fecha_corte { get; set; }
        public bool pr_genera { get; set; }
        public bool pr_carga { get; set; }
        public bool pr_desgloza { get; set; }
        public bool pr_apaplica { get; set; }
        public bool pr_apinco { get; set; }
        public bool pr_apdev { get; set; }
        public bool pr_craplica { get; set; }
        public bool pr_crinco { get; set; }
        public bool pr_crmora { get; set; }
        public bool pr_cr_aplica_incon { get; set; }
        public bool fnd_ap_aplica { get; set; }
        public int fnd_ap_operadora { get; set; }
        public string? fnd_ap_plan { get; set; }
        public bool fnd_cr_soaplica { get; set; }
        public int fnd_cr_sooperadora { get; set; }
        public string? fnd_cr_soplan { get; set; }
        public bool fnd_cr_exaplica { get; set; }
        public int fnd_cr_exoperadora { get; set; }
        public string? fnd_cr_explan { get; set; }
        public string? fnd_ap_planp { get; set; }
        public int ind_cambia_fecpro { get; set; }
        public string? codigo_aportes_env { get; set; }
        public string? codigo_creditos_env { get; set; }
        public bool compara_indicador { get; set; }
        public string? compara_valor { get; set; }
        public string? planilla_envio { get; set; }
        public bool activa { get; set; }
        public string? codigo_inst_deduc { get; set; }
        public int historico_cobro_envio { get; set; }
        public int tipo_cobro_mora { get; set; }
        public bool incinclusiones { get; set; }
        public bool incexclusiones { get; set; }
        public bool incmodificaciones { get; set; }
        public bool incmantienen { get; set; }
        public int transito_planillas_mes { get; set; }
        public string? transito_compara { get; set; }
        public bool mora_cierres { get; set; }
        public string? cta_fondos { get; set; }
        public string? codigo_creditos_alt { get; set; }
        public string? codigo_creditos_alt_env { get; set; }
        public string? codigo_fondos { get; set; }
        public string? codigo_fondos_env { get; set; }
        public string desc_corta { get; set; } = string.Empty;
        public bool deduccion_planilla { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
        public string? frecuencia { get; set; }
        public string? ind_aplica_pagos { get; set; }
        public string cta_crd_mask { get; set; } = string.Empty;
        public string? cta_crd_desc { get; set; }
        public string cta_obr_mask { get; set; } = string.Empty;
        public string? cta_obr_desc { get; set; }
        public string cta_pat_mask { get; set; } = string.Empty;
        public string? cta_pat_desc { get; set; }
        public string cta_fnd_mask { get; set; } = string.Empty;
        public string? cta_fnd_desc { get; set; }
        public string cta_inc_mask { get; set; } = string.Empty;
        public string? cta_inc_desc { get; set; }
        public string? divisa_desc { get; set; }
        public string? op_cr_soc_desc { get; set; }
        public string? op_cr_eso_desc { get; set; }
        public string? op_ap_desc { get; set; }
        public string? plan_ap_obr_desc { get; set; }
        public string? plan_ap_pat_desc { get; set; }
        public string? plan_cr_soc { get; set; }
        public string? plan_cr_eso { get; set; }
        public string? frecuencia_id { get; set; }
        public string? frecuencia_desc { get; set; }
    }
}
