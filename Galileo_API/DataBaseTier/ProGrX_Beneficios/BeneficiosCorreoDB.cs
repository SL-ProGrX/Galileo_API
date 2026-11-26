using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>Códigos de SYS_NOTIFICACIONES_PLANTILLAS usados por Beneficios Solidarios.</summary>
    public static class BeneficiosPlantillas
    {
        public const string Solicitud = "BEN_SOL01";
        public const string SolicitudPsicologia = "BEN_SOL02";
        public const string Resolucion = "BEN_RES01";
        public const string CobroMora = "BEN_MORA01";
        public const string SolicitudBloqueo = "BEN_BLQ01";
        public const string RecargaTarjetas = "BEN_TAR01";
        public const string Deposito = "BEN_DEP01";
    }

    /// <summary>Archivo adjunto de un correo de Beneficios.</summary>
    public sealed record AdjuntoBeneficio(string Nombre, string ContenidoBase64);

    /// <summary>Datos de un correo de Beneficios que se va a encolar.</summary>
    public sealed record CorreoBeneficio
    {
        public required int CodEmpresa { get; init; }
        public required string CodPlantilla { get; init; }
        public required string CodSmtp { get; init; }
        public required string Para { get; init; }
        public string Usuario { get; init; } = "Sistema";
        public string Cc { get; init; } = string.Empty;
        public IReadOnlyDictionary<string, string> Datos { get; init; } = new Dictionary<string, string>();
        public IReadOnlyList<AdjuntoBeneficio> Adjuntos { get; init; } = [];
    }

    /// <summary>
    /// Registra los correos de Beneficios en la cola de envío; los dispara un servicio aparte.
    /// Sin adjuntos usa spCpr_EncolarCorreos (SYS_MAIL_SERVICE). Con adjuntos usa el mismo método de
    /// Compras (spSys_CORREO_POOL_ADD, SYS_MAIL_SERVICE_ADJUNTOS), que admite un solo archivo por correo:
    /// cuando llegan varios PDF se unen en uno.
    /// </summary>
    public class BeneficiosCorreoDB
    {
        private const int LargoAsunto = 100;
        private const int LargoNombreArchivo = 100;
        private const int LargoCodNotificacion = 10;

        private readonly IConfiguration _config;
        private readonly EnvioCorreoDB _envioCorreoDB;

        public BeneficiosCorreoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _envioCorreoDB = new EnvioCorreoDB(_config);
        }

        /// <summary>Cuenta SMTP configurada en la categoría del beneficio.</summary>
        public string CodSmtpPorBeneficio(int codEmpresa, string codBeneficio)
        {
            const string sql = @"
                SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                INNER JOIN AFI_BENEFICIOS B ON B.COD_CATEGORIA = C.COD_CATEGORIA
                WHERE B.COD_BENEFICIO = @codBeneficio";

            return ObtenerCodSmtp(codEmpresa, sql, new { codBeneficio });
        }

        /// <summary>Cuenta SMTP configurada en la categoría indicada.</summary>
        public string CodSmtpPorCategoria(int codEmpresa, string codCategoria)
        {
            const string sql = "SELECT COD_SMTP FROM AFI_BENE_CATEGORIAS WHERE COD_CATEGORIA = @codCategoria";

            return ObtenerCodSmtp(codEmpresa, sql, new { codCategoria });
        }

        private string ObtenerCodSmtp(int codEmpresa, string sql, object parametros)
        {
            var result = DbHelper.ExecuteSingleQuery<string>(new PortalDB(_config), codEmpresa, sql, null, parametros);
            return result.Result?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Encola el correo en estado P con el destinatario real. No depende de AppSettings:EnviaEmail
        /// ni redirige a TestEmail: el envío lo controla el servicio de correo.
        /// </summary>
        public async Task<ErrorDto> EncolarAsync(CorreoBeneficio correo)
        {
            if (string.IsNullOrWhiteSpace(correo.CodSmtp))
            {
                return Error("La categoría del beneficio no tiene una cuenta de correo configurada.");
            }

            if (string.IsNullOrWhiteSpace(correo.Para))
            {
                return Error("No hay un correo electrónico de destino.");
            }

            return correo.Adjuntos.Count == 0
                ? await _envioCorreoDB.QueuePlantillaDestinoRealAsync(
                    correo.CodEmpresa, correo.CodSmtp, correo.CodPlantilla, correo.Para.Trim(), correo.Datos, correo.Usuario)
                : await EncolarConAdjuntosAsync(correo);
        }

        private async Task<ErrorDto> EncolarConAdjuntosAsync(CorreoBeneficio correo)
        {
            var noPdf = correo.Adjuntos.FirstOrDefault(a => !EsPdf(a.ContenidoBase64));
            if (noPdf != null)
            {
                return Error($"Solo se pueden enviar archivos PDF. Revise el archivo: {noPdf.Nombre}");
            }

            var plantilla = await _envioCorreoDB.ObtenerPlantillaAsync(correo.CodEmpresa, correo.CodPlantilla);
            if (plantilla == null)
            {
                return Error($"No existe la plantilla activa {correo.CodPlantilla} en SYS_NOTIFICACIONES_PLANTILLAS.");
            }

            AdjuntoBeneficio adjunto;
            try
            {
                adjunto = UnirAdjuntos(correo);
            }
            catch (Exception ex)
            {
                return Error("No fue posible unir los archivos PDF (revise que no estén protegidos o dañados): " + ex.Message);
            }

            var email = new EmailRequest
            {
                To = correo.Para.Trim(),
                CopyHide = correo.Cc,
                Subject = Recortar(Reemplazar(plantilla.Value.Asunto, correo.Datos, valor => valor), LargoAsunto),
                Body = Reemplazar(plantilla.Value.Cuerpo, correo.Datos, EscaparHtml)
            };

            return await _envioCorreoDB.QueueEmailAdjuntoAsync(
                correo.CodEmpresa,
                correo.CodSmtp,
                email,
                new EnvioCorreoDB.EmailAdjuntoBase64(RecortarNombre(adjunto.Nombre), adjunto.ContenidoBase64),
                Recortar(correo.CodPlantilla, LargoCodNotificacion));
        }

        /// <summary>Devuelve el único archivo o, si son varios, un PDF con todos en el orden recibido.</summary>
        private static AdjuntoBeneficio UnirAdjuntos(CorreoBeneficio correo)
        {
            if (correo.Adjuntos.Count == 1)
            {
                return correo.Adjuntos[0];
            }

            var pdfs = correo.Adjuntos.Select(a => Convert.FromBase64String(a.ContenidoBase64)).ToArray();
            var unido = MProGrXAuxiliarDB.CombinarBytesPdfSharp(pdfs);

            return new AdjuntoBeneficio($"Documentos_{correo.CodPlantilla}.pdf", Convert.ToBase64String(unido));
        }

        // Mismo escape que aplica spCpr_EncolarCorreos; el cuerpo de esta cola admite hasta 4000 caracteres.
        private static string EscaparHtml(string valor) => valor
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");

        private static ErrorDto Error(string mensaje) => new() { Code = -1, Description = mensaje };

        // "JVBER" es el inicio en base64 de la firma "%PDF".
        private static bool EsPdf(string contenidoBase64) =>
            contenidoBase64.StartsWith("JVBER", StringComparison.Ordinal);

        private static string Reemplazar(string texto, IReadOnlyDictionary<string, string> datos, Func<string, string> codificar) =>
            datos.Aggregate(texto, (actual, dato) => actual.Replace($"[[{dato.Key}]]", codificar(dato.Value ?? string.Empty)));

        private static string Recortar(string? valor, int largo)
        {
            var texto = valor ?? string.Empty;
            return texto.Length <= largo ? texto : texto[..largo];
        }

        private static string RecortarNombre(string nombre)
        {
            var extension = Path.GetExtension(nombre);
            return Recortar(Path.GetFileNameWithoutExtension(nombre), LargoNombreArchivo - extension.Length) + extension;
        }
    }
}
