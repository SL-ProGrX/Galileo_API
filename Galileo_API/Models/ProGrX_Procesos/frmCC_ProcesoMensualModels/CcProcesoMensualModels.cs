namespace Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels
{
    public class CcProcesoMensualModels
    {

        public class CcProcesoMensualReporteModel
        {
            public string NombreReporte { get; set; } = string.Empty;
            public string Titulo { get; set; } = string.Empty;
            public Dictionary<string, object> Formulas { get; set; } = [];
            public string SelectionFormula { get; set; } = string.Empty;
        }
        public sealed class CcProcesoMensualGeneraDeduccionesResponse
        {
            public bool Generado { get; set; } = false;
            public string PlanillaEnvio { get; set; } = string.Empty;
            public CcProcesoMensualArchivoGeneradoModel Archivo { get; set; } = new();
        }
        public class CcProcesoMensualGeneraDeduccionesRequest
        {
            public int CodInstitucion { get; set; } = 0;
            public decimal FechaProceso { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
            public bool UsaPlanillaTransito { get; set; } = false;
            public bool AplicaCambioDeducciones { get; set; } = false;
            public int Redondeo { get; set; } = 0;
        }

        public class CcProcesoMensualArchivoGeneradoModel
        {
            public bool Generado { get; set; } = false;
            public string CodigoPlanillaEnvio { get; set; } = string.Empty;
            public string NombreArchivo { get; set; } = string.Empty;
            public string RutaArchivo { get; set; } = string.Empty;
            public string ContentType { get; set; } = "text/csv";
            public byte[] ArchivoBytes { get; set; } = [];
            public List<string> ArchivosGenerados { get; set; } = [];
        }

        public class CcProcesoMensualGeneraArchivoRequest
        {
            public int CodInstitucion { get; set; } = 0;
            public decimal FechaProceso { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
            public string NombreInstitucion { get; set; } = string.Empty;
            public string NombreEmpresa { get; set; } = string.Empty;
            public int EmpresaId { get; set; } = 0;
            public string Unidad { get; set; } = string.Empty;
        }
        public class CcProcesoMensualArchivoConfiguracionModel
        {
            public string Planilla { get; set; } = string.Empty;
            public string CodigoAportesEnv { get; set; } = string.Empty;
            public string CodigoCreditosEnv { get; set; } = string.Empty;
            public decimal PorcAhorro { get; set; } = 0;
            public string CodigoInstDeduc { get; set; } = string.Empty;
            public int IncInclusiones { get; set; } = 0;
            public int IncExclusiones { get; set; } = 0;
            public int IncModificaciones { get; set; } = 0;
            public int IncMantienen { get; set; } = 0;
            public decimal PorcAporte { get; set; } = 0;
            public int ComparaIndicador { get; set; } = 0;
        }
        public sealed class CcProcesoMensualArchivoRegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Direccion { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
        }

        public sealed class CcProcesoMensualArchivoNombreModel
        {
            public string Apellido1 { get; set; } = string.Empty;
            public string Apellido2 { get; set; } = string.Empty;
            public string Nombre1 { get; set; } = string.Empty;
            public string Nombre2 { get; set; } = string.Empty;
        }
        public sealed class CcProcesoMensualArchivoPlanillaBasicaDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

        public sealed class CcProcesoMensualBitacoraPlanillaDto
        {
            public string Transaccion { get; set; } = string.Empty;
            public int CodInstitucion { get; set; } = 0;
            public decimal Proceso { get; set; } = 0;
            public string Gestion { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public string Documento { get; set; } = string.Empty;
        }
        public sealed class CcProcesoMensualCreditoDesgloseConfigDbModel
        {
            public int Aplica { get; set; } = 0;
            public int HistoricoCobroEnvio { get; set; } = 0;
            public DateTime FechaServer { get; set; }
        }
        public sealed class CcProcesoMensualCreditoDesgloseResultadoDbModel
        {
            public int Total { get; set; } = 0;
            public int Pendientes { get; set; } = 0;
            public int Procesados { get; set; } = 0;
        }
        public sealed class CcProcesoMensualCreditoDesgloseDbRequest
        {
            public int CodInstitucion { get; set; }
            public decimal FechaProceso { get; set; }
            public DateTime FechaSistema { get; set; }
            public int AplicaIncon { get; set; }
            public int PrimeraVez { get; set; }
            public int Cantidad { get; set; }
        }

        public sealed class CcProcesoMensualDesgloseRequest
        {
            public int CodInstitucion { get; set; } = 0;
            public decimal FechaProceso { get; set; } = 0;
            public int CodEmpresa { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
        }
        public sealed class CcProcesoMensualDesglosePlanillaResponse
        {
            public bool Desglosado { get; set; } = false;
            public string Mensaje { get; set; } = string.Empty;
        }


        #region Ahorros

        public sealed class CcProcesoMensualAhorros
        {
            public bool Aplicado { get; set; } = false; 
            public CcProcesoMensualAhorroReporteModel ParametrosReporte { get; init; } = new();
        }

        public sealed class CcProcesoMensualAhorroReporteModel
        {
            public decimal Porcentaje { get; set; } = 0;
            public decimal PorcAhorro { get; set; } = 0;
        }

        #endregion

    }
}
