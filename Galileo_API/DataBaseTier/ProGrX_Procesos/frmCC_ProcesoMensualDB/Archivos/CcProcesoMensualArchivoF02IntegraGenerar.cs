using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF02IntegraGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "02";
        private const string ContentTypeCsv = "text/csv";

        private const string TipoAhorro = "A";
        private const string TipoExtraordinario = "E";
        private const string TipoCredito = "C";

        private const string PrefijoEnvio = "E";
        private const string PrefijoMatricula = "MD";
        private const string PrefijoIntegra = "CD";

        private const string ExtensionTxt = "txt";
        private const string ExtensionCsv = "csv";
        private readonly ArchivosGeneradosOptions _archivosOptions;
        public CcProcesoMensualArchivoF02IntegraGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions)
        {
            _archivosOptions = archivosOptions.Value;
        }

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(
                connection,
                request.CodInstitucion);

            var rutaBase = _archivosOptions.RutaBase;

            var contexto = CrearContextoArchivo(
                connection,
                request, rutaBase);
             

            var archivosGenerados = new List<string>
            {
                GenerarArchivo(rutaBase,
                    contexto.RutaDirectorio,
                    CrearNombreArchivo(PrefijoEnvio, configuracion.CodigoInstDeduc, contexto.FechaServidor, ExtensionTxt),
                    CrearContenidoEnvio(
                        ObtenerRegistrosEnvio(
                            connection,
                            request,
                            Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerMovimientosPorIndicadores(configuracion)),
                        configuracion.PorcAhorro)),

                GenerarArchivo(rutaBase,
                    contexto.RutaDirectorio,
                    CrearNombreArchivo(PrefijoMatricula, configuracion.CodigoInstDeduc, contexto.FechaServidor, ExtensionCsv),
                    CrearContenidoCadenas(
                        ObtenerCadenasDesdeSp(
                            connection,
                            SpIntegra.IntegraNew,
                            request.CodInstitucion,
                            request.FechaProceso))),

                GenerarArchivo(rutaBase,
                    contexto.RutaDirectorio,
                    CrearNombreArchivo(PrefijoIntegra, configuracion.CodigoInstDeduc, contexto.FechaServidor, ExtensionCsv),
                    CrearContenidoCadenas(
                        ObtenerCadenasDesdeSp(
                            connection,
                            SpIntegra.IntegraNewMatricula,
                            request.CodInstitucion,
                            request.FechaProceso)))
            };

            return CrearRespuesta(archivosGenerados);
        }

        private static CcProcesoMensualArchivoContextoModel CrearContextoArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request,string rutaBase)
        {
            return new CcProcesoMensualArchivoContextoModel
            {
                FechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection),
                RutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request, rutaBase)
            };
        }

        private static string GenerarArchivo(
            string rutaDirectorio,
            string nombreArchivo,
            string contenido, string rutaBase )
        {
            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(rutaBase,
                rutaDirectorio,
                nombreArchivo);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(rutaBase,
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return rutaArchivo;
        }

        private static List<CcProcesoMensualArchivoF02RegistroDbModel> ObtenerRegistrosEnvio(
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
                    P.Monto_Actual AS MontoActual,
                    ISNULL(S.cod_sector, 0) AS Sector
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.movimiento IN @Movimientos
                  AND P.cod_institucion = @CodInstitucion
                ORDER BY P.cedula, P.tipo, P.cod_deduccion, P.movimiento";

            return [.. connection.Query<CcProcesoMensualArchivoF02RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = request.FechaProceso,
                    Movimientos = movimientos,
                    CodInstitucion = request.CodInstitucion
                })];
        }

        private static List<string> ObtenerCadenasDesdeSp(
            IDbConnection connection,
            SpIntegra procedimiento,
            int codInstitucion,
            decimal fechaProceso)
        {
            var query = procedimiento switch
            {
                SpIntegra.IntegraNew => @"
                    EXEC spPrm_Formato_Integra_New
                        @CodInstitucion,
                        @FechaProceso",

                SpIntegra.IntegraNewMatricula => @"
                    EXEC spPrm_Formato_Integra_New_Matricula
                        @CodInstitucion,
                        @FechaProceso",

                _ => throw new InvalidOperationException(
                    $"Procedimiento no permitido para generar archivo: {procedimiento}.")
            };

            return [.. connection.Query<CcProcesoMensualArchivoCadenaDbModel>(
                    query,
                    new
                    {
                        CodInstitucion = codInstitucion,
                        FechaProceso = fechaProceso
                    })
                .Select(x => x.Cadena ?? string.Empty)];
        }

        private static string CrearContenidoEnvio(
            IEnumerable<CcProcesoMensualArchivoF02RegistroDbModel> registros,
            decimal porcAhorro)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaEnvio(registro, porcAhorro));
            }

            return builder.ToString();
        }

        private static string CrearLineaEnvio(
            CcProcesoMensualArchivoF02RegistroDbModel registro,
            decimal porcAhorro)
        {
            return string.Join(
                "\t",
                FormatearCedula(registro.Cedula),
                registro.CodDeduccion.Trim(),
                ObtenerMontoPorTipo(registro, porcAhorro),
                registro.Sector == 2 ? "2" : "0",
                "0");
        }

        private static string ObtenerMontoPorTipo(
            CcProcesoMensualArchivoF02RegistroDbModel registro,
            decimal porcAhorro)
        {
            return registro.Tipo?.Trim().ToUpperInvariant() switch
            {
                TipoAhorro => porcAhorro.ToString("######0.00", CultureInfo.InvariantCulture),
                TipoExtraordinario => registro.MontoActual.ToString("############0.00", CultureInfo.InvariantCulture),
                TipoCredito => registro.MontoActual.ToString("############0.00", CultureInfo.InvariantCulture),
                _ => registro.MontoActual.ToString("############0.00", CultureInfo.InvariantCulture)
            };
        }

        private static string CrearContenidoCadenas(IEnumerable<string> cadenas)
        {
            var builder = new StringBuilder();

            foreach (var cadena in cadenas)
            {
                builder.AppendLine(cadena);
            }

            return builder.ToString();
        }

        private static string FormatearCedula(string? cedula)
        {
            var valor = cedula?.Trim() ?? string.Empty;

            if (valor.Length == 9 &&
                long.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numero))
            {
                return numero.ToString("0000000000", CultureInfo.InvariantCulture);
            }

            return valor;
        }

        private static string CrearNombreArchivo(
            string prefijo,
            string codigoInstitucion,
            DateTime fecha,
            string extension)
        {
            var codigo = codigoInstitucion?.Trim() ?? string.Empty;
            var fechaTexto = fecha.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            return $"{prefijo}-{codigo}-{fechaTexto}-01.{extension}";
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

        private enum SpIntegra
        {
            IntegraNew,
            IntegraNewMatricula
        }

        private sealed class CcProcesoMensualArchivoContextoModel
        {
            public DateTime FechaServidor { get; set; } = DateTime.MinValue;
            public string RutaDirectorio { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF02RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public int Sector { get; set; } = 0;
        }

        private sealed class CcProcesoMensualArchivoCadenaDbModel
        {
            public string Cadena { get; set; } = string.Empty;
        }
    }
}
