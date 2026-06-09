
using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF05CoopeCajaOldGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "05_OLD";
        private const string ContentTypeText = "text/plain";
        private const string CodigoNo = "NO";
        private const string TipoAporte = "A";
        private const string TipoCredito = "C";
        private readonly ArchivosGeneradosOptions _archivosOptions;
        public CcProcesoMensualArchivoF05CoopeCajaOldGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions)
        {
            _archivosOptions = archivosOptions.Value;
        }


        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];


        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion);

            var rutaBase = _archivosOptions.RutaBase;

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request, rutaBase);

            var archivosGenerados = new List<string>();

            AgregarArchivoSiAplica(
                archivosGenerados,
                connection,
                request,
                configuracion,
                 rutaBase,
                rutaDirectorio,
                TipoAporte,
                configuracion.CodigoAportes);

            AgregarArchivoSiAplica(
                archivosGenerados,
                connection,
                request,
                configuracion,
                 rutaBase,
                rutaDirectorio,
                TipoCredito,
                configuracion.CodigoCreditos);

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

        private static void AgregarArchivoSiAplica(
            List<string> archivosGenerados,
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion,
             string rutaBase,
            string rutaDirectorio,
            string tipo,
            string codigoConfigurado)
        {
            if (EsCodigoNo(codigoConfigurado))
            {
                return;
            }

            var rutaArchivo = GenerarArchivoPorTipo(
                connection,
                request,
                configuracion,
                rutaBase,
                rutaDirectorio,
                tipo);

            archivosGenerados.Add(rutaArchivo);
        }

        private static CcProcesoMensualArchivoF05OldConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_aportes, '') AS CodigoAportes,
                    ISNULL(codigo_creditos, '') AS CodigoCreditos
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF05OldConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF05OldConfigDbModel();
        }

        private static string GenerarArchivoPorTipo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion,
            string rutaBase,
            string rutaDirectorio,
            string tipo)
        {
            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                tipo);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaBase,
                rutaDirectorio,
                nombreArchivo);

            var registros = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRegistrosGeneral(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                tipo);

            var contenido = CrearContenidoArchivo(
                registros,
                configuracion,
                tipo);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaBase,
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return rutaArchivo;
        }

        private static string CrearContenidoArchivo(
            IEnumerable<CcProcesoMensualArchivoRegistroDbModel> registros,
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion,
            string tipo)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros.Where(registro => DebeImprimirRegistro(registro, tipo)))
            {
                builder.AppendLine(
                    CrearLineaArchivo(
                        registro,
                        configuracion,
                        tipo));
            }

            return builder.ToString();
        }

        private static bool DebeImprimirRegistro(
            CcProcesoMensualArchivoRegistroDbModel registro,
            string tipo)
        {
            if (!string.Equals(tipo, TipoCredito, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return ObtenerTipoMovimientoCoopeCaja(registro.Movimiento) != 1;
        }

        private static string CrearLineaArchivo(
            CcProcesoMensualArchivoRegistroDbModel registro,
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion,
            string tipo)
        {
            var codigo = ObtenerCodigoArchivo(configuracion, tipo);
            var monto = ObtenerMontoArchivo(registro, tipo);
            var nombre = Helpers.CcProcesoMensualArchivoRutaHelperDb.SepararNombre(registro.Nombre);

            return TomarIzquierda(codigo, 6)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(registro.Cedula, "I", "0", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Apellido1, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Apellido2, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Nombre1, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Nombre2, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    Helpers.CcProcesoMensualArchivoRutaHelperDb.DepurarCadena(registro.Direccion),
                    "D",
                    " ",
                    140)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "I", "0", 7)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "I", "0", 7)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(monto, "I", "0", 11);
        }

        private static string ObtenerCodigoArchivo(
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion,
            string tipo)
        {
            return string.Equals(tipo, TipoAporte, StringComparison.OrdinalIgnoreCase)
                ? configuracion.CodigoAportes
                : configuracion.CodigoCreditos;
        }

        private static string ObtenerMontoArchivo(
            CcProcesoMensualArchivoRegistroDbModel registro,
            string tipo)
        {
            if (!string.Equals(tipo, TipoCredito, StringComparison.OrdinalIgnoreCase))
            {
                return "0";
            }

            return Convert.ToInt64(registro.MontoActual * 100)
                .ToString(CultureInfo.InvariantCulture);
        }

        private static int ObtenerTipoMovimientoCoopeCaja(string? movimiento)
        {
            return movimiento?.Trim().ToUpperInvariant() switch
            {
                "E" => 1,
                "I" => 2,
                "C" => 3,
                _ => 4
            };
        }

        private static string CrearNombreArchivo(
            int codInstitucion,
            decimal fechaProceso,
            string tipo)
        {
            var institucion = codInstitucion.ToString("00", CultureInfo.InvariantCulture);
            var fecha = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);

            return $"{institucion}-{fecha}-ARC-COOPECAJA-{tipo}_OLD.txt";
        }

        private static string TomarIzquierda(string? valor, int cantidad)
        {
            var texto = valor ?? string.Empty;

            return texto.Length > cantidad
                ? texto[..cantidad]
                : texto;
        }

        private static bool EsCodigoNo(string? codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                CodigoNo,
                StringComparison.OrdinalIgnoreCase);
        }

        private sealed class CcProcesoMensualArchivoF05OldConfigDbModel
        {
            public string CodigoAportes { get; set; } = string.Empty;
            public string CodigoCreditos { get; set; } = string.Empty;
        }

    }
}
