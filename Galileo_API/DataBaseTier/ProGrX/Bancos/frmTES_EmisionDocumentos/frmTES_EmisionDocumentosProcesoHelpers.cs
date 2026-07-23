using Galileo.Models.TES;
using PdfSharp.Pdf.IO;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos.frmTES_EmisionDocumentos
{
    public static class TesEmisionDocumentosEstado
    {
        public const string Pendiente = "Pendiente";
        public const string Preparando = "Preparando";
        public const string Generando = "Generando";
        public const string Validando = "Validando";
        public const string Completado = "Completado";
        public const string Error = "Error";
        public const string RequiereRevision = "RequiereRevision";
        public const string Procesando = "Procesando";
        public const string Finalizando = "Finalizando";
        public const string CompletadoConErrores = "CompletadoConErrores";

        private static readonly HashSet<(string Actual, string Siguiente)> Transiciones = new()
        {
            (Pendiente, Preparando),
            (Preparando, Generando),
            (Generando, Validando),
            (Validando, Completado),
            (Pendiente, Error),
            (Preparando, Error),
            (Generando, Error),
            (Validando, Error),
            (Generando, RequiereRevision),
            (Validando, RequiereRevision)
        };

        public static bool PuedeCambiar(string actual, string siguiente) =>
            Transiciones.Contains((actual, siguiente));

        public static bool EsActivo(string estado) =>
            estado is Pendiente or Procesando or Finalizando;
    }

    public static class TesEmisionDocumentosNumeracion
    {
        public static string CrearNDocumento(
            long documentoBase,
            int secuencia)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(documentoBase);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(secuencia);
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{documentoBase}-{secuencia:D3}");
        }

        public static string? CrearNDocumentoOpcional(
            long documentoBase,
            int secuencia) =>
            documentoBase > 0 && secuencia > 0
                ? CrearNDocumento(documentoBase, secuencia)
                : null;
    }

    public static class TesEmisionDocumentosProcesoHash
    {
        /// <summary>
        /// Calcula una huella estable del propietario y los filtros funcionales.
        /// </summary>
        public static string Crear(
            int codEmpresa,
            string propietario,
            TesEmisionDocFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(filtros);

            var contenido = new
            {
                codEmpresa,
                propietario = Normalizar(propietario),
                filtros.cantidad,
                filtros.banco,
                plan = Normalizar(filtros.plan),
                filtros.docInicial,
                generarPor = Normalizar(filtros.generarPor),
                tipoDoc = Normalizar(filtros.tipoDoc),
                filtros.minimo,
                filtros.maximo,
                filtros.verificacion,
                filtros.fecha_inicio,
                filtros.fecha_corte,
                formatoTE = Normalizar(filtros.formatoTE),
                filtros.docBloqueo,
                filtros.especial
            };

            var bytes = JsonSerializer.SerializeToUtf8Bytes(contenido);
            return Convert.ToHexString(SHA256.HashData(bytes));
        }

        private static string Normalizar(string? valor) =>
            (valor ?? string.Empty).Trim().ToUpperInvariant();
    }

    public static class TesEmisionDocumentosArchivoValidador
    {
        /// <summary>
        /// Comprueba que el contenido sea un PDF completo y legible.
        /// </summary>
        public static TesEmisionDocumentosArchivoValidacion ValidarPdf(byte[] contenido)
        {
            if (contenido == null || contenido.Length == 0)
            {
                return new TesEmisionDocumentosArchivoValidacion();
            }

            try
            {
                using var stream = new MemoryStream(contenido, writable: false);
                using var documento = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                var paginas = documento.PageCount;

                return new TesEmisionDocumentosArchivoValidacion
                {
                    EsValido = paginas > 0,
                    Paginas = paginas,
                    Tamano = contenido.LongLength,
                    Sha256 = Convert.ToHexString(SHA256.HashData(contenido))
                };
            }
            catch (Exception)
            {
                return new TesEmisionDocumentosArchivoValidacion();
            }
        }
    }
}
