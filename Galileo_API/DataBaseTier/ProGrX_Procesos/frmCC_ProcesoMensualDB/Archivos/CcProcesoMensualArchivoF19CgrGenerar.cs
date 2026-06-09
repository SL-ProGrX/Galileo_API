using Dapper;
using System.Data;
using System.Globalization; 
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF19CgrGenerar : CcProcesoMensualArchivoConMovimientosGeneratorBase<CcProcesoMensualArchivoF19CgrGenerar.CcProcesoMensualArchivoF19RegistroDbModel>

    {
        public CcProcesoMensualArchivoF19CgrGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        {
        }
        private const string TipoAhorro = "A";
        private const string TipoExtraordinario = "E";
        private const string TipoCredito = "C"; 
        private decimal _porcAhorro;

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["19"];

        protected override string CodigoPlanillaEnvio => "19";
        protected override string CodigoFormato => "F19";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        protected override string QueryRegistros => @"
            SELECT
                S.Nombre,
                P.cedula AS Cedula,
                P.Tipo,
                P.cod_deduccion AS CodDeduccion,
                P.Movimiento,
                P.Monto_Actual AS MontoActual,
                ISNULL(S.cod_sector, 0) AS Sector
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.cedula, P.tipo, P.cod_deduccion, P.movimiento";

        protected override void PrepararConfiguracion(
          IDbConnection connection,
          CcProcesoMensualArchivoConfiguracionModel configuracion,
          CcProcesoMensualGeneraArchivoRequest request)
        {
            _porcAhorro = configuracion.PorcAhorro;
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF19RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var monto = ObtenerMontoArchivo(registro, _porcAhorro);

            return "2"
                + registro.CodDeduccion.Trim()
                + " "
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    registro.Nombre,
                    "D",
                    " ",
                    28)
                + " "
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    registro.Cedula,
                    "I",
                    "0",
                    10)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    string.Empty,
                    "I",
                    "0",
                    9)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    monto,
                    "I",
                    "0",
                    9);
        }

        private static string ObtenerMontoArchivo(
            CcProcesoMensualArchivoF19RegistroDbModel registro,
            decimal porcAhorro)
        {
            var tipo = registro.Tipo?.Trim().ToUpperInvariant();

            var montoTexto = tipo switch
            {
                TipoAhorro => porcAhorro.ToString("######0.00", CultureInfo.InvariantCulture),
                TipoExtraordinario => registro.MontoActual.ToString("############0.00", CultureInfo.InvariantCulture),
                TipoCredito => registro.MontoActual.ToString("############0.00", CultureInfo.InvariantCulture),
                _ => string.Empty
            };

            return montoTexto.Replace(".", string.Empty);
        }

        public sealed class CcProcesoMensualArchivoF19RegistroDbModel
        {
            public string Nombre { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public decimal MontoActual { get; set; }
            public int Sector { get; set; }
        }
    }
}
