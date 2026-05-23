using System.Data;
using System.Globalization;
using System.Text;
using Dapper; 
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF00ExcelGenerar : ICcProcesoMensualArchivoGenerator
    {

        private const string CodigoPlanillaEnvio = "00";
        private const string ContentTypeCsv = "text/csv";
        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } =  [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo( IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion);

            var movimientos = ObtenerMovimientos(configuracion);

            var registros = ObtenerRegistrosPlanilla(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                movimientos);

            var fechaServidor = ObtenerFechaServidor(connection);

            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                configuracion.CodigoInstDeduc,
                fechaServidor);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);
            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(rutaDirectorio, nombreArchivo);

            var contenido = CrearContenidoCsv(registros); 

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = true,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = nombreArchivo,
                RutaArchivo = rutaArchivo,
                ContentType = ContentTypeCsv,
                ArchivoBytes = Encoding.UTF8.GetBytes(contenido),
                ArchivosGenerados = [rutaArchivo]
            };
        }

        
        private static CcProcesoMensualArchivoF00ConfigDbModel ObtenerConfiguracion(
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
                    ISNULL(porc_aporte, 0) AS PorcAporte,
                    ISNULL(compara_indicador, 0) AS ComparaIndicador
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF00ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF00ConfigDbModel();
        }

        private static List<string> ObtenerMovimientos(
            CcProcesoMensualArchivoF00ConfigDbModel configuracion)
        {
            if (configuracion.ComparaIndicador != 1)
            {
                return ["I", "E", "M", "C", "P"];
            }

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

        private static List<CcProcesoMensualArchivoF00RegistroDbModel> ObtenerRegistrosPlanilla(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso,
            IEnumerable<string> movimientos)
        {
            const string query = @"
                SELECT
                    P.Cedula,
                    S.nombre AS Nombre,
                    P.Tipo,
                    P.Monto_Actual AS MontoActual,
                    P.Movimiento,
                    I.Descripcion AS InstDesc,
                    ISNULL(S.CedulaR, S.cedula) AS IdAlterno
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                INNER JOIN instituciones I
                    ON S.cod_institucion = I.cod_institucion
                WHERE P.Proceso = @FechaProceso
                  AND P.movimiento IN @Movimientos
                  AND P.cod_institucion = @CodInstitucion
                ORDER BY P.cedula, P.tipo, P.movimiento";

            return [.. connection.Query<CcProcesoMensualArchivoF00RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    Movimientos = movimientos,
                    CodInstitucion = codInstitucion
                })];
        }

        private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }

        private static string CrearNombreArchivo(
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

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F00].csv";
        }

        private static string CrearContenidoCsv(
            IEnumerable<CcProcesoMensualArchivoF00RegistroDbModel> registros)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaCsv(registro));
            }

            return builder.ToString();
        }

        private static string CrearLineaCsv(
            CcProcesoMensualArchivoF00RegistroDbModel registro)
        {
            return string.Join(
                ";",
                LimpiarCampo(registro.Cedula),
                LimpiarCampo(registro.Nombre),
                LimpiarCampo(registro.Tipo),
                registro.MontoActual.ToString(CultureInfo.InvariantCulture),
                LimpiarCampo(registro.Movimiento),
                LimpiarCampo(registro.InstDesc),
                LimpiarCampo(registro.IdAlterno));
        }

        private static string LimpiarCampo(string? valor)
        {
            return (valor ?? string.Empty).Trim().Replace(";", " ");
        }

      
        private sealed class CcProcesoMensualArchivoF00ConfigDbModel
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
            public int ComparaIndicador { get; set; } = 0;
        }

        private sealed class CcProcesoMensualArchivoF00RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string InstDesc { get; set; } = string.Empty;
            public string IdAlterno { get; set; } = string.Empty;
        }

    }
}
