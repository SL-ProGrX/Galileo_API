using Galileo.Models.TES;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos.frmTES_EmisionDocumentos
{
    public static class FrmTesEmisionDocumentosResultadoExtractor
    {
        /// <summary>
        /// Extrae los archivos del contrato JSON legado sin modificar sus bytes lógicos.
        /// </summary>
        public static IReadOnlyList<TesEmisionDocumentosArchivoGenerado> Extraer(string resultado)
        {
            if (string.IsNullOrWhiteSpace(resultado))
            {
                return Array.Empty<TesEmisionDocumentosArchivoGenerado>();
            }

            var archivos = new List<TesEmisionDocumentosArchivoGenerado>();
            ProcesarToken(ParsearToken(resultado), archivos);
            return archivos;
        }

        /// <summary>
        /// Conserva el contexto funcional legado y elimina únicamente el contenido pesado de archivos.
        /// </summary>
        public static string CrearContextoLigero(string resultado)
        {
            var token = ParsearToken(resultado);
            return token == null
                ? string.Empty
                : SanitizarToken(token).ToString(Newtonsoft.Json.Formatting.None);
        }

        private static JToken SanitizarToken(JToken token)
        {
            if (token is JObject objeto)
            {
                var copia = new JObject();
                foreach (var propiedad in objeto.Properties())
                {
                    copia[propiedad.Name] = EsContenidoPesado(propiedad.Name)
                        ? JValue.CreateString(string.Empty)
                        : SanitizarToken(propiedad.Value);
                }
                return copia;
            }

            if (token is JArray arreglo)
            {
                return new JArray(arreglo.Select(SanitizarToken));
            }

            if (token is JValue valor && valor.Type == JTokenType.String)
            {
                var texto = valor.Value<string>();
                var anidado = ParsearToken(texto);
                return anidado == null
                    ? valor.DeepClone()
                    : JValue.CreateString(
                        SanitizarToken(anidado).ToString(Newtonsoft.Json.Formatting.None));
            }

            return token.DeepClone();
        }

        private static bool EsContenidoPesado(string nombre) =>
            string.Equals(nombre, "FileContents", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(nombre, "contenido", StringComparison.OrdinalIgnoreCase);

        private static void ProcesarToken(
            JToken? token,
            ICollection<TesEmisionDocumentosArchivoGenerado> archivos)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return;
            }

            if (token is JValue valor && valor.Type == JTokenType.String)
            {
                ProcesarTexto(valor.Value<string>(), archivos);
                return;
            }

            if (token is JObject objeto)
            {
                if (IntentarExtraerPdf(objeto, archivos) ||
                    IntentarExtraerTransferencia(objeto, archivos))
                {
                    return;
                }

                foreach (var propiedad in objeto.Properties())
                {
                    ProcesarToken(propiedad.Value, archivos);
                }
                return;
            }

            if (token is JArray arreglo)
            {
                foreach (var elemento in arreglo)
                {
                    ProcesarToken(elemento, archivos);
                }
            }
        }

        private static void ProcesarTexto(
            string? texto,
            ICollection<TesEmisionDocumentosArchivoGenerado> archivos)
        {
            var token = ParsearToken(texto);
            if (token != null)
            {
                ProcesarToken(token, archivos);
            }
        }

        private static JToken? ParsearToken(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            var limpio = texto.Trim();
            if (!limpio.StartsWith('{') && !limpio.StartsWith('['))
            {
                return null;
            }

            try
            {
                return JToken.Parse(limpio);
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return null;
            }
        }

        private static bool IntentarExtraerPdf(
            JObject objeto,
            ICollection<TesEmisionDocumentosArchivoGenerado> archivos)
        {
            var contenidoToken = ObtenerPropiedad(objeto, "FileContents");
            var nombreToken = ObtenerPropiedad(objeto, "FileDownloadName");
            if (contenidoToken == null || nombreToken == null)
            {
                return false;
            }

            var contenidoBase64 = contenidoToken.Value<string>();
            var nombre = NormalizarNombre(nombreToken.Value<string>(), "documento.pdf");
            if (string.IsNullOrWhiteSpace(contenidoBase64))
            {
                return true;
            }

            try
            {
                archivos.Add(new TesEmisionDocumentosArchivoGenerado
                {
                    Nombre = nombre,
                    ContentType = "application/pdf",
                    Contenido = Convert.FromBase64String(contenidoBase64)
                });
            }
            catch (FormatException)
            {
                // La validación posterior reportará que no existe un archivo publicable.
            }

            return true;
        }

        private static bool IntentarExtraerTransferencia(
            JObject objeto,
            ICollection<TesEmisionDocumentosArchivoGenerado> archivos)
        {
            var contenidoToken = ObtenerPropiedad(objeto, "contenido");
            var consecutivoToken = ObtenerPropiedad(objeto, "bancoConsec");
            if (contenidoToken == null || consecutivoToken == null)
            {
                return false;
            }

            var contenido = contenidoToken.Value<string>() ?? string.Empty;
            var consecutivo = NormalizarNombre(consecutivoToken.Value<string>(), "transferencia");
            var extension = NormalizarExtension(
                ObtenerPropiedad(objeto, "extension")?.Value<string>());
            archivos.Add(new TesEmisionDocumentosArchivoGenerado
            {
                Nombre = $"{consecutivo}.{extension}",
                ContentType = "text/plain",
                Contenido = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(contenido)
            });
            return true;
        }

        private static JToken? ObtenerPropiedad(JObject objeto, string nombre) =>
            objeto.Properties()
                .FirstOrDefault(propiedad =>
                    string.Equals(propiedad.Name, nombre, StringComparison.OrdinalIgnoreCase))?
                .Value;

        private static string NormalizarNombre(string? nombre, string valorPredeterminado)
        {
            var seguro = Path.GetFileName((nombre ?? string.Empty).Trim());
            return string.IsNullOrWhiteSpace(seguro) ? valorPredeterminado : seguro;
        }

        private static string NormalizarExtension(string? extension)
        {
            var limpia = (extension ?? "txt").Trim().TrimStart('.').ToLowerInvariant();
            return limpia.Length is > 0 and <= 10 && limpia.All(char.IsLetterOrDigit)
                ? limpia
                : "txt";
        }
    }
}
