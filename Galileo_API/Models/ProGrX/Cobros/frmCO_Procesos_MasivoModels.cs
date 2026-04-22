namespace Galileo_API.Models.ProGrX.Cobros
{
    public class FrmCoProcesosMasivoModels
    {
      
        public class CoProcesosMasivoResumenModel
        {
            public int CantidadCasosValidos { get; set; }
            public decimal TotalMoraFinanciera { get; set; }
            public decimal TotalMoraLegal { get; set; } 
        }

        public class CoProcesosMasivoCargaRequest
        {
            public List<string> Operaciones { get; set; } = [];
            public string Usuario { get; set; } = string.Empty;
        }


        public class CoProcesosMasivoCargaResponse
        {
            public CoProcesosMasivoResumenModel Resumen { get; set; } = new();
            public List<CoProcesosMasivoRegistroModel> CasosValidos { get; set; } = [];
            public List<CoProcesosMasivoRegistroModel> CasosInconsistentes { get; set; } = [];
        }
      

        public class CoProcesosMasivoRegistroModel
        {
            public string Id_Solicitud { get; set; } = string.Empty;
            public string Codigo { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public decimal Saldo { get; set; } = 0;
            public decimal Mora_Financiera { get; set; } = 0;
            public decimal Mora_Legal { get; set; } = 0;
            public string Garantia_Desc { get; set; } = string.Empty;
            public string Linea_Desc { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public string Proceso { get; set; } = string.Empty;
            public string Antiguedad { get; set; } = string.Empty;
            public string Inconsistencia { get; set; } = string.Empty;
        }
    }
}
