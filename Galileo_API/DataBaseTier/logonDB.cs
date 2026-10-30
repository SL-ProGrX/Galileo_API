using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using System.Security.Cryptography;

namespace Galileo.DataBaseTier
{
    public class LogonDB
    {

        private readonly IConfiguration _config;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private readonly string sendEmail = "";
        private readonly string Notificaciones = "";
        private const string connectionStringName = "DefaultConnString";

        public LogonDB(IConfiguration config)
        {
            _config = config;
            _envioCorreoDB = new EnvioCorreoDB(_config);
            var enviaEmailValue = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value;
            sendEmail = enviaEmailValue != null
                ? enviaEmailValue.ToString()
                : string.Empty;
            var notificacionesValue = _config.GetSection("AppSettings").GetSection("Notificaciones").Value;
            Notificaciones = notificacionesValue != null ? notificacionesValue.ToString() : string.Empty;
        }

        public IntentosObtenerDto? IntentosObtener()
        {
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            const string sql = "select KEY_INTENTOS,TIME_LOCK from US_PARAMETROS";
            return connection.QueryFirstOrDefault<IntentosObtenerDto>(sql);
        }

        public ErrorDto LoginObtener(LoginObtenerDto req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                int existe = connection.Query<int>("spSEG_Logon", req, commandType: CommandType.StoredProcedure).FirstOrDefault();

                if (existe == 0)
                {
                    resp.Code = 2;
                    resp.Description = "El Usuario o Contraseña no fueron encontrados, verifique...";
                }
                else if (existe == 1)
                {
                    resp.Code = 0;
                    resp.Description = "ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = 1;
                resp.Description =$"[LoginObtener] Usuario={req.Usuario} Error: {ex.Message}";
            }
            return resp;
        }

