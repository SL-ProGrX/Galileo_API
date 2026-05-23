using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF23CorreosGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "23";
        private const string ContentTypeCsv = "text/csv";
        private const string ExtensionCsv = ".csv";
        private const string MovimientoExclusion = "E";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var fechaServidor = ObtenerFechaServidor(connection);

            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                fechaServidor);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var registros = ObtenerRegistros(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var contenido = CrearContenidoArchivo(registros);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.GetEncoding(1252));

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = true,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = nombreArchivo,
                RutaArchivo = rutaArchivo,
                ContentType = ContentTypeCsv,
                ArchivoBytes = [],
                ArchivosGenerados = [rutaArchivo ]
            };
        }

        private static List<CcProcesoMensualArchivoF23RegistroDbModel> ObtenerRegistros(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
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

            return [.. connection.Query<CcProcesoMensualArchivoF23RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    MovimientoExclusion
                })];
        }

        private static string CrearContenidoArchivo(
            IEnumerable<CcProcesoMensualArchivoF23RegistroDbModel> registros)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaArchivo(registro));
            }

            return builder.ToString();
        }

        private static string CrearLineaArchivo(
            CcProcesoMensualArchivoF23RegistroDbModel registro)
        {
            return registro.Cedula.Trim()
                + ","
                + (registro.Nombre ?? string.Empty).Replace(",", " ")
                + ","
                + FormatearDecimalVb6(registro.MontoActual);
        }

        private static string FormatearDecimalVb6(decimal valor)
        {
            return valor.ToString(CultureInfo.InvariantCulture);
        }

        private static string CrearNombreArchivo(
            int codInstitucion,
            decimal fechaProceso,
            DateTime fechaServidor)
        {
            var codigoInstitucion = codInstitucion.ToString("00", CultureInfo.InvariantCulture);
            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F23]{ExtensionCsv}";
        }

        private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }

        private sealed class CcProcesoMensualArchivoF23RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
