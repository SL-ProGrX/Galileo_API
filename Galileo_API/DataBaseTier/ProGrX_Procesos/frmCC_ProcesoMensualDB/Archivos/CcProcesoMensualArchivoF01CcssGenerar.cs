using Dapper;
using System.Data;
using System.Globalization;
using System.Text; 
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF01CcssGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "01";
        private const string ExtensionTxt = ".txt";
        private const string ContentTypeText = "text/plain";
        private const string CodigoNo = "NO";

        private const string TipoAporte = "A";
        private const string TipoCredito = "C";

        private readonly ArchivosGeneradosOptions _archivosOptions;
        public CcProcesoMensualArchivoF01CcssGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions)
        {
            _archivosOptions = archivosOptions.Value;
        }


        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];
        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion);

            var rutaDirectorio = CrearRutaDirectorio(request);
            var archivosGenerados = new List<string>();
            var ultimoArchivo = string.Empty;
            var rutaBase = _archivosOptions.RutaBase;

            if (!EsCodigoNo(configuracion.CodigoAportesEnv))
            {
                ultimoArchivo = GenerarArchivoAportes(
                    connection,
                    request,
                    configuracion,
                    rutaDirectorio, rutaBase);

                archivosGenerados.Add(ultimoArchivo);
            }

            if (!EsCodigoNo(configuracion.CodigoCreditosEnv))
            {
                ultimoArchivo = GenerarArchivoCreditos(
                    connection,
                    request,
                    configuracion.CodigoCreditosEnv,
                    configuracion.CodCreArc,
                    rutaDirectorio, rutaBase);

                archivosGenerados.Add(ultimoArchivo);

                if (DebeGenerarCreditoAlterno(configuracion))
                {
                    ultimoArchivo = GenerarArchivoCreditos(
                        connection,
                        request,
                        configuracion.CodigoCreditosAlternoEnv,
                        configuracion.CodCreArcAlterno,
                        rutaDirectorio, rutaBase);

                    archivosGenerados.Add(ultimoArchivo);
                }
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

        private static CcProcesoMensualArchivoF01ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_aportes_env, '') AS CodigoAportesEnv,
                    ISNULL(codigo_creditos_env, '') AS CodigoCreditosEnv,
                    ISNULL(codigo_creditos_alt_env, '') AS CodigoCreditosAlternoEnv,
                    ISNULL(codigo_aportes, '') AS CodApoArc,
                    ISNULL(codigo_creditos, '') AS CodCreArc,
                    ISNULL(CODIGO_CREDITOS_ALT, '') AS CodCreArcAlterno
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF01ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF01ConfigDbModel();
        }

        private static string GenerarArchivoAportes(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            CcProcesoMensualArchivoF01ConfigDbModel configuracion,
            string rutaDirectorio, string rutaBase)
        {
            var codigoAportes = FormatearCodigoEnvio(configuracion.CodigoAportesEnv);
            var nombreArchivo = CrearNombreArchivo( request.CodInstitucion, request.FechaProceso, configuracion.CodApoArc);
            
            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(rutaBase,rutaDirectorio, nombreArchivo);

            var registros = ObtenerRegistrosPorTipo(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                TipoAporte,
                null);

            var contenido = CrearContenidoAportes(registros, codigoAportes);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(rutaBase,
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return rutaArchivo;
        }

        private static string GenerarArchivoCreditos(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request,
            string codigoEnvio,
            string codigoArchivo,
            string rutaDirectorio,string rutaBase)
        {
            var codigoCredito = FormatearCodigoEnvio(codigoEnvio);

            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                codigoArchivo);
             
            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(rutaBase,rutaDirectorio, nombreArchivo);

            var registros = ObtenerRegistrosPorTipo(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                TipoCredito,
                codigoCredito);

            var contenido = CrearContenidoCreditos(registros, codigoCredito);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(rutaBase,
               rutaDirectorio,
               rutaArchivo,
               contenido,
               Encoding.UTF8); 

            return rutaArchivo;
        }

        private static List<CcProcesoMensualArchivoF01RegistroDbModel> ObtenerRegistrosPorTipo(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso,
            string tipo,
            string? codigoDeduccion)
        {
            const string query = @"
                SELECT
                    P.Cedula,
                    P.Monto_Actual AS MontoActual,
                    P.Cod_Deduccion AS CodDeduccion,
                    S.nombre AS Nombre
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.cod_institucion = @CodInstitucion
                  AND P.tipo = @Tipo
                  AND (@CodigoDeduccion IS NULL OR P.cod_Deduccion = @CodigoDeduccion)
                ORDER BY P.cedula";

            return [.. connection.Query<CcProcesoMensualArchivoF01RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    Tipo = tipo,
                    CodigoDeduccion = codigoDeduccion
                })];
        }

        private static string CrearContenidoAportes(
            IEnumerable<CcProcesoMensualArchivoF01RegistroDbModel> registros,
            string codigoAportes)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaAporte(registro, codigoAportes));
            }

            return builder.ToString();
        }

        private static string CrearContenidoCreditos( IEnumerable<CcProcesoMensualArchivoF01RegistroDbModel> registros,string codigoCreditos)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                builder.AppendLine(CrearLineaCredito(registro, codigoCreditos));
            }

            return builder.ToString();
        }

        private static string CrearLineaAporte(
            CcProcesoMensualArchivoF01RegistroDbModel registro,
            string codigoAportes)
        {
            var cedula = FormatearCedula(registro.Cedula);

            return cedula
                + "9999"
                + TomarIzquierda(codigoAportes, 4)
                + "00000000000000000000099999999    AHORRO00000000000000000000000000100000000";
        }

        private static string CrearLineaCredito(  CcProcesoMensualArchivoF01RegistroDbModel registro,  string codigoCreditos)
        {
            var cedula = FormatearCedula(registro.Cedula);
            var monto = FormatearMonto(registro.MontoActual);

            return cedula
                + "9999"
                + TomarIzquierda(codigoCreditos, 4)
                + monto
                + "0000000099999999"
                + "   CREDITO00000000000000000000000000100000000";
        }

        private static string CrearNombreArchivo(
            int codInstitucion,
            decimal fechaProceso,
            string codigoArchivo)
        {
            var institucion = codInstitucion.ToString("000", CultureInfo.InvariantCulture);
            var fechaTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var codigo = codigoArchivo?.Trim() ?? string.Empty;

            return $"{institucion}[{fechaTexto}]-ARC{codigo}{ExtensionTxt}";
        }

        private static string CrearRutaDirectorio(
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var anio = ObtenerAnioProceso(request.FechaProceso);
            var nombreInstitucion = LimpiarNombreDirectorio(request.NombreInstitucion);

            return Path.Combine(
                "C:\\ArchivosGenerados\\",
                "Planilla",
                nombreInstitucion,
                anio);
        }

        private static string ObtenerAnioProceso(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            return fechaBase.Length >= 4
                ? fechaBase[..4]
                : fechaBase;
        }

        private static string FormatearCodigoEnvio(string codigo)
        {
            var texto = codigo?.Trim() ?? string.Empty;

            if (EsCodigoNo(texto))
            {
                return CodigoNo;
            }

            return int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numero)
                ? numero.ToString("0000", CultureInfo.InvariantCulture)
                : texto.PadLeft(4, '0')[^4..];
        }

        private static string FormatearCedula(string cedula)
        {
            var texto = cedula?.Trim() ?? string.Empty;

            if (texto.Length < 11)
            {
                texto = texto.PadLeft(11, '0');
            }

            if (texto.Length > 11)
            {
                texto = texto[..11];
            }

            return texto;
        }

        private static string FormatearMonto(decimal monto)
        {
            var montoCentimos = monto * 100;

            return montoCentimos.ToString(
                "0000000000000",
                CultureInfo.InvariantCulture);
        }

        private static string TomarIzquierda(string valor, int cantidad)
        {
            var texto = valor ?? string.Empty;

            return texto.Length <= cantidad
                ? texto
                : texto[..cantidad];
        }

        private static bool EsCodigoNo(string codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                CodigoNo,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool DebeGenerarCreditoAlterno(
            CcProcesoMensualArchivoF01ConfigDbModel configuracion)
        {
            return !string.Equals(
                    FormatearCodigoEnvio(configuracion.CodigoCreditosAlternoEnv),
                    FormatearCodigoEnvio(configuracion.CodigoCreditosEnv),
                    StringComparison.OrdinalIgnoreCase)
                && !EsCodigoNo(configuracion.CodigoCreditosAlternoEnv);
        }

        private static string LimpiarNombreDirectorio(string valor)
        {
            var nombre = string.IsNullOrWhiteSpace(valor)
                ? "SinInstitucion"
                : valor.Trim();

            foreach (var caracter in Path.GetInvalidFileNameChars())
            {
                nombre = nombre.Replace(caracter, '_');
            }

            return nombre;
        }

        

        private sealed class CcProcesoMensualArchivoF01ConfigDbModel
        {
            public string CodigoAportesEnv { get; set; } = string.Empty;
            public string CodigoCreditosEnv { get; set; } = string.Empty;
            public string CodigoCreditosAlternoEnv { get; set; } = string.Empty;
            public string CodApoArc { get; set; } = string.Empty;
            public string CodCreArc { get; set; } = string.Empty;
            public string CodCreArcAlterno { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF01RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string CodDeduccion { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
