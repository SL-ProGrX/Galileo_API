using Dapper;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Data.SqlClient;
using MimeKit;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Net;

namespace PgxAPI.DataBaseTier
{
    public class EnvioCorreoDB
    {
        private readonly IConfiguration _config;
        private string produccion;
        private string TestEmail;

        public EnvioCorreoDB(IConfiguration config)
        {
            _config = config;
            produccion = _config.GetSection("AppSettings").GetSection("Produccion").Value.ToString();
            TestEmail = _config.GetSection("AppSettings").GetSection("TestEmail").Value.ToString();
        }

        /// <summary>
        /// Metodo principal para enviar correos [eConfig] es la configuracion del servidor de correo y emailRequest es la información del correo a enviar
        /// </summary>
        /// <param name="emailRequest"></param>
        /// <param name="eConfig"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(EmailRequest emailRequest, EnvioCorreoModels eConfig, ErrorDTO response)
        {
            response.Code = 0;
            try
            {
                if(produccion == "N")
                {
                    emailRequest.To = TestEmail;
                }


                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("no-reply", emailRequest.From));
                message.To.Add(new MailboxAddress("Destinatario", emailRequest.To));
                message.Subject = emailRequest.Subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = emailRequest.Body,
                    TextBody = emailRequest.Body // Por si el cliente de correo no soporta HTML
                };

                // Verificar si hay copias ocultas
                if (!string.IsNullOrEmpty(emailRequest.CopyHide))
                {
                    message.Bcc.Add(new MailboxAddress("Copia Oculta", emailRequest.CopyHide));
                }

                // Verificar si hay archivos adjuntos
                if (emailRequest.Attachments != null && emailRequest.Attachments.Count > 0)
                {
                    foreach (var attachment in emailRequest.Attachments)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await attachment.CopyToAsync(memoryStream);
                            bodyBuilder.Attachments.Add(attachment.FileName, memoryStream.ToArray());
                        }
                    }
                }

                message.Body = bodyBuilder.ToMessageBody();

                
                // Configurar el cliente SMTP
                using (var client = new SmtpClient())
                {
                    ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                    client.Connect(eConfig.Host, eConfig.Port, SecureSocketOptions.Auto);
                    client.Authenticate(eConfig.User, eConfig.Password);

                    await client.SendAsync(message);
                    client.Disconnect(true);

                    //BitacoraCorreo(eConfig, message);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            
        }

        //Busco la configuracion activa del buzon de correo electronico
        public EnvioCorreoModels CorreoConfig(int CodCliente, string cod_smtp)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            EnvioCorreoModels correo = new EnvioCorreoModels();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT 
                               [DESCRIPCION] as Providers
                              ,[USUARIO] as 'User'
                              ,[CLAVE] as 'Password'
                              ,[PUERTO_SMTP] as 'Port'
                              ,[SERVIDOR_CORREO] as Host
                              ,[CERTIFICADO] as EnableSsl
                              ,COD_SMTP
                          FROM SYS_MAIL_CONF_SMTP
                        WHERE [ESTADO] = 1 AND COD_SMTP = '{cod_smtp}' ";

                    correo = connection.Query<EnvioCorreoModels>(query).FirstOrDefault();

                }
            }
            catch (Exception)
            {
                return null;
            }
            return correo;
        }

        /// <summary>
        /// Obtiene la cuenta de correo a utilizar según el smtp código
        /// </summary>
        /// <param name="smtpcode"></param>
        /// <returns></returns>
        public EnvioCorreoModels CorreoConfigCuenta(string smtpcode)
        {

            EnvioCorreoModels correo = new EnvioCorreoModels();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("BaseConnString")))
                {
                    var query = $@"SELECT 
                               [DESCRIPCION] as Providers
                              ,[USUARIO] as 'User'
                              ,[CLAVE] as 'Password'
                              ,[PUERTO_SMTP] as 'Port'
                              ,[SERVIDOR_CORREO] as Host
                              ,[CERTIFICADO] as EnableSsl
                          FROM SYS_MAIL_CONF_SMTP
                        WHERE [COD_SMTP] = '{smtpcode}'";

                    correo = connection.Query<EnvioCorreoModels>(query).FirstOrDefault();

                }
            }
            catch (Exception)
            {
                return null;
            }
            return correo;
        }

        /// <summary>
        /// Busco la información del socio para enviar el correo
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        public AfiBeneDatosCorreo BuscoDatosSocioBeneficio(int CodCliente, string cedula, string beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            AfiBeneDatosCorreo correo = new AfiBeneDatosCorreo();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco el correo del socio
                    var query = $@"SELECT
	                               [NOMBREV2] + ' ' + [APELLIDO1] + ' ' + [APELLIDO2] AS nombre
                                  ,[CEDULA] as cedula
                                  ,[AF_EMAIL] as email
	                              , (
	                                      SELECT 
                                           [DESCRIPCION]
                                      FROM [dbo].[AFI_BENEFICIOS]
                                      WHERE COD_BENEFICIO = '{beneficio}'
	                                      ) AS beneficio
                              FROM SOCIOS
                              WHERE  CEDULA = '{cedula}'";
                    correo = connection.Query<AfiBeneDatosCorreo>(query).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                return null;
            }


            return correo;
        }

        /// <summary>
        /// Escribe en la bitácora el correo enviado E es de enviado, ya que sale directamente del API
        /// </summary>
        /// <param name="smtp"></param>
        /// <param name="email"></param>
        public void BitacoraCorreo(EnvioCorreoModels smtp, MimeMessage email)
        {
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                {
                    var toEmail = email.To.ToString().Replace("Destinatario", "").Replace("<", "").Replace(">","").Trim().Replace("\"\" ", "");

                    var query = $@"INSERT INTO [dbo].[SYS_MAIL_SERVICE]
                                       ([COD_SMTP]
                                       ,[PARA]
                                       ,[CC]
                                       ,[CUERPO]
                                       ,[ASUNTO]
                                       ,[PRIORIDAD]
                                       ,[ESTADO]
                                       ,[FECHA]
                                       ,[FECHA_ENVIO])
                                 VALUES
                                       ('{smtp.cod_smtp}'
                                       ,'{toEmail}'
                                       ,'{email.Cc}'
                                       ,'{email.Body}'
                                       ,'{email.Subject}'
                                       ,0
                                       ,'E'
                                       ,getDate()
                                       ,getDate())";

                    connection.Execute(query);

                }
            }
            catch (Exception)
            {

            }
        }
    }
}
