namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrConfiguracionGeneralDto
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string visible { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? inicio_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
        public string valor_mask { get; set; } = string.Empty;
        public string cuenta_descripcion { get; set; } = string.Empty;
    }

    public class CrConfiguracionGeneralGuardarDto
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }

    public class CrConfiguracionOperativosDto
    {
        public DateTime? cr_fecha_calculo { get; set; }
        public decimal cr_por_ahorro { get; set; }
        public decimal cr_tbp { get; set; }
        public string cr_cta_desembolso { get; set; } = string.Empty;
        public string cr_cta_desembolso_mask { get; set; } = string.Empty;
        public string cr_cta_desembolso_desc { get; set; } = string.Empty;
        public string cr_cta_polizas { get; set; } = string.Empty;
        public string cr_cta_polizas_mask { get; set; } = string.Empty;
        public string cr_cta_polizas_desc { get; set; } = string.Empty;
        public decimal cr_psdmnt { get; set; }
        public decimal regla_monto { get; set; }
        public int cod_banco { get; set; }
        public string cod_banco_desc { get; set; } = string.Empty;
        public string tipodoc { get; set; } = string.Empty;
        public int cod_banco_men { get; set; }
        public string cod_banco_men_desc { get; set; } = string.Empty;
        public string cod_tipo_men { get; set; } = string.Empty;
        public bool regla_banco { get; set; }
    }

    public class CrConfiguracionOperativosGuardarDto
    {
        public DateTime? cr_fecha_calculo { get; set; }
        public decimal? cr_por_ahorro { get; set; }
        public decimal? cr_tbp { get; set; }
        public string cr_cta_desembolso { get; set; } = string.Empty;
        public string cr_cta_polizas { get; set; } = string.Empty;
        public decimal? cr_psdmnt { get; set; }
        public decimal? regla_monto { get; set; }
        public int? cod_banco { get; set; }
        public string tipodoc { get; set; } = string.Empty;
        public int? cod_banco_men { get; set; }
        public string cod_tipo_men { get; set; } = string.Empty;
        public bool? regla_banco { get; set; }
    }
    public class CrConfiguracionFechaCorteGuardarDto
    {
        public DateTime? cr_fecha_calculo { get; set; }
    }

    public class CrConfiguracionTbpGuardarDto
    {
        public decimal? cr_tbp { get; set; }
    }
}