using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF15PjGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "15";
        private const string ContentTypeText = "text/plain";
        private const string ExtensionTxt = ".txt";
        private const string TipoAhorro = "A";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion);

            var fechaServidor = ObtenerFechaServidor(connection);

            var nombreArchivo = CrearNombreArchivo(
                configuracion.CodigoInstDeduc,
                fechaServidor);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var movimientos = ObtenerMovimientos(configuracion);

            var registros = ObtenerRegistros(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                movimientos);

            var contenido = CrearContenidoArchivo(registros);

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

        private static CcProcesoMensualArchivoF15ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion)
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
                    ISNULL(porc_aporte, 0) AS PorcAporte
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF15ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF15ConfigDbModel();
        }

        private static List<string> ObtenerMovimientos(
            CcProcesoMensualArchivoF15ConfigDbModel configuracion)
        {
            var movimientos = new List<string>();

            AgregarMovimientoSiAplica(movimientos, configuracion.IncInclusiones, "I");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncExclusiones, "E");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncModificaciones, "C");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncMantienen, "M");

            movimientos.Add("P");

            return movimientos;
        }

        private static void AgregarMovimientoSiAplica( List<string> movimientos, int indicador, string movimiento)
        {
            if (indicador == 1)
            {
                movimientos.Add(movimiento);
            }
        }

        private static List<CcProcesoMensualArchivoF15RegistroDbModel> ObtenerRegistros(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso,
            IEnumerable<string> movimientos)
        {
            const string query = @"
                SELECT
                    P.Cedula,
                    P.Tipo,
                    P.Cod_Deduccion AS CodDeduccion,
                    P.Monto_Actual AS MontoActual,
                    P.Movimiento,
                    ISNULL(S.cod_sector, 0) AS Sector,
                    S.nombre AS Nombre
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.movimiento IN @Movimientos
                  AND P.cod_institucion = @CodInstitucion
                ORDER BY P.cedula, P.tipo, P.movimiento";

            return [.. connection.Query<CcProcesoMensualArchivoF15RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    Movimientos = movimientos,
                    CodInstitucion = codInstitucion
                })];
        }

        private static string CrearContenidoArchivo(
            IEnumerable<CcProcesoMensualArchivoF15RegistroDbModel> registros)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaArchivo(registro));
            }

            return builder.ToString();
        }

        private static string CrearLineaArchivo(
            CcProcesoMensualArchivoF15RegistroDbModel registro)
        {
            return FormatearCedula(registro.Cedula)
                + "\t"
                + registro.CodDeduccion.Trim()
                + "\t"
                + FormatearMontoPorTipo(registro)
                + "\t"
                + ObtenerSectorArchivo(registro.Sector);
        }

        private static string FormatearCedula(string cedula)
        {
            var texto = cedula?.Trim() ?? string.Empty;

            return decimal.TryParse(
                texto,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var numero)
                    ? numero.ToString("0000000000", CultureInfo.InvariantCulture)
                    : texto;
        }

        private static string FormatearMontoPorTipo(
            CcProcesoMensualArchivoF15RegistroDbModel registro)
        {
            if (string.Equals(
                registro.Tipo?.Trim(),
                TipoAhorro,
                StringComparison.OrdinalIgnoreCase))
            {
                return registro.MontoActual.ToString("######0.00", CultureInfo.InvariantCulture);
            }

            return registro.MontoActual.ToString("############0.00", CultureInfo.InvariantCulture);
        }

        private static string ObtenerSectorArchivo(int sector)
        {
            return sector == 2 ? "1" : "0";
        }

        private static string CrearNombreArchivo(
            string codigoInstDeduc,
            DateTime fechaServidor)
        {
            var codigo = codigoInstDeduc?.Trim() ?? string.Empty;
            var fechaTexto = fechaServidor.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            return $"E-{codigo}-{fechaTexto}-01{ExtensionTxt}";
        }

        private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }

        private sealed class CcProcesoMensualArchivoF15ConfigDbModel
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
        }

        private sealed class CcProcesoMensualArchivoF15RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public int Sector { get; set; } = 0;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
