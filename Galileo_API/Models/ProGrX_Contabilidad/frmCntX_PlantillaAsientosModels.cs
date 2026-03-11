namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntxPlantillaDto
    {
        public int? cod_plantilla { get; set; }

        public int? cod_contabilidad { get; set; }

        public string? descripcion { get; set; } = string.Empty;

        public string? tipo_asiento { get; set; } = string.Empty;

        public int? anio_inicio { get; set; }

        public int? mes_inicio { get; set; }

        public string? asiento_descripcion { get; set; } = string.Empty;

        public string? asiento_detalle { get; set; } = string.Empty;

        public string? asiento_documento { get; set; } = string.Empty;

        public int? consecutivo { get; set; }

        public string? tipo_asiento_desc { get; set; } = string.Empty;

        public string? mes_desc { get; set; } = string.Empty;
    }


    public class CntxPlantillaDetalleDto
    {
        public int? cod_plantilla { get; set; }

        public int? cod_contabilidad { get; set; }

        public int? num_linea { get; set; }

        public string? cod_cuenta { get; set; } = string.Empty;

        public string? cod_unidad { get; set; } = string.Empty;

        public string? cod_centro_costo { get; set; } = string.Empty;

        public string? cod_divisa { get; set; } = string.Empty;

        public decimal? tc { get; set; }

        public string? inc_tipo { get; set; } = string.Empty;

        public decimal? inc_valor { get; set; }

        public decimal? debitos { get; set; }

        public decimal? creditos { get; set; }

        // Campos descriptivos

        public string? cuenta_desc { get; set; } = string.Empty;

        public string? unidad_desc { get; set; } = string.Empty;

        public string? centro_costo_desc { get; set; } = string.Empty;

        public string? divisa_desc { get; set; } = string.Empty;
    }

    public class CntxPlantillaSaveDto
    {
        public CntxPlantillaDto header { get; set; } = new();

        public List<CntxPlantillaDetalleDto> detalle { get; set; } = new();
    }

    public class CntxPlantillaResponseDto
    {
        public CntxPlantillaDto? header { get; set; }

        public List<CntxPlantillaDetalleDto> detalle { get; set; } = new();
    }
}
