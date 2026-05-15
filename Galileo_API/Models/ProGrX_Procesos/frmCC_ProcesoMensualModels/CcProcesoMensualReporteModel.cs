namespace Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels
{
    public class CcProcesoMensualReporteModel
    {
        public string NombreReporte { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public Dictionary<string, object> Formulas { get; set; } = new();
        public string SelectionFormula { get; set; } = string.Empty;
    }

}
