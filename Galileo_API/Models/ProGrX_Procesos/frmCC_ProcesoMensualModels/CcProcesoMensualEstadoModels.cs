using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels
{
    public class CcProcesoMensualEstadoModels
    {
        public class CcProcesoMensualInicialResponse
        {
            public List<DropDownListaGenericaModel> Meses { get; set; } = [];
            public List<DropDownListaGenericaModel> Aplicaciones { get; set; } = [];
            public DateTime FechaServidor { get; set; }
            public bool MostrarAplicacion { get; set; }
            public bool HabilitarAhorros { get; set; }
            public CcProcesoMensualEstadoResponse EstadoActual { get; set; } = new();
        }

        public class CcProcesoMensualEstadoResponse
        {
            public string FrecuenciaId { get; set; } = string.Empty;
            public bool ExisteParametroProceso { get; set; }
            public string Mensaje { get; set; } = string.Empty;
            public CcProcesoMensualIndicadoresModel Indicadores { get; set; } = new();
            public string? PasoEjecutado { get; set; }
        }

        public class CcProcesoMensualFrecuenciaSeleccionModel
        {
            public string FrecuenciaSeleccionada { get; set; } = string.Empty;
            public string SufijoFechaProceso { get; set; } = string.Empty;
        }

        public class CcProcesoMensualIndicadoresModel
        {
            public bool Genera { get; set; }
            public bool Fecha { get; set; }
            public bool Carga { get; set; }
            public bool Desgloce { get; set; }

            public bool AhorrosAplica { get; set; }
            public bool AhorrosInconsistencias { get; set; }
            public bool AhorrosDevolucion { get; set; }

            public bool CreditosAplica { get; set; }
            public bool CreditosInconsistencias { get; set; }
            public bool CreditosRecalculo { get; set; }

            public int? OpcionGeneralSeleccionada { get; set; }
            public int? OpcionAhorrosSeleccionada { get; set; }
            public int? OpcionCreditosSeleccionada { get; set; }
        }

        public class CcProcesoMensualReportesModel
        {
            public bool AhorrosAplica { get; set; }
            public bool AhorrosDevolucion { get; set; }
            public bool AhorrosInconsistencias { get; set; }

            public bool Genera { get; set; }
            public bool Carga { get; set; }
            public bool Desgloce { get; set; }

            public bool CreditosAplica { get; set; }
            public bool CreditosInconsistencias { get; set; }
        }
        public class CcProcesoMensualInstitucionParametrosModel
        {
            public string Frecuencia_Id { get; set; } = "M";

            public int Pr_Genera { get; set; }
            public int Pr_Carga { get; set; }
            public int Pr_Desgloza { get; set; }

            public int Pr_ApAplica { get; set; }
            public int Pr_ApInco { get; set; }
            public int Pr_ApDev { get; set; }

            public int Pr_CrAplica { get; set; }
            public int Pr_CrInco { get; set; }
            public int Pr_CrMora { get; set; }
        }

        public class CcProcesoMensualBitacoraDbModel
        {
            public int Id_Seq { get; set; }
            public string Gestion { get; set; } = string.Empty;
            public string Transaccion { get; set; } = string.Empty;
            public string Documento { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public DateTime? Fecha { get; set; }
        }

        public class CcProcesoMensualValidaPasoResponse
        {
            public bool Valido { get; set; } = false;
            public string Mensaje { get; set; } = string.Empty;
        }
        public class CcProcesoMensualValidaPasoRequest
        {
            public int CodInstitucion { get; init; } = 0;
            public decimal FechaProceso { get; init; } = 0;
            public string Transaccion { get; init; } = "08";
        }

        public sealed class CcProcesoMensualCambiarFechaRequest
        {
            public int CodInstitucion { get; set; } = 0;

            public string Anio { get; set; } = string.Empty;

            public int Mes { get; set; } = 0;

            public int Quincena { get; set; } = 0; 

            public string Usuario { get; set; } = string.Empty;
        }
        public sealed class CcProcesoMensualCambiarFechaResponse
        {
            public decimal FechaProceso { get; set; } = 0;
            public DateTime FechaCorte { get; set; }
            public string Mensaje { get; set; } = string.Empty;
        }
    }
}
