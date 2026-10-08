using System.Security;
using System.Text;

namespace Galileo.DataBaseTier
{
    /// <summary>
    /// Reconstruye valores que se usan en rutas de archivos copiando cada carácter desde una
    /// lista permitida constante. El resultado nunca contiene el carácter original recibido,
    /// sino su equivalente de la lista, por lo que un dato externo no llega tal cual a las
    /// operaciones de disco (Path Traversal, CWE-22).
    /// </summary>
    public static class SafePath
    {
        private const string Digits = "0123456789";
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string Accents = "áéíóúüñÁÉÍÓÚÜÑàèìòùâêîôûäëïöçÀÈÌÒÙÂÊÎÔÛÄËÏÖÇ";

        /// <summary>Solo dígitos.</summary>
        public const string DigitChars = Digits;

        /// <summary>Dígitos, guion y punto (códigos de cuenta).</summary>
        public const string AccountChars = Digits + "-.";

        /// <summary>Letras, dígitos, guion bajo, punto y guion (nombres de reporte).</summary>
        public const string ReportNameChars = Letters + Digits + "_.-";

        /// <summary>Caracteres permitidos en un nombre de archivo o carpeta (sin separadores de ruta).</summary>
        public const string FileNameChars = Letters + Digits + Accents + " _.-()[]{}&,;'+@#%$!~=^`";

        /// <summary>Caracteres permitidos en una ruta completa (incluye unidad y separadores).</summary>
        public const string PathChars = FileNameChars + @":\/";

        /// <summary>
        /// Reconstruye el valor y lanza <see cref="SecurityException"/> si contiene algún carácter no permitido.
        /// </summary>
        /// <param name="value">Valor recibido.</param>
        /// <param name="allowedChars">Lista de caracteres permitidos.</param>
        /// <param name="paramName">Nombre del parámetro para el mensaje de error.</param>
        /// <returns>Texto reconstruido solo con caracteres de la lista permitida.</returns>
        public static string Strict(string? value, string allowedChars, string paramName)
        {
            var source = value ?? string.Empty;
            var builder = new StringBuilder(source.Length);

            foreach (var candidate in source)
            {
                if (!TryGetAllowed(allowedChars, candidate, out var allowed))
                {
                    throw new SecurityException($"{paramName} contiene caracteres no permitidos.");
                }

                builder.Append(allowed);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Reconstruye el valor reemplazando por guion bajo los caracteres no permitidos.
        /// </summary>
        /// <param name="value">Valor recibido.</param>
        /// <param name="allowedChars">Lista de caracteres permitidos.</param>
        /// <returns>Texto reconstruido solo con caracteres de la lista permitida.</returns>
        public static string Lenient(string? value, string allowedChars)
        {
            var source = value ?? string.Empty;
            var builder = new StringBuilder(source.Length);

            foreach (var candidate in source)
            {
                builder.Append(TryGetAllowed(allowedChars, candidate, out var allowed) ? allowed : '_');
            }

            return builder.ToString();
        }

        /// <summary>
        /// Reconstruye el valor descartando los caracteres no permitidos.
        /// </summary>
        /// <param name="value">Valor recibido.</param>
        /// <param name="allowedChars">Lista de caracteres permitidos.</param>
        /// <returns>Texto reconstruido solo con caracteres de la lista permitida.</returns>
        public static string Filter(string? value, string allowedChars)
        {
            var source = value ?? string.Empty;
            var builder = new StringBuilder(source.Length);

            foreach (var candidate in source)
            {
                if (TryGetAllowed(allowedChars, candidate, out var allowed))
                {
                    builder.Append(allowed);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Reconstruye una ruta completa, rechaza segmentos ".." y devuelve la ruta normalizada.
        /// </summary>
        /// <param name="value">Ruta recibida.</param>
        /// <param name="paramName">Nombre del parámetro para el mensaje de error.</param>
        /// <returns>Ruta absoluta normalizada.</returns>
        public static string RootPath(string? value, string paramName)
        {
            var rebuilt = Strict(value, PathChars, paramName);

            if (rebuilt.Split(PathSeparators).Contains(".."))
            {
                throw new SecurityException($"{paramName} contiene segmentos no permitidos.");
            }

            return Path.GetFullPath(rebuilt);
        }

        private static readonly char[] PathSeparators = ['\\', '/'];

        private static bool TryGetAllowed(string allowedChars, char candidate, out char allowed)
        {
            var found = false;
            allowed = default;

            for (var i = 0; i < allowedChars.Length; i++)
            {
                if (allowedChars[i] == candidate)
                {
                    allowed = allowedChars[i];
                    found = true;
                }
            }

            return found;
        }
    }
}
