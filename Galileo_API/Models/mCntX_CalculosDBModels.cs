namespace Galileo_API.Models
{
    public sealed class CntXCalculosUtilidadDto
    {
        public decimal utilidad_mes { get; set; } = 0;
        public decimal utilidad_acumulada { get; set; } = 0;
    }

    public sealed class CntXCalculosRestructuraRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public int revision_total { get; set; } = 0;
    }

    public sealed class CntXCalculosAsientoProcesoRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
    }

    public sealed class CntXCalculosAsientoBorraRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
    }

    public sealed class CntXCalculosMovimientoCuentasRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public DateTime fecha_desde { get; set; }
        public DateTime fecha_hasta { get; set; }
        public string cuenta_inicio { get; set; } = string.Empty;
        public string cuenta_corte { get; set; } = string.Empty;
        public int mov_en_cero { get; set; } = 1;
        public string unidad { get; set; } = "0x0";
        public string centro_costo { get; set; } = "0x0";
        public int divisa_origen { get; set; } = 0;
        public int pendientes { get; set; } = 0;
    }

    public sealed class CntXCalculosPeriodoProcesoRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
    }
}
