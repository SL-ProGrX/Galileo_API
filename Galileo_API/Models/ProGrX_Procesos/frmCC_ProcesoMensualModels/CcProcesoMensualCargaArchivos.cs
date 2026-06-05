namespace Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels
{
    public class CcProcesoMensualCargaArchivos
    {
    
        public sealed class CcProcesoMensualCargaDeduccionesRequest
        {
            public int CodEmpresa { get; set; } = 0;
            public int CodInstitucion { get; set; } = 0;
            public decimal FechaProceso { get; set; } = 0;
            public int Pago { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
            public CcProcesoMensualCargaDeduccionesTipo TipoCarga { get; set; } = CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_Excel;

            public List<CcProcesoMensualCargaDeduccionFilaRequest> Filas { get; set; } = [];
        }
        public sealed class CcProcesoMensualCargaDeduccionFilaRequest
        {
            public string Cedula { get; set; } = string.Empty;
            public Dictionary<string, decimal> Montos { get; set; } = [];
            public string Codigo { get; set; } = string.Empty;
            public decimal Monto { get; set; } = 0;
            public int? Tipo { get; set; }
            public string Up { get; set; } = string.Empty;
            public string Ut { get; set; } = string.Empty;

        }
        public enum CcProcesoMensualCargaDeduccionesTipo
        {
            sbCargaDeduc_Excel = 1,
            sbCargaDeduc_Excel_Tek_Experts = 2,
            sbCargaDeduc_Excel_DxC_Costa_Rica = 3,
            sbCargaDeduc_Excel_DxC_CentroAmerica =4,
            sbCargaDeduc_ExcelNew=5,
            sbCargaDeduc_Csv_Integra=6,
            sbCargaDeduccionesArchivoPlano = 7

        }
        public sealed class CcProcesoMensualCargaDeduccionesResponse
        {
            public bool Cargado { get; set; }
            public int RegistrosProcesados { get; set; } = 0;
            public int RegistrosInsertados { get; set; } = 0;
            public string Mensaje { get; set; } = string.Empty;
        }
        public sealed class CcProcesoMensualReglaDeduccionConfig
        {
            public string CodDeduccion { get; init; } = string.Empty;
            public int Tipo { get; init; } = 0;
            public IReadOnlyCollection<string> ColumnasOrigen { get; init; } = [];

            public bool RequiereAportesHabilitados { get; init; }
            public bool RequiereCreditosHabilitados { get; init; }
            public bool RequiereColumnaExistente { get; init; }
            public bool InsertaSoloSiMontoMayorQueCero { get; init; } = true;

        }

        public sealed class CcProcesoMensualCargaConfigDbModel
        {
            public string Planilla { get; set; } = string.Empty;
            public string CodigoAportes { get; set; } = string.Empty;
            public string CodigoCreditos { get; set; } = string.Empty;
            public string CodigoObrero { get; set; } = string.Empty;
            public string CodigoPatronal { get; set; } = string.Empty;
        }
    }
}
