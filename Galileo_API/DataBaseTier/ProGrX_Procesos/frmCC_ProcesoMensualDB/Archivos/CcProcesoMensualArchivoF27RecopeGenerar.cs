using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF27RecopeGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "27";
        private const string ContentTypeText = "text/plain";
        private const string ExtensionTxt = ".txt";
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

            var fechaArchivo = ObtenerFechaArchivo(request.FechaProceso);

            var contenido = CrearContenidoArchivo(
                registros,
                fechaArchivo);

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

        private static List<CcProcesoMensualArchivoF27RegistroDbModel> ObtenerRegistros(
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
                    P.Porc_Deduc AS PorcDeduc,
                    P.Tipo_Deduc AS TipoDeduc,
                    P.Movimiento,
                    S.CedulaR AS CedulaColilla,
                    S.Nombre
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.movimiento IN @Movimientos
                  AND P.cod_institucion = @CodInstitucion
                ORDER BY P.tipo, P.movimiento, P.cedula";

            return [.. connection.Query<CcProcesoMensualArchivoF27RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    Movimientos = movimientos,
                    CodInstitucion = codInstitucion
                })];
        }

        private static string CrearContenidoArchivo(
            IEnumerable<CcProcesoMensualArchivoF27RegistroDbModel> registros,
            DateTime fechaArchivo)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaArchivo(registro, fechaArchivo));
            }

            return builder.ToString();
        }

        private static string CrearLineaArchivo(
            CcProcesoMensualArchivoF27RegistroDbModel registro,
            DateTime fechaArchivo)
        {
            var cedula = FormatearCedula(registro.Cedula);
            var fechaTexto = fechaArchivo.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

            if (string.Equals(
                registro.TipoDeduc?.Trim(),
                TipoDeduccionMonto,
                StringComparison.OrdinalIgnoreCase))
            {
                return cedula
                    + "\t"
                    + fechaTexto
                    + "\t"
                    + registro.CodDeduccion
                    + "\t"
                    + FormatearDecimalVb6(registro.MontoActual);
            }

            return cedula
                + "\t"
                + fechaTexto
                + "\t"
                + registro.CodDeduccion
                + "\t"
                + "\t"
                + FormatearDecimalVb6(registro.MontoActual);
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

        private static DateTime ObtenerFechaArchivo(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            var anio = int.Parse(fechaBase[..4], CultureInfo.InvariantCulture);
            var mes = int.Parse(fechaBase.AsSpan(4, 2), CultureInfo.InvariantCulture);

            return new DateTime(anio, mes, 14, 0, 0, 0, DateTimeKind.Unspecified);
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

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F27]{ExtensionTxt}";
        }

   

        private sealed class CcProcesoMensualArchivoF27RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public decimal PorcDeduc { get; set; } = 0;
            public string TipoDeduc { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public string CedulaColilla { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
