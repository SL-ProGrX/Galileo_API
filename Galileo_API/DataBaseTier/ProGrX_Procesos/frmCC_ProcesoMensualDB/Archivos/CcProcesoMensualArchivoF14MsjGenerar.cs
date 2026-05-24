using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF14MsjGenerar : CcProcesoMensualArchivoPlanoGeneratorBase<CcProcesoMensualArchivoF14MsjGenerar.CcProcesoMensualArchivoRegistroDbModel>

    {
        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["14"];

        protected override string CodigoPlanillaEnvio => "14";
        protected override string CodigoFormato => "F14";
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
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.cedula, P.tipo, P.movimiento";

        protected override string CrearLineaArchivo(
              CcProcesoMensualArchivoRegistroDbModel registro,
              CcProcesoMensualGeneraArchivoRequest request)
        {
            return Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    registro.Cedula,
                    "I",
                    "0",
                    9)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    registro.Nombre,
                    "D",
                    " ",
                    30)
                + FormatearMontoF14(registro.MontoActual);
        }

        private static string FormatearMontoF14(decimal monto)
        {
            var montoTexto = monto.ToString("00000000.00", CultureInfo.InvariantCulture);

            var sinPunto = string.Concat(
                montoTexto.AsSpan()[..8],
                montoTexto.AsSpan(9, 2));

            var montoEntero = long.TryParse(
                sinPunto,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var numero)
                    ? numero
                    : 0;

            return montoEntero.ToString("000000000", CultureInfo.InvariantCulture);
        }

        public sealed class CcProcesoMensualArchivoRegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; }
            public string Movimiento { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

    }
}
