namespace Galileo_API.Models.ProGrX.Creditos
{
    public class FrmCRCambioInfoEstadisticaModels
    { 
        public class CrCambioInfoEstadisticaProcesarRequest
        {
            public string ProcessId { get; set; } = string.Empty;

            public string Usuario { get; set; } = string.Empty;

            public string CodigoDato { get; set; } = string.Empty;

            public string TipoDescripcion { get; set; } = string.Empty;

            public int CantidadLineas { get; set; } = 0;
        }

        public class CrCambioInfoEstadisticaCargaListadoRequest
        {
            public string TipoSeleccionado { get; set; } = string.Empty;

            public string CodigoDato { get; set; } = string.Empty;

            public string Usuario { get; set; } = string.Empty;

            public List<CrCambioInfoEstadisticaCargaExcelData> Registros { get; set; } = [];
        }

        public class CrCambioInfoEstadisticaCargaExcelData
        {
            public string Operacion { get; set; } = string.Empty;
        }

        public class CrCambioInfoEstadisticaCargaListadoResponse
        {
            public string ProcessId { get; set; } = string.Empty;

            public int CantidadRegistros { get; set; } = 0;

            public List<CrCambioInfoEstadisticaCargaResultadoData> Registros { get; set; } = [];
        }

        public class CrCambioInfoEstadisticaCargaResultadoData
        {
            public int Id_Solicitud { get; set; } = 0;

            public string Codigo { get; set; } = string.Empty;

            public string Cedula { get; set; } = string.Empty;

            public string Nombre { get; set; } = string.Empty;
        }

    }
}
