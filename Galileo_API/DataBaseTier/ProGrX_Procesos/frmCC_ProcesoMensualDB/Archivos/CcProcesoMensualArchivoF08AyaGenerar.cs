using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF08AyaGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "08";
        private const string ContentTypeText = "text/plain";
        private const string ExtensionTxt = ".txt";
        private const string CodigoNo = "NO";
        private const string TipoCredito = "C";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var fechaServidor = ObtenerFechaServidor(connection);
            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var archivosGenerados = new List<string>();
            string ultimoArchivo;

            if (!EsCodigoNo(configuracion.CodigoCreditos))
            {
                ultimoArchivo = GenerarArchivoAnterior(
                    connection,
                    request,
                    configuracion,
                    fechaServidor,
                    rutaDirectorio);

                archivosGenerados.Add(ultimoArchivo);
            }

            ultimoArchivo = GenerarArchivoNuevo(
                connection,
                request,
                fechaServidor,
                rutaDirectorio);

            archivosGenerados.Add(ultimoArchivo);

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = archivosGenerados.Count > 0,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = Path.GetFileName(ultimoArchivo),
                RutaArchivo = ultimoArchivo,
                ContentType = ContentTypeText,
                ArchivoBytes = [],
                ArchivosGenerados = archivosGenerados
            };
        }

        private static CcProcesoMensualArchivoF08ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_aportes, '') AS CodigoAportes,
                    ISNULL(codigo_creditos, '') AS CodigoCreditos,
                    dbo.fxSIFCorteAFecha(@FechaProceso) AS FechaCorte
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF08ConfigDbModel>(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso
                }) ?? new CcProcesoMensualArchivoF08ConfigDbModel();
        }

        private static string GenerarArchivoAnterior(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF08ConfigDbModel configuracion,
            DateTime fechaServidor,
            string rutaDirectorio)
        {
            var nombreArchivo = CrearNombreArchivoAnterior(
                request.CodInstitucion,
                request.FechaProceso,
                fechaServidor);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var registros = ObtenerRegistrosCreditos(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var contenido = CrearContenidoArchivoAnterior(
                registros,
                configuracion,
                request.FechaProceso);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.GetEncoding(1252));

            return rutaArchivo;
        }

        private static string GenerarArchivoNuevo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            DateTime fechaServidor,
            string rutaDirectorio)
        {
            var nombreArchivo = CrearNombreArchivoNuevo(
                request.CodInstitucion,
                request.FechaProceso,
                fechaServidor);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var cadenas = ObtenerCadenasAyaNuevo(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var contenido = CrearContenidoCadenasNoVacias(cadenas);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.GetEncoding(1252));

            return rutaArchivo;
        }

        private static List<CcProcesoMensualArchivoF08RegistroDbModel> ObtenerRegistrosCreditos(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    P.Cedula,
                    P.Monto_Actual AS MontoActual,
                    P.Movimiento,
                    S.nombre AS Nombre,
                    S.direccion AS Direccion
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.cod_institucion = @CodInstitucion
                  AND P.tipo = @TipoCredito
                ORDER BY P.cedula";

            return [.. connection.Query<CcProcesoMensualArchivoF08RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    TipoCredito
                })];
        }

        private static List<string> ObtenerCadenasAyaNuevo(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                EXEC spPrm_Formato_AYA_New
                    @CodInstitucion,
                    @FechaProceso";

            return [.. connection.Query<CcProcesoMensualArchivoCadenaDbModel>(
                    query,
                    new
                    {
                        CodInstitucion = codInstitucion,
                        FechaProceso = fechaProceso
                    })
                .Select(x => x.Cadena ?? string.Empty)];
        }

        private static string CrearContenidoArchivoAnterior(
            IEnumerable<CcProcesoMensualArchivoF08RegistroDbModel> registros,
            CcProcesoMensualArchivoF08ConfigDbModel configuracion,
            decimal fechaProceso)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                var tipoMovimiento = ObtenerTipoMovimientoAya(registro.Movimiento);

                // VB6:
                // If i <> 3 Then 'Envia todos menos las exclusiones
                if (tipoMovimiento != 3)
                {
                    builder.AppendLine(CrearLineaArchivoAnterior(
                        registro,
                        configuracion,
                        fechaProceso));
                }
            }

            builder.AppendLine("!");

            return builder.ToString();
        }

        private static string CrearLineaArchivoAnterior(
            CcProcesoMensualArchivoF08RegistroDbModel registro,
            CcProcesoMensualArchivoF08ConfigDbModel configuracion,
            decimal fechaProceso)
        {
            var linea =
                Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    FormatearCedula(registro.Cedula),
                    "D",
                    " ",
                    30)
                + " "
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    configuracion.CodigoCreditos?.Trim(),
                    "D",
                    " ",
                    3)
                + " "
                + "P ";

            var montoSinDecimales = FormatearMontoStandardSinSeparadores(registro.MontoActual);

            linea += Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    montoSinDecimales.Trim(),
                    "I",
                    "0",
                    10)
                + " ";

            linea += Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "D", "0", 10) + " ";
            linea += Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "D", "0", 10) + " ";

            linea += "01/"
                + ObtenerMesProceso(fechaProceso)
                + "/"
                + ObtenerAnioProceso(fechaProceso)
                + " ";

            linea += configuracion.FechaCorte.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + " ";
            linea += "00.00 ";
            linea += fechaProceso.ToString(CultureInfo.InvariantCulture) + "1 ";
            linea += "00000.00";

            return Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                linea,
                "D",
                " ",
                129);
        }

        private static string CrearContenidoCadenasNoVacias(IEnumerable<string> cadenas)
        {
            var builder = new StringBuilder();

            foreach (var cadena in cadenas)
            {
                if (cadena.TrimEnd().Length > 0)
                {
                    builder.AppendLine(cadena);
                }
            }

            return builder.ToString();
        }

        private static int ObtenerTipoMovimientoAya(string movimiento)
        {
            return movimiento?.Trim().ToUpperInvariant() switch
            {
                "E" => 3, // Exclusión
                "I" => 1, // Inclusión
                "C" => 2, // Cambio
                _ => 4
            };
        }

        private static string FormatearCedula(string cedula)
        {
            var texto = cedula?.Trim() ?? string.Empty;

            if (decimal.TryParse(
                texto,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var numero))
            {
                return numero.ToString("0000000000", CultureInfo.InvariantCulture);
            }

            return texto;
        }

        private static string FormatearMontoStandardSinSeparadores(decimal monto)
        {
       
            var montoRedondeado = Math.Round(monto, 2, MidpointRounding.AwayFromZero);
            var montoCentimos = Convert.ToInt64(montoRedondeado * 100);

            return montoCentimos.ToString(CultureInfo.InvariantCulture);
        }

        private static string CrearNombreArchivoAnterior(
            int codInstitucion,
            decimal fechaProceso,
            DateTime fechaServidor)
        {
            var codigoInstitucion = codInstitucion.ToString("00", CultureInfo.InvariantCulture);
            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F08]{ExtensionTxt}";
        }

        private static string CrearNombreArchivoNuevo(
            int codInstitucion,
            decimal fechaProceso,
            DateTime fechaServidor)
        {
            var codigoInstitucion = codInstitucion.ToString("00", CultureInfo.InvariantCulture);
            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} -NUEVO- [{fechaServidorTexto}-F08]{ExtensionTxt}";
        }

        private static string ObtenerMesProceso(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            return fechaBase.Length >= 6
                ? fechaBase.Substring(4, 2)
                : string.Empty;
        }

        private static string ObtenerAnioProceso(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            return fechaBase.Length >= 4
                ? fechaBase[..4]
                : string.Empty;
        }

        private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }

        private static bool EsCodigoNo(string codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                CodigoNo,
                StringComparison.OrdinalIgnoreCase);
        }

        private sealed class CcProcesoMensualArchivoF08ConfigDbModel
        {
            public string CodigoAportes { get; set; } = string.Empty;
            public string CodigoCreditos { get; set; } = string.Empty;
            public DateTime? FechaCorte { get; set; }
        }

        private sealed class CcProcesoMensualArchivoF08RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Direccion { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoCadenaDbModel
        {
            public string Cadena { get; set; } = string.Empty;
        }
    }
}
