using Dapper;
using System.Data;
using System.Globalization;
using System.Security;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF20CenCenaiGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "20";
        private const string ContentTypeCsv = "text/csv";
        private const string ExtensionCsv = ".csv";

        private const string TipoAhorro = "A";
        private const string TipoExtraordinario = "E";
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

            var archivosGenerados = new List<string>
            {
                GenerarArchivoAnterior(
                    connection,
                    request,
                    configuracion,
                    fechaServidor,
                    rutaDirectorio),

                GenerarArchivoNuevo(
                    connection,
                    request,
                    configuracion,
                    fechaServidor,
                    rutaDirectorio)
            };

            var ultimoArchivo = archivosGenerados.LastOrDefault() ?? string.Empty;

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = archivosGenerados.Count > 0,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = Path.GetFileName(ultimoArchivo),
                RutaArchivo = ultimoArchivo,
                ContentType = ContentTypeCsv,
                ArchivoBytes = [],
                ArchivosGenerados = archivosGenerados
            };
        }

        private static CcProcesoMensualArchivoF20ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    ISNULL(planilla, '') AS Planilla,
                    ISNULL(codigo_aportes_env, '') AS CodigoAportesEnv,
                    ISNULL(codigo_creditos_env, '') AS CodigoCreditosEnv,
                    ISNULL(porc_ahorro, 0) AS PorcAhorro,
                    ISNULL(codigo_inst_deduc, '') AS CodigoInstDeduc,
                    ISNULL(IncInclusiones, 0) AS IncInclusiones,
                    ISNULL(IncExclusiones, 0) AS IncExclusiones,
                    ISNULL(IncModificaciones, 0) AS IncModificaciones,
                    ISNULL(IncMantienen, 0) AS IncMantienen,
                    ISNULL(porc_aporte, 0) AS PorcAporte,
                    dbo.fxSIFCorteAFechaInicio(@FechaProceso) AS FechaInicio,
                    dbo.fxSIFCorteAFecha(@FechaProceso) AS FechaCorte
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF20ConfigDbModel>(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso
                }) ?? new CcProcesoMensualArchivoF20ConfigDbModel();
        }

        private static string GenerarArchivoAnterior(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF20ConfigDbModel configuracion,
            DateTime fechaServidor,
            string rutaDirectorio)
        {
            var nombreArchivo = CrearNombreArchivoAnterior(
                request.CodInstitucion,
                request.FechaProceso,
                configuracion.CodigoInstDeduc,
                fechaServidor);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var movimientos = ObtenerMovimientos(configuracion);

            var registros = ObtenerRegistrosArchivoAnterior(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                movimientos);

            var contenido = CrearContenidoArchivoAnterior(
                registros,
                configuracion,
                request.NombreEmpresa);

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
            CcProcesoMensualArchivoF20ConfigDbModel configuracion,
            DateTime fechaServidor,
            string rutaDirectorio)
        {
            var nombreArchivo = CrearNombreArchivoNuevo(
                request.CodInstitucion,
                request.FechaProceso,
                configuracion.CodigoInstDeduc,
                fechaServidor);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var cadenas = ObtenerCadenasCecinaiNuevo(
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

        private static List<string> ObtenerMovimientos(
            CcProcesoMensualArchivoF20ConfigDbModel configuracion)
        {
            var movimientos = new List<string>();

            AgregarMovimientoSiAplica(movimientos, configuracion.IncInclusiones, "I");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncExclusiones, "E");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncModificaciones, "C");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncMantienen, "M");

            movimientos.Add("P");

            return movimientos;
        }

        private static void AgregarMovimientoSiAplica(
            List<string> movimientos,
            int indicador,
            string movimiento)
        {
            if (indicador == 1)
            {
                movimientos.Add(movimiento);
            }
        }

        private static List<CcProcesoMensualArchivoF20RegistroDbModel> ObtenerRegistrosArchivoAnterior(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso,
            IEnumerable<string> movimientos)
        {
            const string query = @"
                SELECT
                    P.cedula AS Cedula,
                    P.Tipo,
                    P.cod_deduccion AS CodDeduccion,
                    P.Movimiento,
                    P.Monto_Actual / 2 AS MontoActual
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.movimiento IN @Movimientos
                  AND P.cod_institucion = @CodInstitucion
                ORDER BY P.cedula, P.tipo, P.cod_deduccion, P.movimiento";

            return [.. connection.Query<CcProcesoMensualArchivoF20RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    Movimientos = movimientos,
                    CodInstitucion = codInstitucion
                })];
        }

        private static List<string> ObtenerCadenasCecinaiNuevo(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                EXEC spPrm_Formato_CECINAI_New
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
            IEnumerable<CcProcesoMensualArchivoF20RegistroDbModel> registros,
            CcProcesoMensualArchivoF20ConfigDbModel configuracion,
            string nombreEmpresa)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaArchivoAnterior(
                    registro,
                    configuracion,
                    nombreEmpresa));
            }

            return builder.ToString();
        }

        private static string CrearLineaArchivoAnterior(
            CcProcesoMensualArchivoF20RegistroDbModel registro,
            CcProcesoMensualArchivoF20ConfigDbModel configuracion,
            string nombreEmpresa)
        {
            var codigoTipo = ObtenerCodigoTipo(registro.Tipo, configuracion);

            return configuracion.CodigoInstDeduc.Trim()
                + ","
                + codigoTipo
                + ","
                + registro.Cedula.Trim()
                + ","
                + TomarIzquierda(nombreEmpresa, 30)
                + ",1,"
                + FormatearDecimalVb6(registro.MontoActual)
                + ",0,"
                + configuracion.FechaInicio.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                + ","
                + configuracion.FechaCorte.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                + ",0,CRC";
        }

        private static string ObtenerCodigoTipo(
            string tipo,
            CcProcesoMensualArchivoF20ConfigDbModel configuracion)
        {
            return tipo?.Trim().ToUpperInvariant() switch
            {
                TipoAhorro => configuracion.CodigoAportesEnv.Trim(),
                TipoExtraordinario => configuracion.CodigoCreditosEnv.Trim(),
                TipoCredito => configuracion.CodigoCreditosEnv.Trim(),
                _ => string.Empty
            };
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

        private static string TomarIzquierda(string? valor, int cantidad)
        {
            var texto = valor ?? string.Empty;

            return texto.Length > cantidad
                ? texto[..cantidad]
                : texto;
        }

        private static string FormatearDecimalVb6(decimal valor)
        {
            return valor.ToString(CultureInfo.InvariantCulture);
        }

        private static string CrearNombreArchivoAnterior(
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

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F20]{ExtensionCsv}";
        }

        private static string CrearNombreArchivoNuevo(
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

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} -NUEVO-  [{fechaServidorTexto}-F20]{ExtensionCsv}";
        }

        private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }

        private sealed class CcProcesoMensualArchivoF20ConfigDbModel
        {
            public string Planilla { get; set; } = string.Empty;
            public string CodigoAportesEnv { get; set; } = string.Empty;
            public string CodigoCreditosEnv { get; set; } = string.Empty;
            public decimal PorcAhorro { get; set; } = 0;
            public string CodigoInstDeduc { get; set; } = string.Empty;
            public int IncInclusiones { get; set; } = 0;
            public int IncExclusiones { get; set; } = 0;
            public int IncModificaciones { get; set; } = 0;
            public int IncMantienen { get; set; } = 0;
            public decimal PorcAporte { get; set; } = 0;
            public DateTime? FechaInicio { get; set; }
            public DateTime? FechaCorte { get; set; } 
        }

        private sealed class CcProcesoMensualArchivoF20RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
        }

        private sealed class CcProcesoMensualArchivoCadenaDbModel
        {
            public string Cadena { get; set; } = string.Empty;
        }
    }
}
