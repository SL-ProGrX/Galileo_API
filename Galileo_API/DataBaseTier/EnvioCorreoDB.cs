using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

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
            await client.AuthenticateAsync(cfg.User, cfg.Password, ct).ConfigureAwait(false);
            await client.SendAsync(message, ct).ConfigureAwait(false);
            await client.DisconnectAsync(true, ct).ConfigureAwait(false);
        }

        public ErrorDto<EnvioCorreoModels?> CorreoConfig(int codCliente, string codSmtp)
        {
            const string sql = @"
SELECT
    [DESCRIPCION]     AS Providers,
    [USUARIO]         AS [User],
    [CLAVE]           AS [Password],
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
            const string sql = @"
SELECT
    [DESCRIPCION]     AS Providers,
    [USUARIO]         AS [User],
    [CLAVE]           AS [Password],
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
    }
}