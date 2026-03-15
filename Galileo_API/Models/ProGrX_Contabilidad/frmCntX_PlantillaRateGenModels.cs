namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXPlantillaRateDetalleData
    {
        public int num_linea { get; set; } = 0;
        public int cod_plantilla { get; set; } = 0;
        public int cod_contabilidad { get; set; } = 0;
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public decimal debitos { get; set; } = 0;
        public decimal creditos { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string unides { get; set; } = string.Empty;
    }

    public class CntXPlantillaRateGenerarRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public int cod_plantilla { get; set; } = 0;
        public decimal monto { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public int periodo_anio { get; set; } = 0;
        public int periodo_mes { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class CntXPlantillaRateData
    {
        public int cod_contabilidad { get; set; } = 0;
        public int cod_plantilla { get; set; } = 0;
        public int consecutivo { get; set; } = 0;
        public string tipo_asiento { get; set; } = string.Empty;
    }
}
