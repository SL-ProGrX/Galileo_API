namespace Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels
{
    public class CcProcesoMensualModels
    {

        public class CcProcesoMensualReporteModel
        {
            public string NombreReporte { get; set; } = string.Empty;
            public string Titulo { get; set; } = string.Empty;
            public Dictionary<string, object> Formulas { get; set; } = new();
            public string SelectionFormula { get; set; } = string.Empty;
        }

        public class CcProcesoMensualGeneraDeduccionesRequest
        {
            public int CodInstitucion { get; set; }
            public decimal FechaProceso { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public bool UsaPlanillaTransito { get; set; }
            public bool AplicaCambioDeducciones { get; set; }
            public int Redondeo { get; set; }
        }


    }
}
