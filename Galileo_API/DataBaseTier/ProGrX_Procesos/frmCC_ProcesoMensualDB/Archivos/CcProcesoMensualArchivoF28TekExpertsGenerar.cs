using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF28TekExpertsGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "28";
        private const string ContentTypeCsv = "text/csv";
        private const string ExtensionCsv = ".csv";

        private const string Encabezado =
                    "CODIGO;TEAM;COLABORADOR;ENTRY_DATE;LOCATION;TERMINATION_DATE;02-D31;02-D32;02-D33;02-D36;02-D37;02-D35;02-D34;02-D38;02-D30";
        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion);

            var fechaServidor = ObtenerFechaServidor(connection);

            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                configuracion.CodigoInstDeduc,
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
                ArchivosGenerados = [rutaArchivo]
            };
        }

        private static CcProcesoMensualArchivoF28ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_inst_deduc, '') AS CodigoInstDeduc
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF28ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF28ConfigDbModel();
        }

        private static List<dynamic> ObtenerRegistros(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                EXEC spPrm_File_028_ASETEK
                    @CodInstitucion,
                    @FechaProceso";

            return [.. connection.Query(
                    query,
                    new
                    {
                        CodInstitucion = codInstitucion,
                        FechaProceso = fechaProceso
                    })];
        }

        private static string CrearContenidoArchivo(
            IEnumerable<dynamic> registros)
        {
            var builder = new StringBuilder();

            // VB6 imprime títulos antes de recorrer el recordset.
            builder.AppendLine(Encabezado);

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaArchivo(registro));
            }

            return builder.ToString();
        }

        private static string CrearLineaArchivo(dynamic registro)
        {
            var fila = (IDictionary<string, object>)registro;

            return ObtenerValor(fila, "Codigo")
                + ";"
                + ObtenerValor(fila, "Team")
                + ";"
                + ObtenerValor(fila, "Colaborador")
                + ";"
                + ObtenerValor(fila, "Entry_Date")
                + ";"
                + ObtenerValor(fila, "Location")
                + ";"
                + ObtenerValor(fila, "Termination_Date")
                + ";"
                + ObtenerValor(fila, "02-D31")
                + ";"
                + ObtenerValor(fila, "02-D32")
                + ";"
                + ObtenerValor(fila, "02-D33")
                + ";"
                + ObtenerValor(fila, "02-D36")
                + ";"
                + ObtenerValor(fila, "02-D37")
                + ";"
                + ObtenerValor(fila, "02-D35")
                + ";"
                + ObtenerValor(fila, "02-D34")
                + ";"
                + ObtenerValor(fila, "02-D38")
                + ";"
                + ObtenerValor(fila, "02-D30");
        }

        private static string ObtenerValor(
            IDictionary<string, object> fila,
            string columna)
        {
            if (!fila.TryGetValue(columna, out var valor) || valor is null)
            {
                return string.Empty;
            }

            return valor switch
            {
                DateTime fecha => fecha.ToString(CultureInfo.InvariantCulture),
                decimal numero => numero.ToString(CultureInfo.InvariantCulture),
                double numero => numero.ToString(CultureInfo.InvariantCulture),
                float numero => numero.ToString(CultureInfo.InvariantCulture),
                _ => valor.ToString() ?? string.Empty
            };
        }

        private static string CrearNombreArchivo(
            int codInstitucion,
            decimal fechaProceso,
            string codigoInstDeduc,
            DateTime fechaServidor)
        {
            var codigoInstitucion = string.IsNullOrWhiteSpace(codigoInstDeduc)
                ? codInstitucion.ToString("00", CultureInfo.InvariantCulture)
                : codigoInstDeduc.Trim();

            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            // VB6: aunque el método es F28, el archivo sale como F25.
            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F25]{ExtensionCsv}";
        }

        private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }

        private sealed class CcProcesoMensualArchivoF28ConfigDbModel
        {
            public string CodigoInstDeduc { get; set; } = string.Empty;
        }
    }
}
