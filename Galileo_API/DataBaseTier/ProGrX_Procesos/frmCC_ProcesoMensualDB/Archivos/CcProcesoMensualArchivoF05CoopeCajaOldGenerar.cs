
using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF05CoopeCajaOldGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "05_OLD";
        private const string ContentTypeText = "text/plain";
        private const string CodigoNo = "NO";
        private const string TipoAporte = "A";
        private const string TipoCredito = "C";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(connection, request.CodInstitucion);
            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var archivosGenerados = new List<string>();
            var ultimoArchivo = string.Empty;

            if (!EsCodigoNo(configuracion.CodigoAportes))
            {
                ultimoArchivo = GenerarArchivoAportes(
                    connection,
                    request,
                    configuracion,
                    rutaDirectorio);

                archivosGenerados.Add(ultimoArchivo);
            }

            if (!EsCodigoNo(configuracion.CodigoCreditos))
            {
                ultimoArchivo = GenerarArchivoCreditos(
                    connection,
                    request,
                    configuracion,
                    rutaDirectorio);

                archivosGenerados.Add(ultimoArchivo);
            }

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

        private static string GenerarArchivoAportes(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion,
            string rutaDirectorio)
        {
            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                "A");

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var registros = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRegistrosGeneral(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                TipoAporte);

            var contenido = CrearContenidoAportes(registros, configuracion);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return rutaArchivo;
        }

        private static string GenerarArchivoCreditos(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion,
            string rutaDirectorio)
        {
            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                "C");

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var registros = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRegistrosGeneral(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                TipoCredito);

            var contenido = CrearContenidoCreditos(registros, configuracion);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return rutaArchivo;
        }

 

        private static string CrearContenidoAportes(
            IEnumerable<CcProcesoMensualArchivoRegistroDbModel> registros,
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaAporte(registro, configuracion));
            }

            return builder.ToString();
        }

        private static string CrearContenidoCreditos(
            IEnumerable<CcProcesoMensualArchivoRegistroDbModel> registros,
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                var tipoMovimiento = ObtenerTipoMovimientoCoopeCaja(registro.Movimiento);

                // VB6:
                // If i <> 1 Then Print #fnFile, vLinea
                if (tipoMovimiento != 1)
                {
                    builder.AppendLine(CrearLineaCredito(registro, configuracion));
                }
            }

            return builder.ToString();
        }

        private static string CrearLineaAporte(
            CcProcesoMensualArchivoRegistroDbModel registro,
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion)
        {
            var nombre = Helpers.CcProcesoMensualArchivoRutaHelperDb.SepararNombre(registro.Nombre);

            return TomarIzquierda(configuracion.CodigoAportes, 6)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(registro.Cedula, "I", "0", 15)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Apellido1, "D", " ", 15)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Apellido2, "D", " ", 15)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Nombre1, "D", " ", 15)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Nombre2, "D", " ", 15)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                     Helpers.CcProcesoMensualArchivoRutaHelperDb.DepurarCadena(registro.Direccion),
                    "D",
                    " ",
                    140)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "I", "0", 7)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "I", "0", 7)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "I", "0", 11);
        }

        private static string CrearLineaCredito(
            CcProcesoMensualArchivoRegistroDbModel registro,
            CcProcesoMensualArchivoF05OldConfigDbModel configuracion)
        {
            var nombre = Helpers.CcProcesoMensualArchivoRutaHelperDb.SepararNombre(registro.Nombre);
            var monto = Convert.ToInt64(registro.MontoActual * 100).ToString(CultureInfo.InvariantCulture);

            return TomarIzquierda(configuracion.CodigoCreditos, 6)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(registro.Cedula, "I", "0", 15)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Apellido1, "D", " ", 15)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Apellido2, "D", " ", 15)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Nombre1, "D", " ", 15)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Nombre2, "D", " ", 15)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                     Helpers.CcProcesoMensualArchivoRutaHelperDb.DepurarCadena(registro.Direccion),
                    "D",
                    " ",
                    140)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "I", "0", 7)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "I", "0", 7)
                +  Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(monto, "I", "0", 11);
        }



        private static int ObtenerTipoMovimientoCoopeCaja(string movimiento)
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

        private static bool EsCodigoNo(string codigo)
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
