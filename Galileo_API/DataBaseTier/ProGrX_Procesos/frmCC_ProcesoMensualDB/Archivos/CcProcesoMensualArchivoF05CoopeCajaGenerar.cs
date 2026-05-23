
using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF05CoopeCajaGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "05";
        private const string ContentTypeText = "text/plain";
        private const string CodigoNo = "NO";
        private const string TipoAporte = "A";
        private const string TipoCredito = "C";
        private const string MovimientoExclusion = "E";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(connection, request.CodInstitucion);
            var empresa = ObtenerDatosEmpresa(connection);

            var fechaServidor = ObtenerFechaServidor(connection);
            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var archivosGenerados = new List<string>();
            var ultimoArchivo = string.Empty;

            if (!EsCodigoNo(configuracion.CodigoAportes))
            {
                ultimoArchivo = GenerarArchivoAportes(
                    connection,
                    request,
                    configuracion,
                    empresa,
                    fechaServidor,
                    rutaDirectorio);

                archivosGenerados.Add(ultimoArchivo);
            }

            if (!EsCodigoNo(configuracion.CodigoCreditos))
            {
                ultimoArchivo = GenerarArchivoCreditos(
                    connection,
                    request,
                    configuracion,
                    empresa,
                    fechaServidor,
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
                ArchivoBytes =[],
                ArchivosGenerados = archivosGenerados
            };
        }

        private static CcProcesoMensualArchivoF05ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_aportes, '') AS CodigoAportes,
                    ISNULL(codigo_creditos, '') AS CodigoCreditos
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF05ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF05ConfigDbModel();
        }

        private static CcProcesoMensualArchivoF05EmpresaDbModel ObtenerDatosEmpresa(
            IDbConnection connection)
        {
            const string query = @"
                SELECT
                    RTRIM(PAG_NOMLARGO) + ', ' + RTRIM(PAG_DOMICILIO) AS Direccion,
                    REPLACE(TELEFONOEMP, '-', '') AS Telefono
                FROM SIF_EMPRESA";

            var empresa = connection.QueryFirstOrDefault<CcProcesoMensualArchivoF05EmpresaDbModel>(
                query) ?? new CcProcesoMensualArchivoF05EmpresaDbModel();

            empresa.Direccion = empresa.Direccion.ToUpperInvariant();

            return empresa;
        }

        private static string GenerarArchivoAportes(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF05ConfigDbModel configuracion,
            CcProcesoMensualArchivoF05EmpresaDbModel empresa,
            DateTime fechaServidor,
            string rutaDirectorio)
        {
            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                fechaServidor,
                "A");

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var registros = ObtenerRegistrosAportes(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var contenido = CrearContenidoAportes(
                registros,
                configuracion,
                empresa);

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
            CcProcesoMensualArchivoF05ConfigDbModel configuracion,
            CcProcesoMensualArchivoF05EmpresaDbModel empresa,
            DateTime fechaServidor,
            string rutaDirectorio)
        {
            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                fechaServidor,
                "C");

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            var registros = ObtenerRegistrosCreditos(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var contenido = CrearContenidoCreditos(
                registros,
                configuracion,
                empresa);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return rutaArchivo;
        }

        private static List<CcProcesoMensualArchivoF05RegistroDbModel> ObtenerRegistrosAportes(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    P.Cedula,
                    P.Monto_Actual AS MontoActual,
                    P.Movimiento,
                    S.nombre AS Nombre,
                    S.cod_departamento AS CodDepartamento
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.cod_institucion = @CodInstitucion
                  AND P.tipo = @TipoAporte
                ORDER BY P.cedula";

            return [.. connection.Query<CcProcesoMensualArchivoF05RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    TipoAporte
                })];
        }

        private static List<CcProcesoMensualArchivoF05RegistroDbModel> ObtenerRegistrosCreditos(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    P.Cedula,
                    P.Monto_Actual AS MontoActual,
                    P.Movimiento,
                    S.nombre AS Nombre,
                    S.direccion AS Direccion,
                    S.cod_departamento AS CodDepartamento
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.cod_institucion = @CodInstitucion
                  AND P.tipo = @TipoCredito
                ORDER BY P.cedula";

            return [.. connection.Query<CcProcesoMensualArchivoF05RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    TipoCredito
                })];
        }

        private static string CrearContenidoAportes(
            IEnumerable<CcProcesoMensualArchivoF05RegistroDbModel> registros,
            CcProcesoMensualArchivoF05ConfigDbModel configuracion,
            CcProcesoMensualArchivoF05EmpresaDbModel empresa)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaAporte(registro, configuracion, empresa));
            }

            return builder.ToString();
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
        private static string CrearContenidoCreditos(
            IEnumerable<CcProcesoMensualArchivoF05RegistroDbModel> registros,
            CcProcesoMensualArchivoF05ConfigDbModel configuracion,
            CcProcesoMensualArchivoF05EmpresaDbModel empresa)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                var tipoMovimiento = ObtenerTipoMovimientoCoopeCaja(registro.Movimiento);

                if (tipoMovimiento != 1)
                {
                    builder.AppendLine(CrearLineaCredito(registro, configuracion, empresa));
                }
            }

            return builder.ToString();
        }

        private static string CrearLineaAporte(
            CcProcesoMensualArchivoF05RegistroDbModel registro,
            CcProcesoMensualArchivoF05ConfigDbModel configuracion,
            CcProcesoMensualArchivoF05EmpresaDbModel empresa)
        {
            var nombre = SepararNombre(registro.Nombre);

            return TomarIzquierda(configuracion.CodigoAportes, 6)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(registro.Cedula?.Trim(), "I", "0", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Apellido1, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Apellido2, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Nombre1, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Nombre2, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    Helpers.CcProcesoMensualArchivoRutaHelperDb.DepurarCadena(empresa.Direccion),
                    "D",
                    " ",
                    140)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(empresa.Telefono, "I", "0", 8)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "I", "0", 8)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "I", "0", 11);
        }

        private static string CrearLineaCredito(
            CcProcesoMensualArchivoF05RegistroDbModel registro,
            CcProcesoMensualArchivoF05ConfigDbModel configuracion,
            CcProcesoMensualArchivoF05EmpresaDbModel empresa)
        {
            var nombre = SepararNombre(registro.Nombre);
            var monto = Convert.ToInt64(registro.MontoActual * 100).ToString(CultureInfo.InvariantCulture);

            return TomarIzquierda(configuracion.CodigoCreditos, 6)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(registro.CodDepartamento?.Trim(), "I", "0", 3)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(registro.Cedula?.Trim(), "I", "0", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Apellido1, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Apellido2, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Nombre1, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(nombre.Nombre2, "D", " ", 15)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    Helpers.CcProcesoMensualArchivoRutaHelperDb.DepurarCadena(empresa.Direccion),
                    "D",
                    " ",
                    140)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(empresa.Telefono, "I", "0", 8)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno("0", "I", "0", 8)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(monto, "I", "0", 11);
        }

        private static CcProcesoMensualArchivoF05NombreModel SepararNombre(string? nombreCompleto)
        {
            var apellido1 = new StringBuilder();
            var apellido2 = new StringBuilder();
            var nombre1 = new StringBuilder();
            var nombre2 = new StringBuilder();

            var posicion = 1;

            foreach (var caracter in nombreCompleto ?? string.Empty)
            {
                if (caracter == ' ')
                {
                    posicion++;
                    continue;
                }

                switch (posicion)
                {
                    case 1:
                        apellido1.Append(caracter);
                        break;

                    case 2:
                        apellido2.Append(caracter);
                        break;

                    case 3:
                        nombre1.Append(caracter);
                        break;

                    case 4:
                        nombre2.Append(caracter);
                        break;
                }
            }

            return new CcProcesoMensualArchivoF05NombreModel
            {
                Apellido1 = apellido1.ToString(),
                Apellido2 = apellido2.ToString(),
                Nombre1 = nombre1.ToString(),
                Nombre2 = nombre2.ToString()
            };
        }

        private static string CrearNombreArchivo(
            int codInstitucion,
            decimal fechaProceso,
            DateTime fechaServidor,
            string tipo)
        {
            var codigoInstitucion = codInstitucion.ToString("00", CultureInfo.InvariantCulture);
            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F05] - COOPECAJA-{tipo}.txt";
        }

        private static string TomarIzquierda(string? valor, int largo)
        {
            var texto = valor ?? string.Empty;

            return texto.Length > largo
                ? texto[..largo]
                : texto;
        }

        private static bool EsCodigoNo(string codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                CodigoNo,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsMovimientoExclusion(string movimiento)
        {
            return string.Equals(
                movimiento?.Trim(),
                MovimientoExclusion,
                StringComparison.OrdinalIgnoreCase);
        }

        private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }

        private sealed class CcProcesoMensualArchivoF05ConfigDbModel
        {
            public string CodigoAportes { get; set; } = string.Empty;
            public string CodigoCreditos { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF05EmpresaDbModel
        {
            public string Direccion { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF05RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string CodDepartamento { get; set; } = string.Empty;
            public string Direccion { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF05NombreModel
        {
            public string Apellido1 { get; set; } = string.Empty;
            public string Apellido2 { get; set; } = string.Empty;
            public string Nombre1 { get; set; } = string.Empty;
            public string Nombre2 { get; set; } = string.Empty;
        }
    }
}
