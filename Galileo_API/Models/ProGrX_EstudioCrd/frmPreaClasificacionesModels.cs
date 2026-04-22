namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class PreaClasificacionRazonData
    {
        public string cod_razon { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string color { get; set; } = string.Empty;
    }

    public class PreaClasificacionData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string razon { get; set; } = string.Empty;
        public string razon_desc { get; set; } = string.Empty;
        public decimal desde { get; set; } = 0;
        public decimal hasta { get; set; } = 0;
        public string? tipo { get; set; } = string.Empty;
    }
}
