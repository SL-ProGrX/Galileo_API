using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF02IntegraGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "02"; 
        private const string ContentTypeCsv = "text/csv";
        private const string TipoAhorro = "A";
        private const string TipoExtraordinario = "E";
        private const string TipoCredito = "C";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } =  [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo( IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);
            var fechaServidor = ObtenerFechaServidor(connection);

            var archivosGenerados = new List<string>
            {
                GenerarArchivoEnvio(
                    connection,
                    request,
                    configuracion,
                    rutaDirectorio,
                    fechaServidor),

                GenerarArchivoMatricula(
                    connection,
                    request,
                    configuracion,
                    rutaDirectorio,
                    fechaServidor),

                GenerarArchivoIntegra(
                    connection,
                    request,
                    configuracion,
                    rutaDirectorio,
                    fechaServidor)
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

        private static CcProcesoMensualArchivoF02ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(planilla, '') AS Planilla,
                    ISNULL(codigo_aportes_env, '') AS CodigoAportesEnv,
                    ISNULL(codigo_creditos_env, '') AS CodigoCreditosEnv,
                    ISNULL(porc_ahorro, 0) AS PorcAhorro,
                    ISNULL(porc_aporte, 0) AS PorcAporte,
                    ISNULL(codigo_inst_deduc, '') AS CodigoInstDeduc,
                    ISNULL(IncInclusiones, 0) AS IncInclusiones,
                    ISNULL(IncExclusiones, 0) AS IncExclusiones,
                    ISNULL(IncModificaciones, 0) AS IncModificaciones,
                    ISNULL(IncMantienen, 0) AS IncMantienen
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF02ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF02ConfigDbModel();
        }

        private static string GenerarArchivoEnvio(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF02ConfigDbModel configuracion,
            string rutaDirectorio,
            DateTime fechaServidor)
        {
            var nombreArchivo = CrearNombreArchivo(
                "E",
                configuracion.CodigoInstDeduc,
                fechaServidor,
                "txt");

            var rutaArchivo = Path.Combine(rutaDirectorio, nombreArchivo);

            var movimientos = ObtenerMovimientos(configuracion);

            var registros = ObtenerRegistrosEnvio(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                movimientos);

            var contenido = CrearContenidoEnvio(
                registros,
                configuracion.PorcAhorro);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return rutaArchivo;
        }

        private static string GenerarArchivoMatricula(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF02ConfigDbModel configuracion,
            string rutaDirectorio,
            DateTime fechaServidor)
        {
            var nombreArchivo = CrearNombreArchivo("MD",configuracion.CodigoInstDeduc, fechaServidor, "csv");

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(rutaDirectorio, nombreArchivo);

            var cadenas = ObtenerCadenasDesdeSp(
                connection,
                "spPrm_Formato_Integra_New_Matricula",
                request.CodInstitucion,
                request.FechaProceso);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                CrearContenidoCadenas(cadenas),
                Encoding.UTF8);

            return rutaArchivo;
        }

        private static string GenerarArchivoIntegra(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF02ConfigDbModel configuracion,
            string rutaDirectorio,
            DateTime fechaServidor)
        {
            var nombreArchivo = CrearNombreArchivo(
                "CD",
                configuracion.CodigoInstDeduc,
                fechaServidor,
                "csv");

            var rutaArchivo = Path.Combine(rutaDirectorio, nombreArchivo);

            var cadenas = ObtenerCadenasDesdeSp(
                connection,
                "spPrm_Formato_Integra_New",
                request.CodInstitucion,
                request.FechaProceso);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                CrearContenidoCadenas(cadenas),
                Encoding.UTF8);

            return rutaArchivo;
        }

        private static List<CcProcesoMensualArchivoF02RegistroDbModel> ObtenerRegistrosEnvio(
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
                    FechaProceso = fechaProceso,
                    Movimientos = movimientos,
                    CodInstitucion = codInstitucion
                })];
        }

        private static List<string> ObtenerCadenasDesdeSp(
            IDbConnection connection,
            string procedimiento,
            int codInstitucion,
            decimal fechaProceso)
        {
            var query = $@"
                EXEC {procedimiento}
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
            var campos = new List<string>
            {
                FormatearCedula(registro.Cedula),
                registro.CodDeduccion.Trim(),
                ObtenerMontoPorTipo(registro, porcAhorro),
                registro.Sector == 2 ? "2" : "0",
                "0"
            };

            return string.Join("\t", campos);
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

        private static List<string> ObtenerMovimientos(
            CcProcesoMensualArchivoF02ConfigDbModel configuracion)
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

        private static string FormatearCedula(string cedula)
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
             
       private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }
              

        private sealed class CcProcesoMensualArchivoF02ConfigDbModel
        {
            public string Planilla { get; set; } = string.Empty;
            public string CodigoAportesEnv { get; set; } = string.Empty;
            public string CodigoCreditosEnv { get; set; } = string.Empty;
            public decimal PorcAhorro { get; set; } = 0;
            public decimal PorcAporte { get; set; } = 0;
            public string CodigoInstDeduc { get; set; } = string.Empty;
            public int IncInclusiones { get; set; } = 0;
            public int IncExclusiones { get; set; } = 0;
            public int IncModificaciones { get; set; } = 0;
            public int IncMantienen { get; set; } = 0;
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
