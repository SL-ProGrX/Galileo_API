namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXPreliminarMontarRequest
    {
        public required int codContabilidad { get; set; }
        public required int anio { get; set; }
        public required int mes { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string unidad { get; set; } = "0x0";
    }
}
