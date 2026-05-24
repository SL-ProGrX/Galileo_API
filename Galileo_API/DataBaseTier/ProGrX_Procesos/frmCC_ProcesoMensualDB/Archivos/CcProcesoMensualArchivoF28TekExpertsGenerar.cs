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
        private const string CodigoFormatoArchivo = "F28";
        private const string ContentTypeCsv = "text/csv";
        private const string ExtensionCsv = ".csv";

        private static readonly string[] Encabezados =
        [
            "CODIGO",
            "TEAM",
            "COLABORADOR",
            "ENTRY_DATE",
            "LOCATION",
            "TERMINATION_DATE",
            "02-D31",
            "02-D32",
            "02-D33",
            "02-D36",
            "02-D37",
            "02-D35",
            "02-D34",
            "02-D38",
            "02-D30"
        ];

        private static readonly string[] Columnas =
        [
            "Codigo",
            "Team",
            "Colaborador",
            "Entry_Date",
            "Location",
            "Termination_Date",
            "02-D31",
            "02-D32",
            "02-D33",
            "02-D36",
            "02-D37",
            "02-D35",
            "02-D34",
            "02-D38",
            "02-D30"
        ];
        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];
        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
                   IDbConnection connection,
                   CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(
                connection,
                request.CodInstitucion);

            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);

            var nombreArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CrearNombreArchivoEstandar(
                request.CodInstitucion,
                request.FechaProceso,
                configuracion.CodigoInstDeduc,
                fechaServidor,
                CodigoFormatoArchivo,
                ExtensionCsv);

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

            return CrearRespuesta(
                nombreArchivo,
                rutaArchivo);
        }

        private static IEnumerable<dynamic> ObtenerRegistros(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                EXEC spPrm_File_028_ASETEK
                    @CodInstitucion,
                    @FechaProceso";

            return connection.Query(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso
                });
        }

        private static string CrearContenidoArchivo(IEnumerable<dynamic> registros)
        {
            var builder = new StringBuilder();

            builder.AppendLine(CrearEncabezado());

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaArchivo(registro));
            }

            return builder.ToString();
        }

        private static string CrearEncabezado()
        {
            return string.Join(";", Encabezados);
        }

        private static string CrearLineaArchivo(dynamic registro)
        {
            var fila = (IDictionary<string, object>)registro;

            var campos = Columnas.Select(columna =>
                ObtenerValor(
                    fila,
                    columna));

            return string.Join(";", campos);
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

        private static CcProcesoMensualArchivoGeneradoModel CrearRespuesta(
            string nombreArchivo,
            string rutaArchivo)
        {
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
    }
}
