using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF06IceCentralGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "06";
        private const string ContentTypeText = "text/plain";
        private const string ExtensionTxt = ".txt";
        private const string CodigoNo = "NO";
        private const string TipoCredito = "C";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion);

            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);

            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                fechaServidor);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            if (EsCodigoNo(configuracion.CodigoCreditos))
            {
                return new CcProcesoMensualArchivoGeneradoModel
                {
                    Generado = false,
                    CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                    NombreArchivo = nombreArchivo,
                    RutaArchivo = rutaArchivo,
                    ContentType = ContentTypeText,
                    ArchivoBytes = [],
                    ArchivosGenerados = []
                };
            }

            var registros = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRegistrosGeneral(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                TipoCredito );

            var contenido = CrearContenidoArchivo(
                connection,
                registros,
                request.FechaProceso);

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
                ContentType = ContentTypeText,
                ArchivoBytes = [],
                ArchivosGenerados = [rutaArchivo]
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


        private static string CrearContenidoArchivo(
            IDbConnection connection,
            IEnumerable<CcProcesoMensualArchivoRegistroDbModel> registros,
            decimal fechaProceso)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                var tipoMovimiento = ObtenerTipoMovimiento(registro.Movimiento);

                if (tipoMovimiento != 4)
                {
                    builder.AppendLine(CrearLineaArchivo(
                        connection,
                        registro,
                        fechaProceso,
                        tipoMovimiento));
                }
            }

            return builder.ToString();
        }

        private static string CrearLineaArchivo(
            IDbConnection connection,
            CcProcesoMensualArchivoRegistroDbModel registro,
            decimal fechaProceso,
            int tipoMovimiento)
        {
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
                connection,
                registro.Cedula,
                fechaProceso);

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

        private static int ObtenerTipoMovimiento(string movimiento)
        {
            return movimiento?.Trim().ToUpperInvariant() switch
            {
                "E" => 1, // Exclusión
                "I" => 2, // Inclusión
                "C" => 3, // Cambio
                _ => 4    // Inválido / mantiene / no procesado
            };
        }

        private static string CrearNombreArchivo(
            int codInstitucion,
            decimal fechaProceso,
            DateTime fechaServidor)
        {
            var codigoInstitucion = codInstitucion.ToString("00", CultureInfo.InvariantCulture);
            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F06]{ExtensionTxt}";
        }


        private static bool EsCodigoNo(string codigo)
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
