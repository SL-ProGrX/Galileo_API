using Dapper;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Helpers;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF29PygGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "29";
        private const string ContentTypeCsv = "text/csv";
        private const string ExtensionCsv = ".csv";
        private const string TipoDeduccionMonto = "M";
        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(
                connection,
                request.CodInstitucion);

            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);

            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
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

            var contenido = CrearContenidoArchivo(
                registros,
                configuracion.CodigoInstDeduc);

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
                ContentType = ContentTypeCsv,
                ArchivoBytes = [],
                ArchivosGenerados = [rutaArchivo]
            };
        }

      

        private static List<string> ObtenerMovimientos(
            CcProcesoMensualArchivoConfiguracionModel configuracion)
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

        private static List<CcProcesoMensualArchivoF29RegistroDbModel> ObtenerRegistros(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso,
            IEnumerable<string> movimientos)
        {
            const string query = @"
                SELECT
                    P.Cedula,
                    P.Cod_Deduccion AS CodDeduccion,
                    P.Monto_Actual AS MontoActual,
                    P.Tipo_Deduc AS TipoDeduc,
                    P.Movimiento,
                    S.CedulaR AS CedulaColilla,
                    dbo.fxSIFCorteAFechaInicio(P.Proceso) AS Inicio,
                    dbo.fxSIFCorteAFecha(P.Proceso) AS Corte,
                    S.Nombre
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.movimiento IN @Movimientos
                  AND P.cod_institucion = @CodInstitucion
                ORDER BY P.tipo, P.movimiento, P.cedula";

            return [.. connection.Query<CcProcesoMensualArchivoF29RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    Movimientos = movimientos,
                    CodInstitucion = codInstitucion
                })];
        }

        private static string CrearContenidoArchivo(
            IEnumerable<CcProcesoMensualArchivoF29RegistroDbModel> registros,
            string codigoInstitucion)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros.Where(EsTipoDeduccionMonto))
            {
                builder.AppendLine(CrearLineaArchivo(registro, codigoInstitucion));
            }

            return builder.ToString();
        }

        private static bool EsTipoDeduccionMonto( CcProcesoMensualArchivoF29RegistroDbModel registro)
        {
            return string.Equals(
                registro.TipoDeduc?.Trim(),
                TipoDeduccionMonto,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string CrearLineaArchivo(
            CcProcesoMensualArchivoF29RegistroDbModel registro,
            string codigoInstitucion)
        {
            return "F2;'01;'"
                + codigoInstitucion.Trim()
                + ";"
                + (registro.CedulaColilla ?? string.Empty).Trim()
                + ";;"
                + registro.Corte.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                + ";"
                + registro.Corte.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                + ";"
                + registro.CodDeduccion.Trim()
                + ";"
                + FormatearDecimalVb6(registro.MontoActual)
                + ";CRC;;"
                + registro.Cedula
                + ";"
                + registro.Nombre;
        }

        private static string FormatearDecimalVb6(decimal valor)
        {
            return valor.ToString(CultureInfo.InvariantCulture);
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

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F29]{ExtensionCsv}";
        }

     

        private sealed class CcProcesoMensualArchivoF29RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string CedulaColilla { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string TipoDeduc { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public DateTime Inicio { get; set; } = DateTime.MinValue;
            public DateTime Corte { get; set; } = DateTime.MinValue;
            public string Nombre { get; set; } = string.Empty;
        }

    }
}
