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

            public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
                IDbConnection connection,
                CcProcesoMensualGeneraArchivoRequest request)
            {
                var configuracion = ObtenerConfiguracion(
                    connection,
                    request.CodInstitucion,
                    request.FechaProceso);

                var contexto = CrearContextoArchivo(
                    connection,
                    request,
                    configuracion.CodigoInstDeduc);

                var archivosGenerados = new List<string>
            {
                GenerarArchivo(
                    rutaDirectorio: contexto.RutaDirectorio,
                    nombreArchivo: CrearNombreArchivo(
                        request,
                        configuracion.CodigoInstDeduc,
                        contexto.FechaServidor,
                        esNuevo: false),
                    contenido: CrearContenidoArchivoAnterior(
                        ObtenerRegistrosArchivoAnterior(
                            connection,
                            request,
                            Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerMovimientosPorIndicadores(configuracion)),
                        configuracion,
                        request.NombreEmpresa)),

                GenerarArchivo(
                    rutaDirectorio: contexto.RutaDirectorio,
                    nombreArchivo: CrearNombreArchivo(
                        request,
                        configuracion.CodigoInstDeduc,
                        contexto.FechaServidor,
                        esNuevo: true),
                    contenido: Helpers.CcProcesoMensualArchivoRutaHelperDb.CrearContenidoCadenasNoVacias(
                        ObtenerCadenasCecinaiNuevo(
                            connection,
                            request.CodInstitucion,
                            request.FechaProceso)))
            };

                return CrearRespuesta(archivosGenerados);
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

            private static CcProcesoMensualArchivoContextoModel CrearContextoArchivo(
                IDbConnection connection,
                CcProcesoMensualGeneraArchivoRequest request,
                string codigoInstDeduc)
            {
                return new CcProcesoMensualArchivoContextoModel
                {
                    FechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection),
                    RutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request),
                    CodigoInstitucionArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerCodigoInstitucionArchivo(
                        request.CodInstitucion,
                        codigoInstDeduc)
                };
            }

            private static string GenerarArchivo(
                string rutaDirectorio,
                string nombreArchivo,
                string contenido)
            {
                var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                    rutaDirectorio,
                    nombreArchivo);

                Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                    rutaDirectorio,
                    rutaArchivo,
                    contenido,
                    Encoding.GetEncoding(1252));

                return rutaArchivo;
            }

            private static List<CcProcesoMensualArchivoF20RegistroDbModel> ObtenerRegistrosArchivoAnterior(
                IDbConnection connection,
                CcProcesoMensualGeneraArchivoRequest request,
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
                    FechaProceso = request.FechaProceso,
                    Movimientos = movimientos,
                    CodInstitucion = request.CodInstitucion
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
                    builder.AppendLine(
                        CrearLineaArchivoAnterior(
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
                return string.Join(
                    ",",
                    configuracion.CodigoInstDeduc.Trim(),
                    ObtenerCodigoTipo(registro.Tipo, configuracion),
                    registro.Cedula.Trim(),
                    Helpers.CcProcesoMensualArchivoRutaHelperDb.TomarIzquierda(nombreEmpresa, 30),
                    "1",
                    registro.MontoActual.ToString(CultureInfo.InvariantCulture),
                    "0",
                    configuracion.FechaInicio.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    configuracion.FechaCorte.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    "0",
                    "CRC");
            }

            private static string ObtenerCodigoTipo(
                string? tipo,
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

            private static string CrearNombreArchivo(
                CcProcesoMensualGeneraArchivoRequest request,
                string codigoInstDeduc,
                DateTime fechaServidor,
                bool esNuevo)
            {
                var codigoInstitucion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerCodigoInstitucionArchivo(
                    request.CodInstitucion,
                    codigoInstDeduc);

                var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(
                    request.FechaProceso);

                var fechaServidorTexto = fechaServidor.ToString(
                    "ddMMyyyy",
                    CultureInfo.InvariantCulture);

                var indicadorNuevo = esNuevo ? " -NUEVO- " : string.Empty;

                return $"E-{codigoInstitucion}_{fechaProcesoTexto}{indicadorNuevo} [{fechaServidorTexto}-F20]{ExtensionCsv}";
            }

            private static CcProcesoMensualArchivoGeneradoModel CrearRespuesta(
                List<string> archivosGenerados)
            {
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

            private sealed class CcProcesoMensualArchivoF20ConfigDbModel
                : CcProcesoMensualArchivoConfiguracionModel
            {
                public DateTime FechaInicio { get; set; } = DateTime.MinValue;
                public DateTime FechaCorte { get; set; } = DateTime.MinValue;
            }

            private sealed class CcProcesoMensualArchivoF20RegistroDbModel
            {
                public string Cedula { get; set; } = string.Empty;
                public string Tipo { get; set; } = string.Empty;
                public string CodDeduccion { get; set; } = string.Empty;
                public string Movimiento { get; set; } = string.Empty;
                public decimal MontoActual { get; set; }
            }

            private sealed class CcProcesoMensualArchivoCadenaDbModel
            {
                public string Cadena { get; set; } = string.Empty;
            }

            private sealed class CcProcesoMensualArchivoContextoModel
            {
                public DateTime FechaServidor { get; set; } = DateTime.MinValue;
                public string RutaDirectorio { get; set; } = string.Empty;
                public string CodigoInstitucionArchivo { get; set; } = string.Empty;
            }
        }
}
