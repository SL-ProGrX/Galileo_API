namespace PgxAPI.Models.AF
{
    public class VBeneficiosIntegralDtoLista
    {
        public int Total { get; set; }
        public List<VBeneficiosIntegralDto> Beneficios { get; set; } = new List<VBeneficiosIntegralDto>();
    }

    public class VBeneficiosIntegralDto
    {
        public string? btn { get; set; }
        public string? cod_beneficio { get; set; }
        public int consec { get; set; } = 0;
        public string? cedula { get; set; }
        public string? nombre_beneficiario { get; set; }
        public float monto { get; set; } = 0;
        public string? estado_desc { get; set; }
        public string? beneficio_desc { get; set; }
        public string? solicita { get; set; }
        public string? solicita_nombre { get; set; }
        public DateTime registra_fecha { get; set; }
        public string? registra_user { get; set; }
        public Nullable<DateTime> autoriza_fecha { get; set; }
        public string? autoriza_user { get; set; }
        public string? empresa_desc { get; set; }
        public string? departamento_desc { get; set; }
        public string? oficina_desc { get; set; }
    }

    public class BeneficiosMonitorFiltros
    {
        public string? beneficio_id { get; set; }
        public string? beneficiario_nombre { get; set; }
        public string? solicita_id { get; set; }
        public string? solicita_nombre { get; set; }
        public string? estado_persona { get; set; }
        public string? institucion { get; set; }
        public string? usuario_registra { get; set; }
        public string? usuario_autoriza { get; set; }
        public string? unidad { get; set; }
        public string? oficina { get; set; }

        public string? estado { get; set; }
        public string? fecha { get; set; }
        public string? fecha_inicio { get; set; }
        public string? fecha_corte { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string vfiltro { get; set; } = string.Empty;
    }

    public class OpcionesLista
    {
        public string? item { get; set; }
        public string? descripcion { get; set; }
    }
}