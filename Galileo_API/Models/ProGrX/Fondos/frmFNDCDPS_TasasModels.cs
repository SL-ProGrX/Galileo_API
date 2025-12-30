namespace Galileo.Models.ProGrX.Fondos
{
    public class FndCdpsTasaRefData
    {
        public string cod_tasa_ref { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public bool? activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class SpCdpTasaConfigResultDto
    {
        public int pass { get; set; }
        public string? movimiento { get; set; }
        public string? mensaje { get; set; }
    }

    public class FndCdpsTasaPlanesDto
    {
        public string cod_operadora { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public bool asignado => registro_fecha.HasValue;
    }

    public class FndCdpTasasVencimientoDto
    {
        public int id_tasa { get; set; }
        public int id_frecuenciacupon { get; set; }
        public int id_plazocupon { get; set; }
        public string plazo { get; set; } = string.Empty;
        public string cupon { get; set; } = string.Empty;
        public decimal tasa { get; set; }
    }

    public class FndCdpsTasaBitacoraDto
    {
        public int id_tasa_cambio { get; set; }
        public string cod_tasa_ref { get; set; } = string.Empty;
        public string? modelo_desc { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? movimiento { get; set; }
        public decimal? v_anterior { get; set; }
        public decimal? v_actual { get; set; }
        public string? notas { get; set; }
    }
}
