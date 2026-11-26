using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Dapper;
using System.Data;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class EnvioCorreoDB
    {
        private readonly IConfiguration _config;
        private readonly PortalDB _portalDb;
        private readonly string _produccion;
        private readonly string _testEmail;

        public EnvioCorreoDB(IConfiguration config)
        {
            _config = config;
            _portalDb = new PortalDB(_config);

            _produccion = _config.GetSection("AppSettings").GetSection("Produccion").Value ?? string.Empty;
            _testEmail  = _config.GetSection("AppSettings").GetSection("TestEmail").Value ?? string.Empty;
        }

        public async Task SendEmailAsync(
            EmailRequest emailRequest,
            EnvioCorreoModels eConfig,
            ErrorDto response,
            CancellationToken ct = default)
        {
            response.Code = 0;

            try
            {
                if (_produccion == "N" && !string.IsNullOrWhiteSpace(_testEmail))
                    emailRequest.To = _testEmail;

                var message = await BuildMessageAsync(emailRequest, ct).ConfigureAwait(false);
                await SendWithSmtpAsync(message, eConfig, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
        }

        public async Task<ErrorDto> QueueEmailAsync(
            string codSmtp,
            EmailRequest emailRequest,
            CancellationToken ct = default)
        {
            var response = new ErrorDto();
            var normalizedSmtp = (codSmtp ?? string.Empty).Trim();
            var recipient = emailRequest.To?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedSmtp) || string.IsNullOrWhiteSpace(recipient))
            {
                response.Code = -1;
                response.Description = "El código SMTP y el destinatario son requeridos para encolar el correo.";
                return response;
            }

            var connectionString = _config.GetConnectionString("BaseConnString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                response.Code = -1;
                response.Description = "BaseConnString no está configurada para encolar el correo.";
                return response;
            }

            const string sql = @"
INSERT INTO [dbo].[SYS_MAIL_SERVICE]
(
    [COD_SMTP], [PARA], [CC], [CUERPO], [ASUNTO], [PRIORIDAD], [ESTADO], [FECHA], [FECHA_ENVIO]
)
VALUES
(
    @CodSmtp, @Para, @Cc, @Cuerpo, @Asunto, 0, 'P', GETDATE(), NULL
);
SELECT CONVERT(int, @@ROWCOUNT);";

            try
            {
                if (_produccion == "N" && !string.IsNullOrWhiteSpace(_testEmail))
                    recipient = _testEmail;

                await using var connection = new SqlConnection(connectionString);
                var inserted = await connection.QuerySingleAsync<int>(new CommandDefinition(
                    sql,
                    new
                    {
                        CodSmtp = normalizedSmtp,
                        Para = recipient,
                        Cc = emailRequest.CopyHide ?? string.Empty,
                        Cuerpo = emailRequest.Body ?? string.Empty,
                        Asunto = emailRequest.Subject ?? string.Empty
                    },
                    cancellationToken: ct)).ConfigureAwait(false);

                response.Code = inserted == 1 ? 0 : -1;
                response.Description = inserted == 1
                    ? "Correo agregado a la cola de envío."
                    : $"No fue posible agregar el correo a SYS_MAIL_SERVICE. Filas insertadas: {inserted}.";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Encola un correo con la plantilla de SYS_NOTIFICACIONES_PLANTILLAS usando spCpr_EncolarCorreos
        /// (base de la empresa). Los valores de <paramref name="textos"/> se escapan en el procedimiento;
        /// los de <paramref name="html"/> (PRODUCTOS, TERMINOS) se insertan tal cual.
        /// Si Produccion = N, el destinatario se redirige a TestEmail.
        /// </summary>
        public Task<ErrorDto> QueuePlantillaAsync(
            int codEmpresa,
            string codSmtp,
            string codPlantilla,
            string para,
            IReadOnlyDictionary<string, string> textos,
            IReadOnlyDictionary<string, string>? html = null,
            string usuario = "Sistema")
        {
            var destino = _produccion == "N" && !string.IsNullOrWhiteSpace(_testEmail) ? _testEmail : para;
            return EncolarPlantillaAsync(codEmpresa, codSmtp, codPlantilla, destino, textos, html, usuario);
        }

        /// <summary>
        /// Igual que <see cref="QueuePlantillaAsync"/>, pero el destinatario es siempre
        /// <paramref name="para"/>, sin redirigir a TestEmail.
        /// </summary>
        public Task<ErrorDto> QueuePlantillaDestinoRealAsync(
            int codEmpresa,
            string codSmtp,
            string codPlantilla,
            string para,
            IReadOnlyDictionary<string, string> textos,
            string usuario) =>
            EncolarPlantillaAsync(codEmpresa, codSmtp, codPlantilla, para, textos, null, usuario);

        private async Task<ErrorDto> EncolarPlantillaAsync(
            int codEmpresa,
            string codSmtp,
            string codPlantilla,
            string destino,
            IReadOnlyDictionary<string, string> textos,
            IReadOnlyDictionary<string, string>? html,
            string usuario)
        {
            var bloques = html ?? new Dictionary<string, string>();

            try
            {
                await using var connection = _portalDb.CreateConnection(codEmpresa);
                var respuesta = await connection.QueryFirstOrDefaultAsync<RespuestaEncolado>(new CommandDefinition(
                    "dbo.spCpr_EncolarCorreos",
                    new
                    {
                        COD_PLANTILLA = codPlantilla,
                        COD_SMTP = codSmtp,
                        PARA = destino,
                        CC = (string?)null,
                        USUARIO = usuario,
                        COD_EMPRESA = (short)codEmpresa,
                        DATOS = ConstruirDatosXml(textos),
                        PRODUCTOS_HTML = bloques.GetValueOrDefault("PRODUCTOS"),
                        TERMINOS_HTML = bloques.GetValueOrDefault("TERMINOS"),
                    },
                    commandType: CommandType.StoredProcedure));

                if (respuesta is null)
                    return new ErrorDto { Code = -1, Description = "spCpr_EncolarCorreos no devolvió resultado." };

                return new ErrorDto { Code = respuesta.Code, Description = respuesta.Description };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Registra en estado P un correo con un adjunto en SYS_MAIL_SERVICE_ADJUNTOS, sin enviarlo,
        /// usando spSys_CORREO_POOL_ADD (base de la empresa). Sirve para cualquier módulo.
        /// PAG_DIVISA = ADJ_B64 distingue estos correos de los pagarés; PAG_SECCION_1 lleva el archivo
        /// en base64 y PAG_SECCION_2 su nombre. El HTML admite hasta 4000 caracteres.
        /// El destinatario es siempre el indicado en la solicitud, sin redirigir a TestEmail.
        /// </summary>
        public async Task<ErrorDto> QueueEmailAdjuntoAsync(
            int codEmpresa,
            string codSmtp,
            EmailRequest emailRequest,
            EmailAdjuntoBase64 adjunto,
            string codNotificacion)
        {
            var destino = emailRequest.To?.Trim();

            if (string.IsNullOrWhiteSpace(codSmtp) || string.IsNullOrWhiteSpace(destino))
                return new ErrorDto { Code = -1, Description = "El código SMTP y el destinatario son requeridos para encolar el correo." };

            // CUERPO es varchar(4000) en SYS_MAIL_SERVICE_ADJUNTOS
            if (emailRequest.Body?.Length > 4000)
                return new ErrorDto { Code = -1, Description = "El cuerpo del correo supera los 4000 caracteres." };

            try
            {
                await using var connection = _portalDb.CreateConnection(codEmpresa);
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.spSys_CORREO_POOL_ADD",
                    new
                    {
                        DETALLE = emailRequest.Body,
                        ASUNTO = emailRequest.Subject,
                        ESTADO = "P",
                        PARA = destino,
                        CEDULA = (string?)null,
                        NOMBRE = (string?)null,
                        PAG_NUMERO = (int?)null,
                        PAG_MONTO = (decimal?)null,
                        PAG_DIVISA = "ADJ_B64",
                        PAG_SECCION1 = adjunto.ContenidoBase64,
                        PAG_SECCION2 = adjunto.NombreArchivo,
                        PAG_SECCION3 = (string?)null,
                        Copia = emailRequest.CopyHide ?? string.Empty,
                        Usuario = "Sistema",
                        NotiCfg = codNotificacion,
                        SMTP = codSmtp.Trim(),
                    },
                    commandType: CommandType.StoredProcedure)).ConfigureAwait(false);

                return new ErrorDto { Code = 0, Description = "Correo agregado a la cola de envío." };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>Archivo adjunto de un correo encolado.</summary>
        public sealed record EmailAdjuntoBase64(string NombreArchivo, string ContenidoBase64);

        public async Task<string?> ObtenerCuerpoPlantillaAsync(int codEmpresa, string codPlantilla, CancellationToken ct = default)
        {
            var plantilla = await ObtenerPlantillaAsync(codEmpresa, codPlantilla, ct).ConfigureAwait(false);
            return plantilla?.Cuerpo;
        }

        public async Task<(string Asunto, string Cuerpo)?> ObtenerPlantillaAsync(int codEmpresa, string codPlantilla, CancellationToken ct = default)
        {
            const string sql = @"
SELECT ASUNTO AS Asunto, CAST(NOTIFICACION AS nvarchar(max)) AS Cuerpo
FROM SYS_NOTIFICACIONES_PLANTILLAS
WHERE COD_PLANTILLA = @CodPlantilla AND ACTIVO = 1;";

            // Las plantillas están en la base de la empresa (misma conexión que usa el resto del módulo).
            await using var connection = _portalDb.CreateConnection(codEmpresa);
            var fila = await connection.QueryFirstOrDefaultAsync<PlantillaFila>(new CommandDefinition(
                sql,
                new { CodPlantilla = codPlantilla },
                cancellationToken: ct)).ConfigureAwait(false);

            return fila is null ? null : (fila.Asunto ?? string.Empty, fila.Cuerpo ?? string.Empty);
        }

        private static string ConstruirDatosXml(IReadOnlyDictionary<string, string> textos) =>
            new XElement("d", textos.Select(kv => new XElement("k", new XAttribute("n", kv.Key), kv.Value)))
                .ToString(SaveOptions.DisableFormatting);

        private sealed class RespuestaEncolado
        {
            public int Code { get; set; }
            public string Description { get; set; } = string.Empty;
        }

        private sealed class PlantillaFila
        {
            public string? Asunto { get; set; }
            public string? Cuerpo { get; set; }
        }

        private static async Task<MimeMessage> BuildMessageAsync(EmailRequest req, CancellationToken ct)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("no-reply", req.From));
            message.To.Add(new MailboxAddress("Destinatario", req.To));
            message.Subject = req.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = req.Body, TextBody = req.Body };

            if (!string.IsNullOrWhiteSpace(req.CopyHide))
                message.Bcc.Add(new MailboxAddress("Copia Oculta", req.CopyHide));

            if (req.Attachments is not null && req.Attachments.Count > 0)
            {
                foreach (var attachment in req.Attachments)
                {
                    await using var ms = new MemoryStream();
                    await attachment.CopyToAsync(ms, ct).ConfigureAwait(false);
                    bodyBuilder.Attachments.Add(attachment.FileName, ms.ToArray());
                }
            }

            message.Body = bodyBuilder.ToMessageBody();
            return message;
        }

        private static async Task SendWithSmtpAsync(MimeMessage message, EnvioCorreoModels cfg, CancellationToken ct)
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(cfg.Host, cfg.Port, SecureSocketOptions.Auto, ct).ConfigureAwait(false);
            await client.AuthenticateAsync(cfg.User, cfg.Secret, ct).ConfigureAwait(false);
            await client.SendAsync(message, ct).ConfigureAwait(false);
            await client.DisconnectAsync(true, ct).ConfigureAwait(false);
        }

        public ErrorDto<EnvioCorreoModels?> CorreoConfig(int codCliente, string codSmtp)
        {

            codSmtp = NormalizeRequiredText(codSmtp, nameof(codSmtp), 20);

            const string sql = @"
SELECT
    [DESCRIPCION]     AS Providers,
    [USUARIO]         AS [User],
    [CLAVE]           AS [Secret],
    [PUERTO_SMTP]     AS [Port],
    [SERVIDOR_CORREO] AS Host,
    [CERTIFICADO]     AS EnableSsl,
    COD_SMTP          AS cod_smtp
FROM SYS_MAIL_CONF_SMTP
WHERE [ESTADO] = 1 AND COD_SMTP = @CodSmtp;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codCliente,
                sql,
                defaultValue: new EnvioCorreoModels(),
                parameters: new { CodSmtp = codSmtp }
            );
        }

        public ErrorDto<EnvioCorreoModels?> CorreoConfigCuenta(string smtpcode)
        {

            smtpcode = NormalizeRequiredText(smtpcode, nameof(smtpcode), 20);

            const string sql = @"
SELECT
    [DESCRIPCION]     AS Providers,
    [USUARIO]         AS [User],
    [CLAVE]           AS [Secret],
    [PUERTO_SMTP]     AS [Port],
    [SERVIDOR_CORREO] AS Host,
    [CERTIFICADO]     AS EnableSsl,
    COD_SMTP          AS cod_smtp
FROM SYS_MAIL_CONF_SMTP
WHERE [COD_SMTP] = @SmtpCode;";

            var baseConn = _config.GetConnectionString("BaseConnString");
            if (string.IsNullOrWhiteSpace(baseConn))
            {
                return new ErrorDto<EnvioCorreoModels?>
                {
                    Code = -1,
                    Description = "BaseConnString is not configured.",
                    Result = null
                };
            }
            return DbHelper.ExecuteSingleQuery(
                baseConn,
                sql,
                defaultValue: new EnvioCorreoModels(),
                parameters: new { SmtpCode = smtpcode }
            );
        }

        public ErrorDto<AfiBeneDatosCorreo?> BuscoDatosSocioBeneficio(int codCliente, string cedula, string beneficio)
        {
            cedula = NormalizeRequiredText(cedula, nameof(cedula), 30);
            beneficio = NormalizeRequiredText(beneficio, nameof(beneficio), 20);

            const string sql = @"
SELECT
    [NOMBREV2] + ' ' + [APELLIDO1] + ' ' + [APELLIDO2] AS nombre,
    [CEDULA] AS cedula,
    [AF_EMAIL] AS email,
    (
        SELECT [DESCRIPCION]
        FROM [dbo].[AFI_BENEFICIOS]
        WHERE COD_BENEFICIO = @Beneficio
    ) AS beneficio
FROM SOCIOS
WHERE CEDULA = @Cedula;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codCliente,
                sql,
                defaultValue: new AfiBeneDatosCorreo(),
                parameters: new { Cedula = cedula, Beneficio = beneficio }
            );
        }

        public ErrorDto BitacoraCorreo(int codEmpresa, EnvioCorreoModels smtp, MimeMessage email)
        {
            const string sql = @"
INSERT INTO [dbo].[SYS_MAIL_SERVICE]
(
    [COD_SMTP],
    [PARA],
    [CC],
    [CUERPO],
    [ASUNTO],
    [PRIORIDAD],
    [ESTADO],
    [FECHA],
    [FECHA_ENVIO]
)
VALUES
(
    @CodSmtp,
    @Para,
    @Cc,
    @Cuerpo,
    @Asunto,
    0,
    'E',
    GETDATE(),
    GETDATE()
);";

            var toEmail = email.To.ToString()
                .Replace("Destinatario", "")
                .Replace("<", "")
                .Replace(">", "")
                .Trim()
                .Replace("\"\" ", "");

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodSmtp = smtp.cod_smtp,
                    Para = toEmail,
                    Cc = email.Cc.ToString(),
                    Cuerpo = email.Body?.ToString(),
                    Asunto = email.Subject
                }
            );
        }

        private static string NormalizeRequiredText(string? value, string paramName, int maxLength = 100)
        {
            var normalized = (value ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException($"{paramName} es requerido.", paramName);

            if (normalized.Length > maxLength)
                throw new ArgumentException($"{paramName} no es válido.", paramName);

            return normalized;
        }

    }
}
