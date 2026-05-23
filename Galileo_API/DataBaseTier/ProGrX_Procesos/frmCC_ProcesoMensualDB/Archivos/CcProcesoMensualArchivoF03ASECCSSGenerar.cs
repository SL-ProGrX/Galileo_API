using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF03AseccssGenerar : ICcProcesoMensualArchivoGenerator
    { 
        public const string TipoAporte = "A";
        public const string TipoCredito = "C";

        public const string MovimientoExclusion = "E";
        public const string MovimientoInclusion = "I";
        public const string MovimientoCambio = "C";
        public const string MovimientoMantiene = "M"; 

        private const string CodigoPlanillaEnvio = "03_A";
        private const string ContentTypeText = "text/plain";
        private const string ExtensionTxt = ".txt";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion( connection,  request.CodInstitucion);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);
            var fechaServidor = ObtenerFechaServidor(connection);
            var archivosGenerados = new List<string>();

                var rutaArchivo = GenerarPorUnidad(
                    connection,
                    request,
                    configuracion,
                    rutaDirectorio,
                    fechaServidor,
                    request.Unidad);

                archivosGenerados.Add(rutaArchivo);
        

            var ultimoArchivo = archivosGenerados.LastOrDefault() ?? string.Empty;

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

        private static string GenerarPorUnidad(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF03ConfigDbModel configuracion,
            string rutaDirectorio,
            DateTime fechaServidor,
            string unidad)
        {
            var nombreArchivo = CrearNombreArchivo(
                request.FechaProceso,
                fechaServidor,
                request.CodInstitucion,
                unidad);

            var rutaArchivo = Path.Combine(rutaDirectorio, nombreArchivo);

            var registros = ObtenerRegistros(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                unidad);

            var contenido = CrearContenidoArchivo(
                registros,
                configuracion,
                request.FechaProceso);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return rutaArchivo;
        }

        private static CcProcesoMensualArchivoF03ConfigDbModel ObtenerConfiguracion(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(planilla, '') AS Planilla,
                    ISNULL(codigo_aportes, '') AS CodigoAportes,
                    ISNULL(codigo_creditos, '') AS CodigoCreditos,
                    ISNULL(porc_ahorro, 0) AS PorcAhorro
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF03ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF03ConfigDbModel();
        }

        private static List<CcProcesoMensualArchivoF03RegistroDbModel> ObtenerRegistros(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso,
            string unidad)
        {
            const string query = @"
                SELECT
                    P.Cedula,
                    P.Tipo,
                    P.Movimiento,
                    P.Monto_Actual AS MontoActual,
                    S.nombre AS Nombre
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                   AND S.up = @Unidad
                WHERE P.Proceso = @FechaProceso
                  AND P.cod_institucion = @CodInstitucion
                ORDER BY P.cedula, P.tipo, P.movimiento";

            return [.. connection.Query<CcProcesoMensualArchivoF03RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    Unidad = unidad
                })];
        }

        private static string CrearContenidoArchivo(
            IEnumerable<CcProcesoMensualArchivoF03RegistroDbModel> registros,
            CcProcesoMensualArchivoF03ConfigDbModel configuracion,
            decimal fechaProceso)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                var linea = CrearLinea(registro, configuracion, fechaProceso);

                if (!string.IsNullOrEmpty(linea))
                {
                    builder.AppendLine(linea);
                }
            }

            builder.AppendLine("!");

            return builder.ToString();
        }

        private static string CrearLinea(
            CcProcesoMensualArchivoF03RegistroDbModel registro,
            CcProcesoMensualArchivoF03ConfigDbModel configuracion,
            decimal fechaProceso)
        {
            var tipoMovimiento = ObtenerTipoMovimiento(registro.Movimiento);
            var monto = FormatearMonto(registro.MontoActual);
            var fechaTexto = FormatearFecha(fechaProceso);
            var procesoTexto = fechaProceso.ToString(CultureInfo.InvariantCulture);

            return registro.Tipo?.Trim().ToUpperInvariant() switch
            {
               TipoAporte =>
                    "",

               TipoCredito =>
                    CrearLineaCredito(registro, configuracion, tipoMovimiento, monto, fechaTexto, procesoTexto),

                _ => string.Empty
            };
        }
 

        private static string CrearLineaCredito(
            CcProcesoMensualArchivoF03RegistroDbModel registro,
            CcProcesoMensualArchivoF03ConfigDbModel configuracion,
            string tipoMovimiento,
            string monto,
            string fechaTexto,
            string procesoTexto)
        {
            return Helpers.CcProcesoMensualArchivoRutaHelperDb.RellenarEspaciosDerecha(
                    FormatearCedula(registro.Cedula),
                    15)
                + configuracion.CodigoCreditos.Trim()
                + " "
                + tipoMovimiento
                + " "
                + monto
                + " 0000000000 0000000000 "
                + fechaTexto
                + new string(' ', 12)
                + "00.00 "
                + procesoTexto
                + "1";
        }

        private static string ObtenerTipoMovimiento(string movimiento)
        {
            return movimiento?.Trim().ToUpperInvariant() switch
            {
               MovimientoExclusion =>
                   "B",
              MovimientoInclusion =>
                   "F",
                MovimientoCambio =>
                   "F",
                _ => "F"
            };
        }

        private static string FormatearMonto(decimal monto)
        {
            var texto = monto.ToString("00000000.00", CultureInfo.InvariantCulture);
            return texto.Replace(".", string.Empty);
        }

        private static string FormatearFecha(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            if (fechaBase.Length < 6)
            {
                return string.Empty;
            }

            var mes = fechaBase.Substring(4, 2);
            var anio = fechaBase[..4];

            return $"01/{mes}/{anio}";
        }

        private static string FormatearCedula(string cedula)
        {
            var texto = cedula?.Trim() ?? string.Empty;

            if (long.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numero))
            {
                texto = numero.ToString("0000000000", CultureInfo.InvariantCulture);
            }

            return texto;
        }

       
        private static string CrearNombreArchivo(
            decimal fechaProceso,
            DateTime fechaServidor,
            int codInstitucion,
            string unidad)
        {
            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var institucionTexto = codInstitucion.ToString("00", CultureInfo.InvariantCulture);

            return $"F03-[{fechaProcesoTexto}] {fechaServidorTexto}-{institucionTexto}u{unidad}{ExtensionTxt}";
        }

       
        private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }

      

        private sealed class CcProcesoMensualArchivoF03ConfigDbModel
        {
            public string Planilla { get; set; } = string.Empty;
            public string CodigoAportes { get; set; } = string.Empty;
            public string CodigoCreditos { get; set; } = string.Empty;
            public decimal PorcAhorro { get; set; } = 0;
        }

        private sealed class CcProcesoMensualArchivoF03RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Nombre { get; set; } = string.Empty;
        }

    }
}
