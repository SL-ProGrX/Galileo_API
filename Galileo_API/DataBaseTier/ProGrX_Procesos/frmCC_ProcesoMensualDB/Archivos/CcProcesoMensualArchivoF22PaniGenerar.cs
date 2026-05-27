using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF22PaniGenerar : CcProcesoMensualArchivoPlanoGenerarBase<CcProcesoMensualArchivoF22PaniGenerar.CcProcesoMensualArchivoRegistroDbModel>

    {

        private const string MovimientoExclusion = "E";

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["22"];

        protected override string CodigoPlanillaEnvio => "22";
        protected override string CodigoFormato => "F22";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Monto_Actual AS MontoActual,
                P.Movimiento,
                P.Tipo,
                S.nombre AS Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento <> @MovimientoExclusion
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.cedula, P.tipo, P.movimiento";

        protected override object CrearParametrosRegistros(
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return new
            {
               request.FechaProceso,
                 request.CodInstitucion,
                MovimientoExclusion
            };
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoRegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var monto = Convert.ToInt64(registro.MontoActual * 100);

            return Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    registro.Cedula,
                    "I",
                    "0",
                    10)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    registro.Nombre,
                    "D",
                    " ",
                    30)
                + request.FechaProceso.ToString(CultureInfo.InvariantCulture)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    monto.ToString(CultureInfo.InvariantCulture),
                    "I",
                    "0",
                    8);
        }

        public sealed class CcProcesoMensualArchivoRegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

    }
}
