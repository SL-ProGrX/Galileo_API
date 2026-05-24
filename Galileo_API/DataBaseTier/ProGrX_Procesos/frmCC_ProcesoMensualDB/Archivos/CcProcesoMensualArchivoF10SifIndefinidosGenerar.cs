using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF10SifIndefinidosGenerar : CcProcesoMensualArchivoPlanoGeneratorBase<CcProcesoMensualArchivoF10SifIndefinidosGenerar.CcProcesoMensualArchivoRegistroDbModel>

    {
        private const string MovimientoMantiene = "M";

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["10"];

        protected override string CodigoPlanillaEnvio => "10";
        protected override string CodigoFormato => "F10";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Monto_Actual AS MontoActual,
                P.Movimiento,
                S.nombre AS Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento <> @MovimientoMantiene
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.cedula, P.tipo, P.movimiento";
        protected override object CrearParametrosRegistros(
           CcProcesoMensualGeneraArchivoRequest request)
        {
            return new
            {
               request.FechaProceso,
              request.CodInstitucion,
                MovimientoMantiene
            };
        }

        protected override string CrearLineaArchivo(
                  CcProcesoMensualArchivoRegistroDbModel registro,
                  CcProcesoMensualGeneraArchivoRequest request)
        {
            return Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    registro.Cedula,
                    "D",
                    " ",
                    15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    registro.Nombre,
                    "D",
                    " ",
                    50)
                + FormatearMonto(registro.MontoActual)
                + " "
                + registro.Movimiento;
        }

        private static string FormatearMonto(decimal monto)
        {
            return monto.ToString(
                "000000000.00",
                CultureInfo.InvariantCulture);
        }

        public sealed class CcProcesoMensualArchivoRegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; }
            public string Movimiento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

    }
}
