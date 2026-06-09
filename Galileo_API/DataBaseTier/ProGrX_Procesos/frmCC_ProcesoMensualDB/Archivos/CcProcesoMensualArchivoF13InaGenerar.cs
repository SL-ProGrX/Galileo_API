using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF13InaGenerar :  CcProcesoMensualArchivoPlanoGenerarBase<CcProcesoMensualArchivoF13InaGenerar.CcProcesoMensualArchivoRegistroDbModel>
    {
        private string _codigoCreditosEnv = string.Empty;

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["13"];

        protected override string CodigoPlanillaEnvio => "13";
        protected override string CodigoFormato => "F13";
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

        public CcProcesoMensualArchivoF13InaGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        {
        }
        public override CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion);

            _codigoCreditosEnv = configuracion.CodigoCreditosEnv;

            return base.GenerarArchivo(connection, request);
        }
        protected override string CrearLineaArchivo(
           CcProcesoMensualArchivoRegistroDbModel registro,
           CcProcesoMensualGeneraArchivoRequest request)
        {
            return Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    string.Empty,
                    "I",
                    "0",
                    10)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    registro.Cedula,
                    "I",
                    "0",
                    10)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    string.Empty,
                    "I",
                    "0",
                    11)
                + FormatearMontoF13(registro.MontoActual)
                + _codigoCreditosEnv.Trim()
                + "0002000";
        }

        private static CcProcesoMensualArchivoF13ConfigDbModel ObtenerConfiguracion(
          IDbConnection connection,
          int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_creditos_env, '') AS CodigoCreditosEnv
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF13ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF13ConfigDbModel();
        }
        private static string FormatearMontoF13(decimal monto)
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

        private sealed class CcProcesoMensualArchivoF13ConfigDbModel
        {
            public string CodigoCreditosEnv { get; set; } = string.Empty;
        }

    }
}
