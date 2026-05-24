using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF27RecopeGenerar : CcProcesoMensualArchivoConMovimientosGeneratorBase<CcProcesoMensualArchivoF27RecopeGenerar.CcProcesoMensualArchivoF27RegistroDbModel>

    {
        private const string TipoDeduccionMonto = "M"; 
        private DateTime _fechaArchivo = DateTime.MinValue;

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["27"];

        protected override string CodigoPlanillaEnvio => "27";
        protected override string CodigoFormato => "F27";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Cod_Deduccion AS CodDeduccion,
                P.Monto_Actual AS MontoActual,
                P.Porc_Deduc AS PorcDeduc,
                P.Tipo_Deduc AS TipoDeduc,
                P.Movimiento,
                S.CedulaR AS CedulaColilla,
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
            _fechaArchivo = ObtenerFechaArchivo(request.FechaProceso);
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF27RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var cedula = FormatearCedula(registro.Cedula);
            var fechaTexto = _fechaArchivo.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            var monto = registro.MontoActual.ToString(CultureInfo.InvariantCulture);

            if (string.Equals(
                registro.TipoDeduc?.Trim(),
                TipoDeduccionMonto,
                StringComparison.OrdinalIgnoreCase))
            {
                return cedula
                    + "\t"
                    + fechaTexto
                    + "\t"
                    + registro.CodDeduccion
                    + "\t"
                    + monto;
            }

            return cedula
                + "\t"
                + fechaTexto
                + "\t"
                + registro.CodDeduccion
                + "\t"
                + "\t"
                + monto;
        }

        private static string FormatearCedula(string? cedula)
        {
            var texto = cedula?.Trim() ?? string.Empty;

            return decimal.TryParse(
                texto,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var numero)
                    ? numero.ToString("0000000000", CultureInfo.InvariantCulture)
                    : texto;
        }

        private static DateTime ObtenerFechaArchivo(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            var anio = int.Parse(fechaBase[..4], CultureInfo.InvariantCulture);
            var mes = int.Parse(fechaBase.AsSpan(4, 2), CultureInfo.InvariantCulture);

            return new DateTime(
                anio,
                mes,
                14,
                0,
                0,
                0,
                DateTimeKind.Unspecified);
        }

        public sealed class CcProcesoMensualArchivoF27RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public decimal PorcDeduc { get; set; } = 0;
            public string TipoDeduc { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public string CedulaColilla { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
