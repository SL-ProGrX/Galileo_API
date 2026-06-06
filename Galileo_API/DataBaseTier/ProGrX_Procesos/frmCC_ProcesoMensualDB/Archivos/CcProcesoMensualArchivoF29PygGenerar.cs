using Dapper;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Helpers;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF29PygGenerar : CcProcesoMensualArchivoConMovimientosGeneratorBase<CcProcesoMensualArchivoF29PygGenerar.CcProcesoMensualArchivoF29RegistroDbModel>

    {
        private const string TipoDeduccionMonto = "M";  

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["29"];

        protected override string CodigoPlanillaEnvio => "29";
        protected override string CodigoFormato => "F29";
        protected override string ExtensionArchivo => ".csv";
        protected override string ContentType => ContentTypeCsv;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Cod_Deduccion AS CodDeduccion,
                P.Monto_Actual AS MontoActual,
                P.Tipo_Deduc AS TipoDeduc,
                P.Movimiento,
                S.CedulaR AS CedulaColilla,
                dbo.fxSIFCorteAFecha(P.Proceso) AS Corte,
                S.Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.tipo, P.movimiento, P.cedula";


      

        protected override IEnumerable<CcProcesoMensualArchivoF29RegistroDbModel> FiltrarRegistros(
            IEnumerable<CcProcesoMensualArchivoF29RegistroDbModel> registros)
        {
            return registros.Where(registro =>
                string.Equals(
                    registro.TipoDeduc?.Trim(),
                    TipoDeduccionMonto,
                    StringComparison.OrdinalIgnoreCase));
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF29RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return "F2;'01;'"
                + CodigoInstitucionArchivo
                + ";"
                + (registro.CedulaColilla ?? string.Empty).Trim()
                + ";;"
                + registro.Corte.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                + ";"
                + registro.Corte.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                + ";"
                + registro.CodDeduccion.Trim()
                + ";"
                + registro.MontoActual.ToString(CultureInfo.InvariantCulture)
                + ";CRC;;"
                + registro.Cedula
                + ";"
                + registro.Nombre;
        }


        public sealed class CcProcesoMensualArchivoF29RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string CedulaColilla { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string TipoDeduc { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public DateTime Corte { get; set; } = DateTime.MinValue;
            public string Nombre { get; set; } = string.Empty;
        }

    }
}
