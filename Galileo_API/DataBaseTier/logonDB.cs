using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class LogonDB
    {

        private readonly IConfiguration _config;
        private readonly EnvioCorreoDB _envioCorreoDB;
        public string sendEmail = "";
        public string Notificaciones = "";

        public LogonDB(IConfiguration config)
        {
            _config = config;
            _envioCorreoDB = new EnvioCorreoDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            Notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value.ToString();
        }

        public IntentosObtenerDto IntentosObtener()
        {
            IntentosObtenerDto resp = new IntentosObtenerDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {

                    var strSQL = "select KEY_INTENTOS,TIME_LOCK  from US_PARAMETROS";

                    resp = connection.Query<IntentosObtenerDto>(strSQL).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                resp = null;
            }
            return resp;
        }

        public ErrorDto LoginObtener(LoginObtenerDto req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
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
            }
            catch (Exception)
            {
                resp.Code = 1;
                resp.Description = "No se pudo establecer la conexión con el servidor de la Aplicación...";
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
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var values = new
                    {
                        Usuario = usuario
                    };

                    response.Result = connection.Query<ClientesEmpresasObtenerDto>("spPGX_Usuario_Consultar_Clientes", values, commandType: CommandType.StoredProcedure).ToList();
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
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
            TfaData info = new TfaData();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[sp2FA_Usuario_Cfg]";
                    var values = new
                    {
                        Usuario = usuario,

                    };
                    info = connection.QueryFirstOrDefault<TfaData>(procedure, values, commandType: CommandType.StoredProcedure)!;

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public async Task<ErrorDto> TFA_Codigo_EnviarMAIL(string usuario, string email)
        {
            ErrorDto resp = new ErrorDto();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {

                    var codigo2FA = Generate2FACode();
                    var procedure = "[sp2FA_Usuario_Codigo]";
                    var values = new
                    {
                        Usuario = usuario,
                        Codigo = codigo2FA
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";

                    TfaDatosCorreo datos = new TfaDatosCorreo();
                    datos.codigo = codigo2FA;
                    datos.email = email;
                    await TfaCodigoEmail_Enviar(datos);

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto TFA_Codigo_Validar(string usuario, string codigo)
        {
            ErrorDto resp = new ErrorDto();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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

        public string Generate2FACode()
        {
            Random random = new Random();
            int code = random.Next(100000, 1000000); // Generates a 6-digit number
            return code.ToString("D6"); // Formats it as a 6-digit string
        }

        private async Task TfaCodigoEmail_Enviar(TfaDatosCorreo datos)
        {
            ErrorDto response = new ErrorDto();
            EnvioCorreoModels eConfig = _envioCorreoDB.CorreoConfigCuenta(Notificaciones);

            string body = @$"<!DOCTYPE html>
                            <html>
                            <head>
                                <link rel=""preconnect"" href=""https://fonts.gstatic.com"">
                                <link href=""https://fonts.googleapis.com/css2?family=Roboto:wght@300&display=swap"" rel=""stylesheet"">
                                <style>
                                    .b-b{{border-bottom:solid 20px #b00}}.p-20{{padding:20px 20px 20px 20px}}.p-5{{padding:5px}}.p-20-0{{padding:0 20px 20px 20px}}.w-50{{width:50%}}.w-100{{width:100}}
                                </style>
                            </head>
                            <body style=""background-color:#E7E7E7; font-family: 'Roboto', sans-serif;"">
    
                                <table style=""width: 600px; background-color: white;"">
                                    <tbody>
                                        <tr align=""center"" class=""p-20"">
                                            <td>
                                                <img src=""https://cdn.prod.website-files.com/6556c20a6abfe6cb4b3b0f09/656e6869ae3ee6a4e179f28b_SystemLogic_LOGO-04.png"" style=""padding-top: 10px; padding-bottom: 10px;width: 50%; height: auto;"">
                                            </td>
                                        </tr>
            
                                        <tr align=""center"" style=""background-color: #3c3c3c;"" class=""p-20""> 
                                            <td><h3 style=""color: white;"">PGX-SSecurity - 2FA</h3></td>
                                        </tr>
            
                                        <tr style=""background-color: white;""> 
                                            <td class=""p-20"">
                                                <p>Por favor regresa a la aplicación e ingresa el siguiente código para validar el ingreso:</p>
                                            </td>
                                        </tr>
            
                                            <tr align=""center"" class=""p-20""> 
                                                <td>
                                                <strong>
                                                    <span style=""background-color:#f80808; color:white; font-size:24px;"">&nbsp;{datos.codigo}&nbsp;</span>
                                                </strong>
                                                </td>
                                            </tr>
             
                                            <tr align=""center"" class=""p-20""> 
                                            <td class=""p-20"">
                                                <p></p>
                                            </td>
                                            </tr>
            
                                            <tr align=""center"" style=""background-color:#676767;""> 
                                            <td class=""p-20"">
                                                <p style=""color: white;"">MPB System Logic  © {DateTime.Now.Year}</span></p>
                                            </td>
                                        </tr> 
                                    </tbody>
                                </table> 
                            </body>
                            </html>";

            List<IFormFile> Attachments = new List<IFormFile>();

            if (sendEmail == "Y")
            {
                EmailRequest emailRequest = new EmailRequest();

                emailRequest.To = datos.email;
                emailRequest.From = eConfig.User;
                emailRequest.Subject = "System Logic Código de Verificación";
                emailRequest.Body = body;
                emailRequest.Attachments = Attachments;

                if (eConfig != null)
                {
                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, response);
                }

            } 
        }
    }
}
