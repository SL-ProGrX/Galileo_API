using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF32DxCGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "32";
        private const string ContentTypeCsv = "text/csv";
        private const string ExtensionCsv = ".csv";
        private const string CodigoDeduccionCantidad = "DE31";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(
                connection,
                request.CodInstitucion);

            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);

            var nombreArchivoCsv = CrearNombreArchivoCsv(
                request.CodInstitucion,
                request.FechaProceso,
                configuracion.CodigoInstDeduc,
                fechaServidor);

            var nombreArchivoExcel = CrearNombreArchivoExcel(
                request.CodInstitucion,
                request.FechaProceso,
                configuracion.CodigoInstDeduc,
                fechaServidor);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var rutaArchivoCsv = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivoCsv);

            var rutaArchivoExcel = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivoExcel);

            var movimientos = ObtenerMovimientos(configuracion);

            var registros = ObtenerRegistros(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                movimientos);

            var lineas = CrearLineasArchivo(registros);

            var contenidoCsv = CrearContenidoCsv(lineas);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivoCsv,
                contenidoCsv,
                Encoding.GetEncoding(1252));


            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = true,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = nombreArchivoCsv,
                RutaArchivo = rutaArchivoCsv,
                ContentType = ContentTypeCsv,
                ArchivoBytes = [],
                ArchivosGenerados =
                [
                    rutaArchivoCsv,
                    rutaArchivoExcel
                ]
            };
        }

      
        private static List<string> ObtenerMovimientos(
            CcProcesoMensualArchivoConfiguracionModel configuracion)
        {
            if (configuracion.ComparaIndicador != 1)
            {
                return ["I", "E", "M", "C", "P"] ;
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

        private static List<CcProcesoMensualArchivoF32RegistroDbModel> ObtenerRegistros(
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

            return [.. connection.Query<CcProcesoMensualArchivoF32RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    Movimientos = movimientos,
                    CodInstitucion = codInstitucion
                })];
        }

        private static List<CcProcesoMensualArchivoF32LineaModel> CrearLineasArchivo(
            IEnumerable<CcProcesoMensualArchivoF32RegistroDbModel> registros)
        {
            var lineas = new List<CcProcesoMensualArchivoF32LineaModel>();

            foreach (var registro in registros)
            {
                lineas.Add(CrearLineaModel(registro));
            }

            return lineas;
        }

        private static CcProcesoMensualArchivoF32LineaModel CrearLineaModel(
            CcProcesoMensualArchivoF32RegistroDbModel registro)
        {
            if (string.Equals(
                registro.CodDeduccion?.Trim(),
                CodigoDeduccionCantidad,
                StringComparison.OrdinalIgnoreCase))
            {
                return new CcProcesoMensualArchivoF32LineaModel
                {
                    Empleado = (registro.CedulaColilla ?? string.Empty).Trim(),
                    Concepto = registro.CodDeduccion,
                    Cantidad = FormatearDecimalVb6(registro.MontoActual),
                    Monto = "0"
                };
            }

            return new CcProcesoMensualArchivoF32LineaModel
            {
                Empleado = (registro.CedulaColilla ?? string.Empty).Trim(),
                Concepto = registro.CodDeduccion,
                Cantidad = "0",
                Monto = FormatearDecimalVb6(registro.MontoActual)
            };
        }

        private static string CrearContenidoCsv(
            IEnumerable<CcProcesoMensualArchivoF32LineaModel> lineas)
        {
            var builder = new StringBuilder();

            // VB6: vCadena = "empleado;concepto;cantidad;monto"
            builder.AppendLine("empleado;concepto;cantidad;monto");

            foreach (var linea in lineas)
            {
                builder.AppendLine(
                    linea.Empleado
                    + ";"
                    + linea.Concepto
                    + ";"
                    + linea.Cantidad
                    + ";"
                    + linea.Monto);
            }

            return builder.ToString();
        }

        private static string FormatearDecimalVb6(decimal valor)
        {
            return valor.ToString(CultureInfo.InvariantCulture);
        }

        private static string CrearNombreArchivoCsv(
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

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F32]{ExtensionCsv}";
        }

        private static string CrearNombreArchivoExcel(
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

            // VB6 no agrega extensión aquí; sbSIFGridExportar probablemente la agrega internamente.
            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F32]";
        }

       
        private sealed class CcProcesoMensualArchivoF32RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string CedulaColilla { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public DateTime? Inicio { get; set; }
            public DateTime? Corte { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF32LineaModel
        {
            public string Empleado { get; set; } = string.Empty;
            public string Concepto { get; set; } = string.Empty;
            public string Cantidad { get; set; } = string.Empty;
            public string Monto { get; set; } = string.Empty;
        }
    }
}
