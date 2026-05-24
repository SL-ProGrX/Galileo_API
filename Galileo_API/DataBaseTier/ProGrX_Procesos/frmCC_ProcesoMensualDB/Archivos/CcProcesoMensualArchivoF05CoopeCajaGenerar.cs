
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

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var contexto = CrearContexto(
                connection,
                request);

            var archivosGenerados = new List<string>();

            AgregarArchivoSiAplica(
                archivosGenerados,
                contexto,
                TipoAporte,
                contexto.Configuracion.CodigoAportes);

            AgregarArchivoSiAplica(
                archivosGenerados,
                contexto,
                TipoCredito,
                contexto.Configuracion.CodigoCreditos);

            return CrearRespuesta(
                archivosGenerados);
        }

        private static CcProcesoMensualArchivoF05Contexto CrearContexto(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return new CcProcesoMensualArchivoF05Contexto
            {
                Connection = connection,
                Request = request,
                Configuracion = ObtenerConfiguracion(
                    connection,
                    request.CodInstitucion),
                Empresa = ObtenerDatosEmpresa(connection),
                FechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection),
                RutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request)
            };
        }

        private static void AgregarArchivoSiAplica(
            ICollection<string> archivosGenerados,
            CcProcesoMensualArchivoF05Contexto contexto,
            string tipo,
            string codigoConfigurado)
        {
            if (EsCodigoNo(codigoConfigurado))
            {
                return;
            }

            archivosGenerados.Add(
                GenerarArchivoPorTipo(
                    contexto,
                    tipo));
        }

        private static string GenerarArchivoPorTipo(
            CcProcesoMensualArchivoF05Contexto contexto,
            string tipo)
        {
            var nombreArchivo = CrearNombreArchivo(
                contexto.Request.CodInstitucion,
                contexto.Request.FechaProceso,
                contexto.FechaServidor,
                tipo);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                contexto.RutaDirectorio,
                nombreArchivo);

            var registros = ObtenerRegistros(
                contexto.Connection,
                contexto.Request.CodInstitucion,
                contexto.Request.FechaProceso,
                tipo);

            var contenido = CrearContenidoArchivo(
                registros,
                contexto.Configuracion,
                contexto.Empresa,
                tipo);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                contexto.RutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return rutaArchivo;
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

        private static List<CcProcesoMensualArchivoF05RegistroDbModel> ObtenerRegistros(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso,
            string tipo)
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
                  AND P.tipo = @Tipo
                ORDER BY P.cedula";

            return [.. connection.Query<CcProcesoMensualArchivoF05RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    Tipo = tipo
                })];
        }

        private static string CrearContenidoArchivo(
            IEnumerable<CcProcesoMensualArchivoF05RegistroDbModel> registros,
            CcProcesoMensualArchivoF05ConfigDbModel configuracion,
            CcProcesoMensualArchivoF05EmpresaDbModel empresa,
            string tipo)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros.Where(registro => DebeImprimirRegistro(registro, tipo)))
            {
                builder.AppendLine(
                    CrearLineaArchivo(
                        registro,
                        configuracion,
                        empresa,
                        tipo));
            }

            return builder.ToString();
        }

        private static bool DebeImprimirRegistro(
            CcProcesoMensualArchivoF05RegistroDbModel registro,
            string tipo)
        {
            return !EsTipoCredito(tipo)
                || ObtenerTipoMovimientoCoopeCaja(registro.Movimiento) != 1;
        }

        private static string CrearLineaArchivo(
            CcProcesoMensualArchivoF05RegistroDbModel registro,
            CcProcesoMensualArchivoF05ConfigDbModel configuracion,
            CcProcesoMensualArchivoF05EmpresaDbModel empresa,
            string tipo)
        {
            var nombre = Helpers.CcProcesoMensualArchivoRutaHelperDb.SepararNombre(
                registro.Nombre);

            return ObtenerCodigoArchivo(configuracion, tipo)
                + CrearDetallePersona(registro, nombre, tipo)
                + CrearDetalleEmpresa(empresa)
                + CrearFinalArchivo(registro, tipo);
        }

        private static string ObtenerCodigoArchivo(
            CcProcesoMensualArchivoF05ConfigDbModel configuracion,
            string tipo)
        {
            var codigo = EsTipoAporte(tipo)
                ? configuracion.CodigoAportes
                : configuracion.CodigoCreditos;

            return Helpers.CcProcesoMensualArchivoRutaHelperDb.TomarIzquierda(
                codigo,
                6);
        }

        private static string CrearDetallePersona(
            CcProcesoMensualArchivoF05RegistroDbModel registro,
            CcProcesoMensualArchivoNombreModel nombre,
            string tipo)
        {
            var builder = new StringBuilder();

            if (EsTipoCredito(tipo))
            {
                AgregarCampoRelleno(
                    builder,
                    registro.CodDepartamento?.Trim(),
                    "I",
                    "0",
                    3);
            }

            AgregarCampoRelleno(builder, registro.Cedula?.Trim(), "I", "0", 15);
            AgregarCampoRelleno(builder, nombre.Apellido1, "D", " ", 15);
            AgregarCampoRelleno(builder, nombre.Apellido2, "D", " ", 15);
            AgregarCampoRelleno(builder, nombre.Nombre1, "D", " ", 15);
            AgregarCampoRelleno(builder, nombre.Nombre2, "D", " ", 15);

            return builder.ToString();
        }

        private static string CrearDetalleEmpresa(
            CcProcesoMensualArchivoF05EmpresaDbModel empresa)
        {
            var builder = new StringBuilder();

            AgregarCampoRelleno(
                builder,
                Helpers.CcProcesoMensualArchivoRutaHelperDb.DepurarCadena(empresa.Direccion),
                "D",
                " ",
                140);

            AgregarCampoRelleno(builder, empresa.Telefono, "I", "0", 8);
            AgregarCampoRelleno(builder, "0", "I", "0", 8);

            return builder.ToString();
        }

        private static string CrearFinalArchivo(
            CcProcesoMensualArchivoF05RegistroDbModel registro,
            string tipo)
        {
            var monto = EsTipoCredito(tipo)
                ? Convert.ToInt64(registro.MontoActual * 100).ToString(CultureInfo.InvariantCulture)
                : "0";

            return Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                monto,
                "I",
                "0",
                11);
        }

        private static void AgregarCampoRelleno(
            StringBuilder builder,
            string? valor,
            string direccion,
            string relleno,
            int largo)
        {
            builder.Append(
                Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    valor,
                    direccion,
                    relleno,
                    largo));
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
            DateTime fechaServidor,
            string tipo)
        {
            var codigoInstitucion = codInstitucion.ToString("00", CultureInfo.InvariantCulture);
            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F05] - COOPECAJA-{tipo}.txt";
        }

        private static CcProcesoMensualArchivoGeneradoModel CrearRespuesta(
            List<string> archivosGenerados)
        {
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

        private static bool EsTipoCredito(string tipo)
        {
            return string.Equals(
                tipo,
                TipoCredito,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsTipoAporte(string tipo)
        {
            return string.Equals(
                tipo,
                TipoAporte,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsCodigoNo(string? codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                CodigoNo,
                StringComparison.OrdinalIgnoreCase);
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
            public decimal MontoActual { get; set; }
            public string Movimiento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string CodDepartamento { get; set; } = string.Empty;
            public string Direccion { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF05Contexto
        {
            public IDbConnection Connection { get; init; } = default!;
            public CcProcesoMensualGeneraArchivoRequest Request { get; init; } = default!;
            public CcProcesoMensualArchivoF05ConfigDbModel Configuracion { get; init; } = new();
            public CcProcesoMensualArchivoF05EmpresaDbModel Empresa { get; init; } = new();
            public DateTime FechaServidor { get; init; }
            public string RutaDirectorio { get; init; } = string.Empty;
        }
    }
}
