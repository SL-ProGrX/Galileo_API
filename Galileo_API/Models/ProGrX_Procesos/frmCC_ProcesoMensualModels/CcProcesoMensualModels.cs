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
            public string DirectorioResultados { get; set; } = string.Empty;
        }
        public class CcProcesoMensualArchivoConfiguracionModel
        {
            public string Planilla { get; set; } = string.Empty;
            public string CodigoAportesEnv { get; set; } = string.Empty;
            public string CodigoCreditosEnv { get; set; } = string.Empty;
            public decimal PorcAhorro { get; set; }
            public string CodigoInstDeduc { get; set; } = string.Empty;
            public int IncInclusiones { get; set; }
            public int IncExclusiones { get; set; }
            public int IncModificaciones { get; set; }
            public int IncMantienen { get; set; }
            public decimal PorcAporte { get; set; }
            public int ComparaIndicador { get; set; }
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
            public decimal MontoActual { get; set; }
            public string Movimiento { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
