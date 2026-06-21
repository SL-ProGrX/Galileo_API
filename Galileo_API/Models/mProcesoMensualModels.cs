namespace Galileo_API.Models
{
    public class MProcesoMensualModels
    {
        public class ProcesoMensualFndAsientoRequest
        {
            public decimal Proceso { get; set; }
            public int CodInstitucion { get; set; }
            public int Operadora { get; set; }
            public string Plan { get; set; } = string.Empty;
            public string Cuenta { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public string NumeroDocumento { get; set; } = string.Empty;
        }
        public class SbCrEnviaConPlanillaTransitoModel
        {
            public int Total { get; set; }
            public int Pendientes { get; set; }
            public int Procesados { get; set; }
        }
        public class CcProcesoMensualDevolucionFondoRequest
        {
            public int CodInstitucion { get; set; }
            public decimal FechaProceso { get; set; }
            public int Operadora { get; set; }
            public string Plan { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public string Documento { get; set; } = string.Empty;
            public string CuentaInconsistencia { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
        }

        
    }
}
