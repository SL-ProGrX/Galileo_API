using System.Globalization;
using System.Text;
using System.Data;
using Dapper;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Helpers
{
    public static class CcProcesoMensualArchivoRutaHelperDb
    {
        private const string CarpetaPlanilla = "Planilla";
        private const string NombreInstitucionDefault = "SinInstitucion";
        private const string DireccionDerecha = "D";

        public static string CrearNombreArchivoEstandar(
            int codInstitucion,
            decimal fechaProceso,
            string codigoInstDeduc,
            DateTime fechaServidor,
            string codigoFormato,
            string extension)
        {
            var codigoInstitucion = ObtenerCodigoInstitucionArchivo(
                   codInstitucion,
                   codigoInstDeduc);

            var fechaProcesoTexto = FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-{codigoFormato}]{extension}";
        }

        public static string FormatearFechaProceso(decimal fechaProceso)
        {
            var fechaBase = ObtenerFechaProcesoTexto(fechaProceso);

            return fechaBase.Length >= 6
                ? $"{fechaBase[..4]}-{fechaBase.Substring(4, 2)}"
                : fechaBase;
        }

        public static string ObtenerAnioProceso(decimal fechaProceso)
        {
            var fechaBase = ObtenerFechaProcesoTexto(fechaProceso);

            return fechaBase.Length >= 4
                ? fechaBase[..4]
                : fechaBase;
        }

        public static string RellenarCerosIzquierda(string? valor, int largo)
        {
            return AjustarTexto(
                valor,
                largo,
                '0',
                alinearDerecha: true);
        }

        public static string RellenarEspaciosDerecha(string? valor, int largo)
        {
            return AjustarTexto(
                valor,
                largo,
                ' ',
                alinearDerecha: false);
        }

        public static string ObtenerRutaPlanilla(
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var anio = ObtenerAnioProceso(request.FechaProceso);
            var nombreInstitucion = LimpiarNombreDirectorio(request.NombreInstitucion);

            return Path.Combine(
                request.DirectorioResultados,
                CarpetaPlanilla,
                nombreInstitucion,
                anio);
        }

        public static void CrearDirectorioSiNoExiste(string rutaDirectorio)
        {
            Directory.CreateDirectory(rutaDirectorio);
        }

        public static string CombinarArchivo(
            string rutaDirectorio,
            string nombreArchivo)
        {
            return Path.Combine(rutaDirectorio, nombreArchivo);
        }

        public static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT dbo.MyGetdate() AS Fecha";

            return connection.QueryFirstOrDefault<DateTime>(query);
        }

        public static List<CcProcesoMensualArchivoRegistroDbModel> ObtenerRegistrosGeneral(
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
                    S.direccion AS Direccion
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.cod_institucion = @CodInstitucion
                  AND P.tipo = @Tipo
                ORDER BY P.cedula";

            return [.. connection.Query<CcProcesoMensualArchivoRegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    Tipo = tipo
                })];
        }

        public static CcProcesoMensualArchivoConfiguracionModel ObtenerConfiguracionGeneral(
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

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoConfiguracionModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoConfiguracionModel();
        }

        public static void GuardarArchivoTexto(
            string rutaDirectorio,
            string rutaArchivo,
            string contenido,
            Encoding encoding)
        {
            CrearDirectorioSiNoExiste(rutaDirectorio);

            if (File.Exists(rutaArchivo))
            {
                File.Delete(rutaArchivo);
            }

            var contenidoLimpio = contenido.TrimStart('\uFEFF');

            File.WriteAllText(
                rutaArchivo,
                contenidoLimpio,
                encoding);
        }

        public static string DepurarCadena(string? valor)
        {
            var texto = valor?.Trim() ?? string.Empty;

            return new string([.. texto.Where(EsCaracterPermitido)]);
        }

        public static string FxStringRelleno(
            string? cadena,
            string direccion,
            string charRelleno,
            int cantidad)
        {
            var valor = TomarIzquierda(
                cadena?.Trim(),
                cantidad);

            var relleno = ObtenerCaracterRelleno(charRelleno);

            var alinearDerecha = !string.Equals(
                direccion,
                DireccionDerecha,
                StringComparison.OrdinalIgnoreCase);

            return AjustarTexto(
                valor,
                cantidad,
                relleno,
                alinearDerecha);
        }

        public static CcProcesoMensualArchivoNombreModel SepararNombre(
            string? nombreCompleto)
        {
            var partes = new[]
            {
                new StringBuilder(),
                new StringBuilder(),
                new StringBuilder(),
                new StringBuilder()
            };

            var posicion = 0;

            foreach (var caracter in nombreCompleto ?? string.Empty)
            {
                if (caracter == ' ')
                {
                    posicion++;
                    continue;
                }

                if (posicion < partes.Length)
                {
                    partes[posicion].Append(caracter);
                }
            }

            return new CcProcesoMensualArchivoNombreModel
            {
                Apellido1 = partes[0].ToString(),
                Apellido2 = partes[1].ToString(),
                Nombre1 = partes[2].ToString(),
                Nombre2 = partes[3].ToString()
            };
        }

        public static List<string> ObtenerMovimientosPorComparador(
            CcProcesoMensualArchivoConfiguracionModel configuracion)
        {
            return configuracion.ComparaIndicador != 1
                ? ["I", "E", "M", "C", "P"]
                : ObtenerMovimientosPorIndicadores(configuracion);
        }

        public static List<string> ObtenerMovimientosPorIndicadores(
            CcProcesoMensualArchivoConfiguracionModel configuracion)
        {
            var movimientos = new List<string>();

            AgregarMovimientoSiAplica(
                movimientos,
                configuracion.IncInclusiones,
                "I");

            AgregarMovimientoSiAplica(
                movimientos,
                configuracion.IncExclusiones,
                "E");

            AgregarMovimientoSiAplica(
                movimientos,
                configuracion.IncModificaciones,
                "C");

            AgregarMovimientoSiAplica(
                movimientos,
                configuracion.IncMantienen,
                "M");

            movimientos.Add("P");

            return movimientos;
        }

        public static string ObtenerCodigoInstitucionArchivo(
            int codInstitucion,
            string? codigoInstDeduc)
        {
            return string.IsNullOrWhiteSpace(codigoInstDeduc)
                ? codInstitucion.ToString("00", CultureInfo.InvariantCulture)
                : codigoInstDeduc.Trim();
        }

        public static string FormatearFechaPunto(DateTime fecha)
        {
            return fecha.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }

        public static string CrearContenidoCadenasNoVacias(
            IEnumerable<string> cadenas)
        {
            var builder = new StringBuilder();

            foreach (var cadena in cadenas.Where(cadena => cadena.TrimEnd().Length > 0))
            {
                builder.AppendLine(cadena);
            }

            return builder.ToString();
        }

        public static string TomarIzquierda(
            string? valor,
            int cantidad)
        {
            return CortarTexto(
                valor,
                cantidad,
                desdeDerecha: false);
        }

        private static string ObtenerFechaProcesoTexto(decimal fechaProceso)
        {
            return Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);
        }

        private static string LimpiarNombreDirectorio(string? valor)
        {
            var nombre = string.IsNullOrWhiteSpace(valor)
                ? NombreInstitucionDefault
                : valor.Trim();

            foreach (var caracter in Path.GetInvalidFileNameChars())
            {
                nombre = nombre.Replace(caracter, '_');
            }

            return nombre;
        }

        private static string AjustarTexto(
            string? valor,
            int largo,
            char relleno,
            bool alinearDerecha)
        {
            var texto = CortarTexto(
                valor,
                largo,
                desdeDerecha: alinearDerecha);

            return alinearDerecha
                ? texto.PadLeft(largo, relleno)
                : texto.PadRight(largo, relleno);
        }

        private static string CortarTexto(
            string? valor,
            int largo,
            bool desdeDerecha)
        {
            var texto = valor ?? string.Empty;

            if (texto.Length <= largo)
            {
                return texto;
            }

            return desdeDerecha
                ? texto[^largo..]
                : texto[..largo];
        }

        private static char ObtenerCaracterRelleno(string? charRelleno)
        {
            return string.IsNullOrEmpty(charRelleno)
                ? ' '
                : charRelleno[0];
        }

        private static bool EsCaracterPermitido(char caracter)
        {
            var ascii = (int)caracter;

            return ascii == 32 || ascii is > 47 and < 123;
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
    }
}