        public ErrorDto<List<ClientesEmpresasObtenerDto>> ClientesObtener(string usuario)
        {
            var response = new ErrorDto<List<ClientesEmpresasObtenerDto>>()
            {
                Result = new List<ClientesEmpresasObtenerDto>()
            };

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var values = new
                    {
                        Usuario = usuario
                    };

                    response.Result = connection.Query<ClientesEmpresasObtenerDto>("spPGX_Usuario_Consultar_Clientes", values, commandType: CommandType.StoredProcedure)
                        .Where(empresa => string.Equals(empresa.usuario?.Trim(), usuario.Trim(), StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public int ValidarDatos(string usuario, string email)
        {
            int Result = 0;

            string sql = "SELECT dbo.fxSEG_Token_Valida_Usuario( @usuario, @email, '' )";
            var values = new
            {
                usuario = usuario,
                email = email
            };

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                    Result = connection.Query<int>(sql, values).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return Result;

        }

        public int ValidarToken(string usuario, string token)
        {
            int Result = 0;

            string sql = "SELECT dbo.fxSEG_Token_Valida( @usuario, @token )";
            var values = new
            {
                usuario = usuario,
                token = token
            };

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                    Result = connection.Query<int>(sql, values).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return Result;

        }

        public int EnviarToken(string usuario, string token, string tokenKey)
        {
            int Result = 0;

            var values = new
            {
                Usuario = usuario,
                Token = token,
                TokenKey = tokenKey
            };

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                    Result = connection.Query<int>("spSEG_Token_Registra", values, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }

            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return Result;

        }

        public TfaData TFA_Data_Load(string usuario)
        {
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            return connection.QueryFirstOrDefault<TfaData?>(
                "[sp2FA_Usuario_Cfg]",
                new { Usuario = usuario },
                commandType: CommandType.StoredProcedure) ?? new TfaData();
        }

        public async Task<ErrorDto> TFA_Codigo_EnviarMAIL(
            string usuario,
            string email,
            string nombreAplicacion)
        {
            var resp = new ErrorDto();

            if (sendEmail != "Y")
            {
                resp.Code = -1;
                resp.Description = "El envío de correo para 2FA no está habilitado.";
                return resp;
            }

            try
            {
                var codigo2FA = Generate2FACode();
                await using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    resp.Code = (await connection.QueryAsync<int>(
                        "[sp2FA_Usuario_Codigo]",
                        new
                        {
                            Usuario = usuario,
                            Codigo = codigo2FA
                        },
                        commandType: CommandType.StoredProcedure)).FirstOrDefault();
                    resp.Description = "Ok";
                }

                var datos = new TfaDatosCorreo
                {
                    codigo = codigo2FA,
                    email = email
                };
                return await TfaCodigoEmail_Enviar(datos, nombreAplicacion);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                return resp;
            }
        }

        public ErrorDto TFA_Codigo_Validar(string usuario, string codigo)
        {
            ErrorDto resp = new ErrorDto();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var procedure = "[sp2FA_Usuario_Codigo_Valida]";
                    var values = new
                    {
                        Usuario = usuario,
                        Codigo = codigo
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public static string Generate2FACode()
        {
            var code = RandomNumberGenerator.GetInt32(100000, 1000000);
            return code.ToString("D6");
        }

        private async Task<ErrorDto> TfaCodigoEmail_Enviar(
            TfaDatosCorreo datos,
            string nombreAplicacion)
        {
            string body = $$$"""
                <!doctype html>
                <html lang="es">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <meta http-equiv="X-UA-Compatible" content="IE=edge">
                    <meta name="color-scheme" content="light dark">
                    <meta name="supported-color-schemes" content="light dark">
                    <title>Código de verificación {{{nombreAplicacion}}}</title>
                    <style>
                        .logo-dark, .system-logo-dark { display:none !important; max-height:0; overflow:hidden; }
                        @media (prefers-color-scheme: dark) {
                            .email-body { background-color:#111827 !important; color:#e5e7eb !important; }
                            .email-card, .email-header { background-color:#1f2937 !important; border-color:#374151 !important; }
                            .email-copy, .email-code-label { color:#cbd5e1 !important; }
                            .email-code-surface { background-color:#26364a !important; border-color:#40546d !important; }
                            .email-code { color:#ffffff !important; }
                            .email-footer { background-color:#17212f !important; border-color:#374151 !important; }
                            .system-logo-light { display:none !important; max-height:0 !important; }
                            .system-logo-dark { display:block !important; max-height:none !important; }
                            .logo-light { display:none !important; max-height:0 !important; }
                            .logo-dark { display:block !important; max-height:none !important; }
                        }
                    </style>
                </head>
                <body class="email-body" style="margin:0; padding:0; background-color:#f3f5f8; font-family:Arial, Helvetica, sans-serif; color:#243247;">
                    <div style="display:none; max-height:0; overflow:hidden; opacity:0; color:transparent;">
                        Tu código para completar el inicio de sesión en {{{nombreAplicacion}}}.
                    </div>
                    <table role="presentation" width="100%" border="0" cellspacing="0" cellpadding="0" style="width:100%; background-color:#f3f5f8;">
                        <tr>
                            <td align="center" style="padding:32px 12px;">
                                <table class="email-card" role="presentation" width="600" border="0" cellspacing="0" cellpadding="0" style="width:100%; max-width:600px; background-color:#ffffff; border:1px solid #dce3ec; border-radius:12px; overflow:hidden;">
                                    <tr>
                                        <td class="email-header" align="center" style="padding:28px 24px 22px; background-color:#ffffff;">
                                            <img class="logo-light" src="https://www.progrxweb.com/images/progrx-logo.png" width="210" alt="ProGrX" style="display:block; width:210px; max-width:100%; height:auto; border:0;">
                                            <img class="logo-dark" src="https://www.progrxweb.com/images/progrx-small.png" width="150" alt="ProGrX" style="display:none; width:150px; max-width:100%; height:auto; border:0;">
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="center" style="padding:17px 24px; background-color:#244d78;">
                                            <p style="margin:0; color:#ffffff; font-size:12px; font-weight:bold; letter-spacing:1.5px; text-transform:uppercase;">{{{nombreAplicacion}}} · Verificación de acceso</p>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="padding:32px 36px 12px;">
                                            <h1 style="margin:0 0 14px; color:#1f2937; font-size:23px; line-height:1.3; font-weight:700;">Verificación en dos pasos</h1>
                                            <p class="email-copy" style="margin:0; color:#526174; font-size:15px; line-height:1.6;">Se solicitó un código para completar el inicio de sesión en {{{nombreAplicacion}}}. Ingresa este código en la pantalla de verificación:</p>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="center" style="padding:20px 36px 22px;">
                                            <table class="email-code-surface" role="presentation" border="0" cellspacing="0" cellpadding="0" style="width:100%; background-color:#f1f6fc; border:1px solid #d7e4f2; border-radius:8px;">
                                                <tr>
                                                    <td align="center" style="padding:18px 12px;">
                                                        <p class="email-code-label" style="margin:0 0 8px; color:#526174; font-size:12px; font-weight:bold; letter-spacing:1px; text-transform:uppercase;">Código de verificación</p>
                                                        <p class="email-code" style="margin:0; color:#173f6b; font-size:34px; line-height:1.2; font-weight:bold; letter-spacing:8px;">{{{datos.codigo}}}</p>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="padding:0 36px 30px;">
                                            <p class="email-copy" style="margin:0 0 10px; color:#526174; font-size:13px; line-height:1.6;">Selecciona y copia el código. Al pegarlo en la aplicación, la verificación comienza automáticamente.</p>
                                            <p class="email-copy" style="margin:0; color:#526174; font-size:13px; line-height:1.6;">Por seguridad, no compartas este código con nadie. Si no solicitaste iniciar sesión, puedes ignorar este correo.</p>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="email-footer" align="center" style="padding:18px 24px; background-color:#f7f8fa; border-top:1px solid #e5e9ef;">
                                            <table role="presentation" border="0" cellspacing="0" cellpadding="0" style="margin:0 auto;"><tr><td align="right" valign="middle" style="padding:0 4px 0 0;"><img class="system-logo-light" src="https://www.progrxweb.com/images/system-logic.png" width="78" alt="System Logic" style="display:block; width:78px; max-width:100%; height:auto; border:0;"><img class="system-logo-dark" src="https://www.progrxweb.com/images/system-logic-white.png" width="78" alt="System Logic" style="display:none; width:78px; max-width:100%; height:auto; border:0;"></td><td valign="middle" style="padding:0 0 0 4px; color:#738096; font-size:12px; line-height:1.5;">ProGrX &copy; {{{DateTime.Now.Year}}}</td></tr></table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
                """;

            var emailRequest = new EmailRequest
            {
                To = datos.email,
                Subject = $"Código de verificación para {nombreAplicacion}",
                Body = body
            };

            return await _envioCorreoDB.QueueEmailAsync(Notificaciones, emailRequest);
        }
    }
}
