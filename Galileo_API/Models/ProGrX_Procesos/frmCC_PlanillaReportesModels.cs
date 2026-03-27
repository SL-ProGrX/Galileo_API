namespace Galileo.Models.ProGrX_Procesos
{
    public class CcPlanillaReporteCatalogoDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
    public class CcPlanillaReporteTipoDto
    {
        public string codigo_opcion { get; set; } = string.Empty;
        public string codigo_reporte { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
    public class CcPlanillaReportesParametrosInicialesDto
    {
        public decimal proceso { get; set; } = 0m;
        public string proceso_format { get; set; } = string.Empty;
        public string frecuencia_pago { get; set; } = "M";
        public int? cod_institucion { get; set; }
        public string institucion_descripcion { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public List<CcPlanillaTipoCobroDto> tipos_cobro { get; set; } = new();
    }
    public class CcPlanillaProcesoScrollDto
    {
        public decimal proceso { get; set; } = 0m;
        public string proceso_format { get; set; } = string.Empty;
    }
    public class CcPlanillaInstitucionInfoDto
    {
        public int cod_institucion { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string frecuencia_id { get; set; } = "M";
        public decimal porc_aporte { get; set; } = 0m;
        public decimal porc_ahorro { get; set; } = 0m;
    }
    public class CcPlanillaTipoCobroDto
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}