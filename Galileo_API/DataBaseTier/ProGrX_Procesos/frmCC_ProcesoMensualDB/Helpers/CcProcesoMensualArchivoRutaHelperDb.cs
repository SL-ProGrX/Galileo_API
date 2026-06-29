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
        public static readonly Encoding Utf8SinBom = new UTF8Encoding(false);

        /// <summary>
        /// Crea un nombre de archivo estándar para los archivos generados en el proceso mensual.
        /// </summary>
        /// <param name="codInstitucion"></param>
        /// <param name="fechaProceso"></param>
        /// <param name="codigoInstDeduc"></param>
        /// <param name="fechaServidor"></param>
        /// <param name="codigoFormato"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Formatea la fecha de proceso en el formato "YYYY-MM" a partir de un valor decimal.
        /// </summary>
        /// <param name="fechaProceso"></param>
        /// <returns></returns>
        public static string FormatearFechaProceso(decimal fechaProceso)
        {
            var fechaBase = ObtenerFechaProcesoTexto(fechaProceso);

            return fechaBase.Length >= 6
                ? $"{fechaBase[..4]}-{fechaBase.Substring(4, 2)}"
                : fechaBase;
        }

        /// <summary>
        /// Obtiene el año de proceso a partir de un valor decimal que representa la fecha de proceso.
        /// </summary>
        /// <param name="fechaProceso"></param>
        /// <returns></returns>
        public static string ObtenerAnioProceso(decimal fechaProceso)
        {
            var fechaBase = ObtenerFechaProcesoTexto(fechaProceso);

            return fechaBase.Length >= 4
                ? fechaBase[..4]
                : fechaBase;
        }
        /// <summary>
        /// Rellena una cadena con ceros a la izquierda hasta alcanzar un largo específico.
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="largo"></param>
        /// <returns></returns>
        public static string RellenarCerosIzquierda(string? valor, int largo)
        {
            return AjustarTexto(
                valor,
                largo,
                '0',
                alinearDerecha: true);
        }
        /// <summary>
        /// Rellena una cadena con espacios a la derecha hasta alcanzar un largo específico.
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="largo"></param>
        /// <returns></returns>
        public static string RellenarEspaciosDerecha(string? valor, int largo)
        {
            return AjustarTexto(
                valor,
                largo,
                ' ',
                alinearDerecha: false);
        }
        /// <summary>
        /// Obtiene la ruta completa para almacenar la planilla generada, basada en la fecha de proceso y el nombre de la institución.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="rutaBaseConfigurada"></param>
        /// <returns></returns>
        public static string ObtenerRutaPlanilla(CcProcesoMensualGeneraArchivoRequest request, string rutaBaseConfigurada)
        {
            ArgumentNullException.ThrowIfNull(request);

            var anio = Path.GetFileName(ObtenerAnioProceso(request.FechaProceso));
            var nombreInstitucion = Path.GetFileName(LimpiarNombreDirectorio(request.NombreInstitucion));


            var rutaBase = Path.GetFullPath(rutaBaseConfigurada);

            var rutaFinal = Path.GetFullPath(Path.Combine(
                rutaBase,
                CarpetaPlanilla,
                nombreInstitucion,
                anio));

            ValidarRutaDentroDeBase(rutaBase, rutaFinal);

            return rutaFinal;
        }
        /// <summary>
        /// Crea un directorio si no existe, asegurando que la ruta esté dentro de la ruta base configurada.
        /// </summary>
        /// <param name="rutaBaseConfigurada"></param>
        /// <param name="rutaDirectorio"></param>
        public static void CrearDirectorioSiNoExiste(string rutaBaseConfigurada, string rutaDirectorio)
        {
            var rutaBase = Path.GetFullPath(rutaBaseConfigurada);
            var rutaDirectorioSeguro = Path.GetFullPath(rutaDirectorio);

            ValidarRutaDentroDeBase(rutaBase, rutaDirectorioSeguro);

            Directory.CreateDirectory(rutaDirectorioSeguro);
        }
        /// <summary>
        /// Valida que la ruta final esté dentro de la ruta base configurada, lanzando una excepción si no es así.
        /// </summary>
        /// <param name="rutaBase"></param>
        /// <param name="rutaFinal"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void ValidarRutaDentroDeBase(string rutaBase, string rutaFinal)
        {
            var rutaBaseNormalizada = Path.GetFullPath(rutaBase);

            if (!rutaBaseNormalizada.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                rutaBaseNormalizada += Path.DirectorySeparatorChar;
            }

            var rutaFinalNormalizada = Path.GetFullPath(rutaFinal);

            if (!rutaFinalNormalizada.StartsWith(rutaBaseNormalizada, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La ruta generada está fuera del directorio permitido.");
            }
        }
        /// <summary>
        /// Obtiene la fecha y hora actual del servidor de base de datos utilizando una consulta SQL.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT dbo.MyGetdate() AS Fecha";

            return connection.QueryFirstOrDefault<DateTime>(query);
        }
        /// <summary>
        /// Obtiene los registros de la planilla general desde la base de datos según los parámetros proporcionados.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="codInstitucion"></param>
        /// <param name="fechaProceso"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public static List<CcProcesoMensualArchivoRegistroDbModel> ObtenerRegistrosGeneral(IDbConnection connection, int codInstitucion, decimal fechaProceso, string tipo)
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

        /// <summary>
        /// Obtiene la configuración general de la institución desde la base de datos según el código de institución proporcionado.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public static CcProcesoMensualArchivoConfiguracionModel ObtenerConfiguracionGeneral(IDbConnection connection, int codInstitucion)
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

        /// <summary>
        /// Guarda contenido de texto en un archivo validando que la ruta esté dentro de la base permitida.
        /// </summary>
        /// <param name="rutaBaseConfigurada"></param>
        /// <param name="rutaDirectorio"></param>
        /// <param name="rutaArchivo"></param>
        /// <param name="contenido"></param>
        /// <param name="encoding"></param>
        public static void GuardarArchivoTexto(
            string rutaBaseConfigurada,
            string rutaDirectorio,
            string rutaArchivo,
            string contenido,
            Encoding encoding)
        {
            var rutaBase = Path.GetFullPath(rutaBaseConfigurada);
            var rutaDirectorioSeguro = Path.GetFullPath(rutaDirectorio);
            var rutaArchivoSeguro = Path.GetFullPath(rutaArchivo);

            ValidarRutaDentroDeBase(rutaBase, rutaDirectorioSeguro);
            ValidarRutaDentroDeBase(rutaBase, rutaArchivoSeguro);

            Directory.CreateDirectory(rutaDirectorioSeguro);

            if (File.Exists(rutaArchivoSeguro))
            {
                File.Delete(rutaArchivoSeguro);
            }

            var contenidoLimpio = contenido.TrimStart('\uFEFF');

            File.WriteAllText(
                rutaArchivoSeguro,
                contenidoLimpio,
                encoding);
        }

        /// <summary>
        /// Combina directorio y nombre de archivo validando que ambos sean seguros y estén dentro de la ruta base.
        /// </summary>
        /// <param name="rutaBaseConfigurada"></param>
        /// <param name="rutaDirectorio"></param>
        /// <param name="nombreArchivo"></param>
        /// <returns></returns>
        public static string CombinarArchivo(string rutaBaseConfigurada, string rutaDirectorio, string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaDirectorio))
            {
                throw new ArgumentException("La ruta del directorio es requerida.", nameof(rutaDirectorio));
            }

            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                throw new ArgumentException("El nombre del archivo es requerido.", nameof(nombreArchivo));
            }

            var nombreArchivoSeguro = Path.GetFileName(nombreArchivo);

            if (!string.Equals(nombreArchivo, nombreArchivoSeguro, StringComparison.Ordinal))
            {
                throw new ArgumentException("El nombre del archivo no es válido.", nameof(nombreArchivo));
            }
            if (Path.IsPathRooted(nombreArchivoSeguro))
            {
                throw new ArgumentException("El nombre del archivo no es válido.", nameof(nombreArchivo));
            }

            var rutaBase = Path.GetFullPath(rutaBaseConfigurada);
            var rutaDirectorioSeguro = Path.GetFullPath(rutaDirectorio);

            ValidarRutaDentroDeBase(rutaBase, rutaDirectorioSeguro);

            var rutaArchivo = Path.GetFullPath(Path.Combine(
                rutaDirectorioSeguro,
                nombreArchivoSeguro));

            ValidarRutaDentroDeBase(rutaBase, rutaArchivo);

            return rutaArchivo;
        }

        /// <summary>
        /// Limpia una cadena eliminando caracteres no permitidos para el proceso de generación.
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static string DepurarCadena(string? valor)
        {
            var texto = valor?.Trim() ?? string.Empty;

            return new string([.. texto.Where(EsCaracterPermitido)]);
        }

        /// <summary>
        /// Rellena una cadena con el carácter indicado según dirección y tamaño requerido.
        /// </summary>
        /// <param name="cadena"></param>
        /// <param name="direccion"></param>
        /// <param name="charRelleno"></param>
        /// <param name="cantidad"></param>
        /// <returns></returns>
        public static string FxStringRelleno(string? cadena, string direccion, string charRelleno, int cantidad)
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

        /// <summary>
        /// Separa un nombre completo en cuatro partes: dos apellidos y dos nombres.
        /// </summary>
        /// <param name="nombreCompleto"></param>
        /// <returns></returns>
        public static CcProcesoMensualArchivoNombreModel SepararNombre(string? nombreCompleto)
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

        /// <summary>
        /// Obtiene la lista de movimientos según el comparador de configuración.
        /// </summary>
        /// <param name="configuracion"></param>
        /// <returns></returns>
        public static List<string> ObtenerMovimientosPorComparador(CcProcesoMensualArchivoConfiguracionModel configuracion)
        {
            return configuracion.ComparaIndicador != 1
                ? ["I", "E", "M", "C", "P"]
                : ObtenerMovimientosPorIndicadores(configuracion);
        }

        /// <summary>
        /// Obtiene movimientos habilitados según los indicadores individuales de la configuración.
        /// </summary>
        /// <param name="configuracion"></param>
        /// <returns></returns>
        public static List<string> ObtenerMovimientosPorIndicadores(CcProcesoMensualArchivoConfiguracionModel configuracion)
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

        /// <summary>
        /// Obtiene el código de institución a usar en nombre de archivo.
        /// </summary>
        /// <param name="codInstitucion"></param>
        /// <param name="codigoInstDeduc"></param>
        /// <returns></returns>
        public static string ObtenerCodigoInstitucionArchivo(int codInstitucion, string? codigoInstDeduc)
        {
            return string.IsNullOrWhiteSpace(codigoInstDeduc)
                ? codInstitucion.ToString("00", CultureInfo.InvariantCulture)
                : codigoInstDeduc.Trim();
        }

        /// <summary>
        /// Formatea una fecha en formato dd.MM.yyyy.
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public static string FormatearFechaPunto(DateTime fecha)
        {
            return fecha.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Construye un contenido de texto incluyendo solo cadenas no vacías al final.
        /// </summary>
        /// <param name="cadenas"></param>
        /// <returns></returns>
        public static string CrearContenidoCadenasNoVacias(IEnumerable<string> cadenas)
        {
            var builder = new StringBuilder();

            foreach (var cadena in cadenas.Where(cadena => cadena.TrimEnd().Length > 0))
            {
                builder.AppendLine(cadena);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Retorna la parte izquierda de una cadena con la cantidad indicada.
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="cantidad"></param>
        /// <returns></returns>
        public static string TomarIzquierda(string? valor, int cantidad)
        {
            return CortarTexto(
                valor,
                cantidad,
                desdeDerecha: false);
        }

        /// <summary>
        /// Convierte la fecha de proceso decimal en texto sin parte decimal.
        /// </summary>
        /// <param name="fechaProceso"></param>
        /// <returns></returns>
        private static string ObtenerFechaProcesoTexto(decimal fechaProceso)
        {
            return Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Limpia el nombre de directorio reemplazando caracteres inválidos.
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Ajusta una cadena al largo indicado, recortando y rellenando según alineación.
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="largo"></param>
        /// <param name="relleno"></param>
        /// <param name="alinearDerecha"></param>
        /// <returns></returns>
        private static string AjustarTexto(string? valor, int largo, char relleno, bool alinearDerecha)
        {
            var texto = CortarTexto(
                valor,
                largo,
                desdeDerecha: alinearDerecha);

            return alinearDerecha
                ? texto.PadLeft(largo, relleno)
                : texto.PadRight(largo, relleno);
        }

        /// <summary>
        /// Corta una cadena al largo indicado desde la izquierda o derecha.
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="largo"></param>
        /// <param name="desdeDerecha"></param>
        /// <returns></returns>
        private static string CortarTexto(string? valor, int largo, bool desdeDerecha)
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

        /// <summary>
        /// Obtiene el carácter de relleno a partir de una cadena.
        /// </summary>
        /// <param name="charRelleno"></param>
        /// <returns></returns>
        private static char ObtenerCaracterRelleno(string? charRelleno)
        {
            return string.IsNullOrEmpty(charRelleno)
                ? ' '
                : charRelleno[0];
        }

        /// <summary>
        /// Indica si un carácter es válido dentro del rango permitido por el proceso.
        /// </summary>
        /// <param name="caracter"></param>
        /// <returns></returns>
        private static bool EsCaracterPermitido(char caracter)
        {
            var ascii = (int)caracter;

            return ascii == 32 || ascii is > 47 and < 123;
        }

        /// <summary>
        /// Agrega un movimiento a la lista cuando su indicador está habilitado.
        /// </summary>
        /// <param name="movimientos"></param>
        /// <param name="indicador"></param>
        /// <param name="movimiento"></param>
        private static void AgregarMovimientoSiAplica(List<string> movimientos, int indicador, string movimiento)
        {
            if (indicador == 1)
            {
                movimientos.Add(movimiento);
            }
        }
    }
}

