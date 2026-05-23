
using Dapper;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Helpers;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF16StarHGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "16";
        private const string ContentTypeText = "text/plain";
        private const string ExtensionTxt = ".txt";

        private const string TipoAporte = "A";
        private const string TipoCredito = "C";

        private const string MovimientoExclusion = "E";
        private const string MovimientoInclusion = "I";
        private const string MovimientoCambio = "C";
        private const string MovimientoMantiene = "M";

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

            var registros = ObtenerRegistros(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var contenido = CrearContenidoArchivo(
                registros,
                configuracion,
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

        private static CcProcesoMensualArchivoF16ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(planilla, '') AS Planilla,
                    ISNULL(codigo_aportes_env, '') AS CodigoAportesEnv,
                    ISNULL(codigo_creditos_env, '') AS CodigoCreditosEnv,
                    ISNULL(porc_ahorro, 0) AS PorcAhorro
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF16ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF16ConfigDbModel();
        }

        private static List<CcProcesoMensualArchivoF16RegistroDbModel> ObtenerRegistros(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
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
                WHERE P.Proceso = @FechaProceso
                  AND P.cod_institucion = @CodInstitucion
                ORDER BY P.cedula, P.tipo, P.movimiento";

            return [.. connection.Query<CcProcesoMensualArchivoF16RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion
                })];
        }

        private static string CrearContenidoArchivo(
            IEnumerable<CcProcesoMensualArchivoF16RegistroDbModel> registros,
            CcProcesoMensualArchivoF16ConfigDbModel configuracion,
            decimal fechaProceso)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                var linea = CrearLineaArchivo(
                    registro,
                    configuracion,
                    fechaProceso);

                if (!string.IsNullOrEmpty(linea))
                {
                    builder.AppendLine(linea);
                }
            }

            // VB6: Print #fnFile, "!"
            builder.AppendLine("!");

            return builder.ToString();
        }

        private static string CrearLineaArchivo(
            CcProcesoMensualArchivoF16RegistroDbModel registro,
            CcProcesoMensualArchivoF16ConfigDbModel configuracion,
            decimal fechaProceso)
        {
            var tipo = registro.Tipo?.Trim().ToUpperInvariant();

            return tipo switch
            {
                TipoAporte => CrearLineaAporte(registro, configuracion, fechaProceso),
                TipoCredito => CrearLineaCredito(registro, configuracion, fechaProceso),
                _ => string.Empty
            };
        }

        private static string CrearLineaAporte(
            CcProcesoMensualArchivoF16RegistroDbModel registro,
            CcProcesoMensualArchivoF16ConfigDbModel configuracion,
            decimal fechaProceso)
        {
            if (string.Equals(
                registro.Movimiento?.Trim(),
                MovimientoMantiene,
                StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return CrearLineaBase(
                    registro,
                    configuracion.CodigoAportesEnv,
                    ObtenerTipoMovimiento(registro.Movimiento ?? ""),
                    FormatearMontoSinPunto(registro.MontoActual),
                    fechaProceso)
                + new string(' ', 12)
                + configuracion.PorcAhorro.ToString("00.00", CultureInfo.InvariantCulture)
                + " "
                + fechaProceso.ToString(CultureInfo.InvariantCulture)
                + "1";
        }

        private static string CrearLineaCredito(
            CcProcesoMensualArchivoF16RegistroDbModel registro,
            CcProcesoMensualArchivoF16ConfigDbModel configuracion,
            decimal fechaProceso)
        {
            return CrearLineaBase(
                    registro,
                    configuracion.CodigoCreditosEnv,
                    ObtenerTipoMovimiento(registro.Movimiento ?? ""),
                    FormatearMontoSinPunto(registro.MontoActual),
                    fechaProceso)
                + new string(' ', 12)
                + "00.00 "
                + fechaProceso.ToString(CultureInfo.InvariantCulture)
                + "1";
        }

        private static string CrearLineaBase(
            CcProcesoMensualArchivoF16RegistroDbModel registro,
            string codigoTipo,
            string tipoMovimiento,
            string monto,
            decimal fechaProceso)
        {
            return Helpers.CcProcesoMensualArchivoRutaHelperDb.RellenarEspaciosDerecha( FormatearCedula(registro.Cedula), 15)
                + codigoTipo.Trim()
                + " "
                + tipoMovimiento
                + " "
                + monto
                + " 0000000000 0000000000 01/"
                + ObtenerMesProceso(fechaProceso)
                + "/"
                + ObtenerAnioProceso(fechaProceso);
        }

        private static string ObtenerTipoMovimiento(string movimiento)
        {
            return movimiento?.Trim().ToUpperInvariant() switch
            {
                MovimientoExclusion => "B",
                MovimientoInclusion => "F",
                MovimientoCambio => "F",
                _ => "F"
            };
        }

        private static string FormatearMontoSinPunto(decimal monto)
        {
            var montoTexto = monto.ToString("00000000.00", CultureInfo.InvariantCulture);

            return montoTexto.Replace(".", string.Empty);
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

        private static string CrearNombreArchivo(
            int codInstitucion,
            decimal fechaProceso,
            DateTime fechaServidor)
        {
            var codigoInstitucion = codInstitucion.ToString("00", CultureInfo.InvariantCulture);
            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F16]{ExtensionTxt}";
        }

        private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }

        private sealed class CcProcesoMensualArchivoF16ConfigDbModel
        {
            public string Planilla { get; set; } = string.Empty;
            public string CodigoAportesEnv { get; set; } = string.Empty;
            public string CodigoCreditosEnv { get; set; } = string.Empty;
            public decimal PorcAhorro { get; set; } = 0;
        }

        private sealed class CcProcesoMensualArchivoF16RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
