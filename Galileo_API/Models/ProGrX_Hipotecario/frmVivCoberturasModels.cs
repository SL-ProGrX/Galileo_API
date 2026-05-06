namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class FrmVivCoberturasResumenRequest
    {
        public long numero_operacion { get; set; } = 0;
        public string modo_cobertura { get; set; } = string.Empty;
        public string numero_finca { get; set; } = string.Empty;
    }

    public class FrmVivCoberturasCargaResponse
    {
        public FrmVivCoberturasOperacionResponse operacion { get; set; } = new();
        public List<FrmVivCoberturasFincaItem> fincas { get; set; } = new();
        public FrmVivCoberturasResumenResponse resumen { get; set; } = new();
    }

    public class FrmVivCoberturasOperacionResponse
    {
        public long id_solicitud { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class FrmVivCoberturasFincaItem
    {
        public string numero_finca { get; set; } = string.Empty;
        public decimal avaluo { get; set; } = 0;
    }

    public class FrmVivCoberturasResumenResponse
    {
        public decimal avaluo { get; set; } = 0;
        public decimal disponible { get; set; } = 0;
        public decimal hip_externa { get; set; } = 0;
        public decimal hip_interna { get; set; } = 0;
        public decimal hip_libera { get; set; } = 0;
        public decimal cobertura { get; set; } = 0;
    }

    public class FrmVivCoberturasResumenRawResponse
    {
        public decimal Avaluo { get; set; } = 0;
        public decimal disponible { get; set; } = 0;
        public decimal HipExterna { get; set; } = 0;
        public decimal HipInterna { get; set; } = 0;
        public decimal HipLibera { get; set; } = 0;
        public decimal Cobertura { get; set; } = 0;
    }
}
