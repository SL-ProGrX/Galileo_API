using Galileo.Models.TES;
using System.Security.Cryptography;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public sealed class TesEmisionDocumentosArchivoStore
    {
        private readonly string _raiz;

        public TesEmisionDocumentosArchivoStore(string raiz)
        {
            if (string.IsNullOrWhiteSpace(raiz))
            {
                throw new ArgumentException("La raíz de archivos es requerida.", nameof(raiz));
            }

            _raiz = AsegurarSeparador(Path.GetFullPath(raiz));
        }

        /// <summary>
        /// Valida y publica un archivo mediante un movimiento atómico en el mismo volumen.
        /// </summary>
        public TesEmisionDocumentosArchivoPersistir Publicar(
            Guid procesoId,
            int orden,
            TesEmisionDocumentosArchivoGenerado archivo)
        {
            ValidarEntrada(archivo);
            var archivoId = Guid.NewGuid();
            var extension = ObtenerExtension(archivo.Nombre, archivo.ContentType);
            var integridad = ValidarIntegridad(archivo, extension);
            var directorio = ResolverDentroRaiz(procesoId.ToString("N"));
            Directory.CreateDirectory(directorio);

            var temporal = ResolverDentroRaiz(
                procesoId.ToString("N"),
                $"{archivoId:N}.tmp");
            var definitivo = ResolverDentroRaiz(
                procesoId.ToString("N"),
                $"{archivoId:N}.bin");

            try
            {
                using (var stream = new FileStream(
                    temporal,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    stream.Write(archivo.Contenido);
                    stream.Flush(flushToDisk: true);
                }

                File.Move(temporal, definitivo);
            }
            catch
            {
                EliminarTemporalSeguro(temporal);
                throw;
            }

            return new TesEmisionDocumentosArchivoPersistir
            {
                ArchivoId = archivoId,
                ProcesoId = procesoId,
                Orden = orden,
                Nombre = Path.GetFileName(archivo.Nombre),
                Extension = extension,
                ContentType = archivo.ContentType,
                RutaInterna = definitivo,
                Tamano = integridad.Tamano,
                Sha256 = integridad.Sha256,
                Paginas = extension == ".pdf" ? integridad.Paginas : null
            };
        }

        private static void ValidarEntrada(TesEmisionDocumentosArchivoGenerado archivo)
        {
            ArgumentNullException.ThrowIfNull(archivo);
            if (archivo.Contenido.Length == 0)
                throw new InvalidDataException("El archivo generado está vacío.");
            if (string.IsNullOrWhiteSpace(archivo.Nombre))
                throw new InvalidDataException("El nombre lógico del archivo es requerido.");
        }

        private static string ObtenerExtension(string nombre, string contentType)
        {
            var extension = Path.GetExtension(Path.GetFileName(nombre)).ToLowerInvariant();
            if (string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                extension = ".pdf";
            }

            if (!ExtensionEsValida(extension))
            {
                throw new InvalidDataException("La extensión del archivo generado no está permitida.");
            }

            return extension;
        }

        private static bool ExtensionEsValida(string extension) =>
            extension.Length is >= 2 and <= 11 &&
            extension[0] == '.' &&
            extension.Skip(1).All(caracter =>
                caracter is >= 'a' and <= 'z' or >= '0' and <= '9');

        private static TesEmisionDocumentosArchivoValidacion ValidarIntegridad(
            TesEmisionDocumentosArchivoGenerado archivo,
            string extension)
        {
            if (extension == ".pdf")
            {
                var validacionPdf = TesEmisionDocumentosArchivoValidador.ValidarPdf(archivo.Contenido);
                if (!validacionPdf.EsValido)
                    throw new InvalidDataException("El PDF generado está incompleto o no es legible.");
                return validacionPdf;
            }

            return new TesEmisionDocumentosArchivoValidacion
            {
                EsValido = true,
                Paginas = 0,
                Tamano = archivo.Contenido.LongLength,
                Sha256 = Convert.ToHexString(SHA256.HashData(archivo.Contenido))
            };
        }

        private string ResolverDentroRaiz(params string[] segmentos)
        {
            var ruta = Path.GetFullPath(Path.Combine(new[] { _raiz }.Concat(segmentos).ToArray()));
            if (!ruta.StartsWith(_raiz, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La ruta del archivo salió de la raíz controlada.");
            }
            return ruta;
        }

        private void EliminarTemporalSeguro(string temporal)
        {
            var ruta = Path.GetFullPath(temporal);
            if (ruta.StartsWith(_raiz, StringComparison.OrdinalIgnoreCase) && File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }

        private static string AsegurarSeparador(string ruta) =>
            ruta.EndsWith(Path.DirectorySeparatorChar)
                ? ruta
                : ruta + Path.DirectorySeparatorChar;
    }
}
