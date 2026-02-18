namespace Galileo_API.Models.ProGrX_Contabilidad
{

    public class CntxPlantillaRateDetalleDto
    {
        public int? NumLinea { get; set; }
        public string? CodCuenta { get; set; } = string.Empty;
        public string? CodUnidad { get; set; } = string.Empty;
        public string? CodCentroCosto { get; set; } = string.Empty;
        public string? CodDivisa { get; set; } = string.Empty;
        public string? Detalle { get; set; } = string.Empty;
        public decimal? Debitos { get; set; }
        public decimal? Creditos { get; set; }
    }

    public class CntxPlantillaRateDto
    {
        public string? CodPlantilla { get; set; }
        public string? Descripcion { get; set; } = string.Empty;
        public string? TipoAsiento { get; set; } = string.Empty;
        public int? Consecutivo { get; set; }
        public List<CntxPlantillaRateDetalleDto> Detalle { get; set; } = new();

        public string? RegistroUsuario { get; set; }
    }

}
