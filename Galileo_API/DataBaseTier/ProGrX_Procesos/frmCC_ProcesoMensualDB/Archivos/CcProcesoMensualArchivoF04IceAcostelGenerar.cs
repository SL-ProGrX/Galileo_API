
using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF04IceAcostelGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "04";
        private const string ContentTypeText = "text/plain";
        private const string ExtensionTxt = ".txt";
        private const string TipoCredito = "C";
        private const string MovimientoExclusion = "E";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            ActualizarExclusionesPorMontoMinimo(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var registros = ObtenerRegistros(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);

            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                fechaServidor);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request,DirectorioResultadosBase);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var contenido = CrearContenidoArchivo(registros);

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
                ContentType = ContentTypeText,
                ArchivoBytes = [],
                ArchivosGenerados = [rutaArchivo]
            };
        }

        private static void ActualizarExclusionesPorMontoMinimo(IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            const string query = @"
                UPDATE prm_planilla
                SET monto_actual = 0,
                    movimiento = @MovimientoExclusion
                WHERE Proceso = @FechaProceso
                  AND monto_actual <= 100
                  AND Tipo = @TipoCredito
                  AND cod_institucion = @CodInstitucion";

            connection.Execute(query, new
            {
                FechaProceso = fechaProceso,
                CodInstitucion = codInstitucion,
                TipoCredito,
                MovimientoExclusion
            });
        }

        private static List<CcProcesoMensualArchivoF04RegistroDbModel> ObtenerRegistros(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    P.cedula AS Cedula,
                    P.movimiento AS Movimiento,
                    P.monto_actual AS MontoActual,
                    S.nombre AS Nombre,
                    ISNULL(SUM(R.saldo), 0) AS Saldos
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                LEFT JOIN reg_creditos R
                    ON P.cedula = R.cedula
                   AND R.estado = 'A'
                WHERE P.Proceso = @FechaProceso
                  AND P.Tipo = @TipoCredito
                  AND P.cod_institucion = @CodInstitucion
                GROUP BY
                    P.cedula,
                    P.monto_actual,
                    P.movimiento,
                    S.nombre
                ORDER BY P.cedula";

            return [.. connection.Query<CcProcesoMensualArchivoF04RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    TipoCredito
                })];
        }
 
        private static string CrearNombreArchivo(
            int codInstitucion,
            decimal fechaProceso,
            DateTime fechaServidor)
        {
            var codigoInstitucion = codInstitucion.ToString("00", CultureInfo.InvariantCulture);
            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F04]{ExtensionTxt}";
        }

        private static string CrearContenidoArchivo(  IEnumerable<CcProcesoMensualArchivoF04RegistroDbModel> registros)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaArchivo(registro));
            }

            return builder.ToString();
        }

        private static string CrearLineaArchivo(
            CcProcesoMensualArchivoF04RegistroDbModel registro)
        {
            var cedula = FormatearCedula(registro.Cedula);

            if (string.Equals(  registro.Movimiento?.Trim(),  MovimientoExclusion, StringComparison.OrdinalIgnoreCase))
            {
                return cedula
                    + FormatearNumero(0, 11)
                    + FormatearNumero(0, 10);
            }

            return cedula
                + FormatearNumero(Convert.ToInt64(registro.Saldos) * 100, 11)
                + FormatearNumero(Convert.ToInt64(registro.MontoActual) * 100, 10);
        }

        private static string FormatearCedula(string cedula)
        {
            var texto = cedula?.Trim() ?? string.Empty;

            return long.TryParse(texto,NumberStyles.Integer,CultureInfo.InvariantCulture,out var numero)
            ? numero.ToString("000000000",CultureInfo.InvariantCulture)
            : texto;
        }

        private static string FormatearNumero(long valor, int longitud)
        {
            return valor.ToString(new string('0', longitud), CultureInfo.InvariantCulture);
        }

        private sealed class CcProcesoMensualArchivoF04RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Nombre { get; set; } = string.Empty;
            public decimal Saldos { get; set; } = 0;
        }

    }
}
