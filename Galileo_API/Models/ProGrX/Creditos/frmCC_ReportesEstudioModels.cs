namespace Galileo_API.Models.ProGrX.Creditos
{
    public sealed class CcReportesEstudioCatalogoDto
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class CcReportesEstudioCatalogosResponseDto
    {
        public List<CcReportesEstudioCatalogoDto> instituciones { get; set; } = [];
        public List<CcReportesEstudioCatalogoDto> estados { get; set; } = [];
        public List<CcReportesEstudioCatalogoDto> carteras { get; set; } = [];
    }

    public sealed class CcReportesEstudioLineasRequestDto
    {
        public bool retencion { get; set; }
        public bool lineas_internas { get; set; }
        public bool solo_con_saldo { get; set; }
        public string? cod_cartera { get; set; }
    }

    public sealed class CcReportesEstudioGenerarRequestDto
    {
        public string codigo_reporte { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public int rango_proyeccion { get; set; } = 12;
        public int? cod_institucion { get; set; }
        public string? cod_estado { get; set; }
        public int frecuencia { get; set; } = 5;
        public string? lineas { get; set; }
    }
}
