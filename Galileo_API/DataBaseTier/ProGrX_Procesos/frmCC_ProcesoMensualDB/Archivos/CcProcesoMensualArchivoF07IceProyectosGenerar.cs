using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF07IceProyectosGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "07";
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

            var fechaServidor = ObtenerFechaServidor(connection);

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

            var registros = ObtenerRegistrosCreditos(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

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

        private static List<CcProcesoMensualArchivoF07RegistroDbModel> ObtenerRegistrosCreditos(
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

            return [.. connection.Query<CcProcesoMensualArchivoF07RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    TipoCredito
                })];
        }

        private static string CrearContenidoArchivo(
            IDbConnection connection,
            IEnumerable<CcProcesoMensualArchivoF07RegistroDbModel> registros,
            decimal fechaProceso)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                var tipoMovimiento = ObtenerTipoMovimiento(registro.Movimiento);

                // VB6:
                // Solo se procesan Inclusiones
                // If x = 2 Then Print #fnFile, vLinea
                if (tipoMovimiento == 2)
                {
                    builder.AppendLine(CrearLineaArchivo(
                        connection,
                        registro,
                        fechaProceso));
                }
            }

            return builder.ToString();
        }

        private static string CrearLineaArchivo(
            IDbConnection connection,
            CcProcesoMensualArchivoF07RegistroDbModel registro,
            decimal fechaProceso)
        {
            var nombre = SepararNombre(registro.Nombre);

            var credito = ObtenerDatosCredito(
                connection,
                registro.Cedula,
                fechaProceso);

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
                 + FormatearDecimalVb6(monto)
                 + "\t"
                 + FormatearDecimalVb6(plazo)
                 + "\t"
                 + FormatearDecimalVb6(registro.MontoActual);
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

        private static CcProcesoMensualArchivoF07NombreModel SepararNombre(string? nombreCompleto)
        {
            var resultado = new CcProcesoMensualArchivoF07NombreModel();
            var texto = nombreCompleto ?? string.Empty;
            var posicion = 1;

            foreach (var caracter in texto)
            {
                if (caracter == ' ')
                {
                    posicion++;
                    continue;
                }

                switch (posicion)
                {
                    case 1:
                        resultado.Apellido1 += caracter;
                        break;

                    case 2:
                        resultado.Apellido2 += caracter;
                        break;

                    case 3:
                        resultado.Nombre1 += caracter;
                        break;

                    case 4:
                        resultado.Nombre2 += caracter;
                        break;
                }
            }

            return resultado;
        }

        private static int ObtenerTipoMovimiento(string movimiento)
        {
            return movimiento?.Trim().ToUpperInvariant() switch
            {
                "E" => 1,
                "I" => 2,
                "C" => 3,
                _ => 4
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

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F07]{ExtensionTxt}";
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

        private static string FormatearDecimalVb6(decimal valor)
        {
            return valor.ToString(CultureInfo.InvariantCulture);
        }

        private sealed class CcProcesoMensualArchivoF07ConfigDbModel
        {
            public string CodigoCreditos { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF07RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Direccion { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF07CreditoDbModel
        {
            public int Cantidad { get; set; } = 0;
            public decimal Monto { get; set; } = 0;
            public decimal Saldo { get; set; } = 0;
            public decimal Plazo { get; set; } = 0;
        }

        private sealed class CcProcesoMensualArchivoF07NombreModel
        {
            public string Apellido1 { get; set; } = string.Empty;
            public string Apellido2 { get; set; } = string.Empty;
            public string Nombre1 { get; set; } = string.Empty;
            public string Nombre2 { get; set; } = string.Empty;
        }
    }
}
