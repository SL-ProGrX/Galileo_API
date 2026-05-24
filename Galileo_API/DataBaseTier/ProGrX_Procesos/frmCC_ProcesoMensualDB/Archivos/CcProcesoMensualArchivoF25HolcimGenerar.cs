using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF25HolcimGenerar : CcProcesoMensualArchivoConMovimientosGeneratorBase<CcProcesoMensualArchivoF25HolcimGenerar.CcProcesoMensualArchivoF25RegistroDbModel>
    {
        private const string TipoDeduccionMonto = "M"; 
        private string _codigoInstitucionArchivo = string.Empty;

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["25"];

        protected override string CodigoPlanillaEnvio => "25";
        protected override string CodigoFormato => "F25";
        protected override string ExtensionArchivo => ".csv";
        protected override string ContentType => ContentTypeCsv;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Cod_Deduccion AS CodDeduccion,
                P.Monto_Actual AS MontoActual,
                P.Porc_Deduc AS PorcDeduc,
                P.Tipo_Deduc AS TipoDeduc,
                P.Movimiento,
                S.CedulaR AS CedulaColilla,
                dbo.fxSIFCorteAFechaInicio(P.Proceso) AS Inicio,
                dbo.fxSIFCorteAFecha(P.Proceso) AS Corte,
                S.Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.tipo, P.movimiento, P.cedula";


        protected override void PrepararConfiguracion(IDbConnection connection,
            CcProcesoMensualArchivoConfiguracionModel configuracion,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            _codigoInstitucionArchivo = string.IsNullOrWhiteSpace(configuracion.CodigoInstDeduc)
                ? request.CodInstitucion.ToString("00", CultureInfo.InvariantCulture)
                : configuracion.CodigoInstDeduc.Trim();
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF25RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var inicioStr = registro.Inicio.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            var corteStr = registro.Corte.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

            if (string.Equals(
                registro.TipoDeduc?.Trim(),
                TipoDeduccionMonto,
                StringComparison.OrdinalIgnoreCase))
            {
                return (registro.CedulaColilla ?? string.Empty).Trim()
                    + ";"
                    + _codigoInstitucionArchivo
                    + ";"
                    + inicioStr
                    + ";"
                    + corteStr
                    + ";"
                    + registro.CodDeduccion
                    + ";"
                    + registro.MontoActual.ToString(CultureInfo.InvariantCulture)
                    + ";;;"
                    + registro.Cedula
                    + ";"
                    + registro.Nombre;
            }

            return (registro.CedulaColilla ?? string.Empty).Trim()
                + ";"
                + _codigoInstitucionArchivo
                + ";"
                + inicioStr
                + ";31.12.9999"
                + ";"
                + registro.CodDeduccion
                + ";;"
                + registro.PorcDeduc.ToString(CultureInfo.InvariantCulture)
                + ";;"
                + registro.Cedula
                + ";"
                + registro.Nombre;
        }

        public sealed class CcProcesoMensualArchivoF25RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string CedulaColilla { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public decimal PorcDeduc { get; set; } = 0;
            public string TipoDeduc { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public DateTime Inicio { get; set; } = DateTime.MinValue;
            public DateTime Corte { get; set; } = DateTime.MinValue;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}

