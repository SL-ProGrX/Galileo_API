using System.Text.Json.Serialization;
namespace Galileo_API.Models.ProGrX.Creditos
{
    public class FrmCrAbonoMasivoManualModels
    {
        public  class CrAplicacionAbonoMasivoRequest
        {
            public string Usuario { get; set; } = string.Empty;
            public IReadOnlyCollection<CrAplicacionAbonoMasivoRegistroRequest> Registros { get; set; } = [];
        }

        public class CrAplicacionAbonoMasivoRegistroRequest
        {
            public long Operacion { get; set; } = 0;
            public decimal Abono { get; set; } = 0;
        }

        public class CrAplicacionAbonoMasivoResponse
        {
            public int CantidadCasos { get; set; } = 0;
            public decimal MontoTotal { get; set; } = 0;

            public IReadOnlyCollection<CrAplicacionAbonoMasivoDetalle> Registros { get; set; } = [];
        }

        public class CrAplicacionAbonoMasivoDetalle
        {
            public long Id_Solicitud { get; set; } = 0;
            public string Codigo { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public decimal Abono { get; set; } = 0;
        }
        public  class CrAplicacionAbonoMasivoProcesarRequest
        {
            public string Usuario { get; set; } = string.Empty;
            [JsonRequired]
            public long Operadora { get; set; }
            public string Plan { get; set; } = string.Empty;
            public string Cuenta { get; set; } = string.Empty;
            [JsonRequired]
            public bool FondoGeneral { get; set; }
            public string Tipo { get; set; } = string.Empty;
        }
        public  class CrAplicacionAbonoMasivoProcesarResponse
        {
            public string TipoDocumento { get; set; } = string.Empty;
            public string NumeroDocumento { get; set; } = string.Empty;
        }
 
    }
}
