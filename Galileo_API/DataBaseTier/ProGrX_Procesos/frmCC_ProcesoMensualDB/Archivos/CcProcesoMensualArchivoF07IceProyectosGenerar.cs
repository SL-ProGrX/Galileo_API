using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF07IceProyectosGenerar :  CcProcesoMensualArchivoPlanoGeneratorBase<CcProcesoMensualArchivoRegistroDbModel>

    {
        private const string CodigoNo = "NO";
        private const string TipoCredito = "C";

        private IDbConnection? _connection;

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["07"];

        protected override string CodigoPlanillaEnvio => "07";
        protected override string CodigoFormato => "F07";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        // No se usa porque F07 usa ObtenerRegistrosGeneral.
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
            // VB6: Solo se procesan Inclusiones.
            return registros.Where(registro => ObtenerTipoMovimiento(registro.Movimiento) == 2);
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoRegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            if (_connection is null)
            {
                return string.Empty;
            }

            var nombre = Helpers.CcProcesoMensualArchivoRutaHelperDb.SepararNombre(
                registro.Nombre);

            var credito = ObtenerDatosCredito(
                _connection,
                registro.Cedula,
                request.FechaProceso);

            var monto = credito.Cantidad > 0
                ? credito.Monto
                : registro.MontoActual;

            var plazo = credito.Cantidad > 0
                ? credito.Plazo
                : 1m;

            return registro.Cedula.Trim()
                + "\t"
                + nombre.Apellido1
                + "\t"
                + nombre.Apellido2
                + "\t"
                + nombre.Nombre1
                + " "
                + nombre.Nombre2
                + "\t"
                + monto.ToString(CultureInfo.InvariantCulture)
                + "\t"
                + plazo.ToString(CultureInfo.InvariantCulture)
                + "\t"
                + registro.MontoActual.ToString(CultureInfo.InvariantCulture);
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
                "F07",
                ".txt");

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = false,
                CodigoPlanillaEnvio = "07",
                NombreArchivo = nombreArchivo,
                RutaArchivo = rutaArchivo,
                ContentType = "text/plain",
                ArchivoBytes = [],
                ArchivosGenerados = []
            };
        }

        private static CcProcesoMensualArchivoF07ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_creditos, '') AS CodigoCreditos
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF07ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF07ConfigDbModel();
        }

        private static CcProcesoMensualArchivoF07CreditoDbModel ObtenerDatosCredito(
            IDbConnection connection,
            string cedula,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    COUNT(*) AS Cantidad,
                    ISNULL(SUM(montoapr), 0) AS Monto,
                    ISNULL(SUM(Saldo), 0) AS Saldo,
                    ISNULL(AVG(plazo), 1) AS Plazo
                FROM reg_creditos
                WHERE prideduc <= @FechaProceso
                  AND estado = 'A'
                  AND cedula = @Cedula";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF07CreditoDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    Cedula = cedula
                }) ?? new CcProcesoMensualArchivoF07CreditoDbModel
                {
                    Cantidad = 0,
                    Monto = 0,
                    Plazo = 1
                };
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

        private sealed class CcProcesoMensualArchivoF07ConfigDbModel
        {
            public string CodigoCreditos { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF07CreditoDbModel
        {
            public int Cantidad { get; set; }
            public decimal Monto { get; set; }
            public decimal Saldo { get; set; }
            public decimal Plazo { get; set; }
        }
    }
}
