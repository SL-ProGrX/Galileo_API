using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF06IceCentralGenerar : CcProcesoMensualArchivoPlanoGeneratorBase<CcProcesoMensualArchivoRegistroDbModel>
    {
        private const string CodigoNo = "NO";
        private const string TipoCredito = "C";

        private IDbConnection? _connection;

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["06"];

        protected override string CodigoPlanillaEnvio => "06";
        protected override string CodigoFormato => "F06";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        // No se usa porque F06 usa ObtenerRegistrosGeneral.
        protected override string QueryRegistros => string.Empty;

        public override CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion);

            if (EsCodigoNo(configuracion.CodigoCreditos))
            {
                return CrearRespuestaSinGenerar(
                    connection,
                    request);
            }

            _connection = connection;

            return base.GenerarArchivo(connection, request);
        }

        protected override IEnumerable<CcProcesoMensualArchivoRegistroDbModel> ObtenerRegistros(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRegistrosGeneral(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                TipoCredito);
        }

        protected override IEnumerable<CcProcesoMensualArchivoRegistroDbModel> FiltrarRegistros(
            IEnumerable<CcProcesoMensualArchivoRegistroDbModel> registros)
        {
            return registros.Where(registro => ObtenerTipoMovimiento(registro.Movimiento) != 4);
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoRegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            if (_connection is null)
            {
                return string.Empty;
            }

            var tipoMovimiento = ObtenerTipoMovimiento(registro.Movimiento);

            var cedula = Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                registro.Cedula,
                "I",
                "0",
                9);

            if (tipoMovimiento == 1)
            {
                return Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    cedula.Trim(),
                    "D",
                    "0",
                    30);
            }

            var montoTotal = ObtenerMontoTotalCredito(
                _connection,
                registro.Cedula,
                request.FechaProceso);

            var montoMensual = Convert.ToInt64(registro.MontoActual * 100);

            return cedula
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    montoTotal.ToString(CultureInfo.InvariantCulture),
                    "I",
                    "0",
                    11)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    montoMensual.ToString(CultureInfo.InvariantCulture),
                    "I",
                    "0",
                    10);
        }

        private static CcProcesoMensualArchivoGeneradoModel CrearRespuestaSinGenerar(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);

            var nombreArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CrearNombreArchivoEstandar(
                request.CodInstitucion,
                request.FechaProceso,
                string.Empty,
                fechaServidor,
                "F06",
                ".txt");

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = false,
                CodigoPlanillaEnvio = "06",
                NombreArchivo = nombreArchivo,
                RutaArchivo = rutaArchivo,
                ContentType = "text/plain",
                ArchivoBytes = [],
                ArchivosGenerados = []
            };
        }

        private static CcProcesoMensualArchivoF06ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_creditos, '') AS CodigoCreditos
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF06ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF06ConfigDbModel();
        }

        private static long ObtenerMontoTotalCredito(
            IDbConnection connection,
            string cedula,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    ISNULL(SUM(montoapr), 0) AS Monto
                FROM reg_creditos
                WHERE prideduc <= @FechaProceso
                  AND estado = 'A'
                  AND cedula = @Cedula";

            var monto = connection.QueryFirstOrDefault<decimal>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    Cedula = cedula
                });

            return Convert.ToInt64(monto * 100);
        }

        private static int ObtenerTipoMovimiento(string? movimiento)
        {
            return movimiento?.Trim().ToUpperInvariant() switch
            {
                "E" => 1,
                "I" => 2,
                "C" => 3,
                _ => 4
            };
        }

        private static bool EsCodigoNo(string? codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                CodigoNo,
                StringComparison.OrdinalIgnoreCase);
        }

        private sealed class CcProcesoMensualArchivoF06ConfigDbModel
        {
            public string CodigoCreditos { get; set; } = string.Empty;
        }
    }
}
