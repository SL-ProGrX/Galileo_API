using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class CambiarContrasenaDB
    {
        private const string connectionStringName = "DefaultConnString";
        private const string changePasswordProcedureName = "spSEG_Password";

        private readonly IConfiguration _config;

        public CambiarContrasenaDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<ParametrosObtenerDto> ParametrosObtener()
        {
            var response = DbHelper.CreateOkResponse(new ParametrosObtenerDto());
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var strSQL = @"
                                    SELECT [ID_PARAMETRO], [KEY_LENMIN], [KEY_LENMAX], 
                                           [KEY_RENEW_DAY], [KEY_REMAIN_DAYS], 
                                           [KEY_HISTORY], [TIME_LOCK], 
                                           [KEY_INTENTOS], [KEY_CAPCHAR], 
                                           [KEY_SIMCHAR], [KEY_NUMCHAR],
                                           [TFA_IND], [TFA_METODO]
                                    FROM [PGX_Portal].[dbo].[US_PARAMETROS]";

                    var result = connection.Query<ParametrosObtenerDto>(strSQL).FirstOrDefault();
                    if (result is null)
                    {
                        return DbHelper.CreateErrorResponse<ParametrosObtenerDto>("No se encontraron los parámetros de contraseña.");
                    }

                    response.Result = result;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto<List<string>> KeyHistoryObtener(string Usuario, int topQuantity)
        {
            var response = DbHelper.CreateOkResponse(new List<string>());
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var strSQL = @"
                                SELECT TOP (@TopQuantity) [KEYSEC]
                                FROM US_KEYHISTORY KH
                                INNER JOIN US_usuarios U ON KH.IDKEYSEC = U.USERID
                                WHERE U.USUARIO = @Usuario
                                ORDER BY KH.ID DESC";

                    response.Result = connection.Query<string>(strSQL, new { TopQuantity = topQuantity, Usuario }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        public ErrorDto CambiarClave(ClaveCambiarDto cambioClave)
        {
            ErrorDto resp = DbHelper.CreateOkResponse();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    // Check if the user exists before changing the password
                    var userExists = connection.QueryFirstOrDefault<int>(
                        "SELECT COUNT(1) FROM US_usuarios WHERE Usuario = @Usuario",
                        new { cambioClave.Usuario });

                    if (userExists == 0)
                    {
                        resp.Code = -1;
                        resp.Description = "Usuario no encontrado.";
                        return resp;
                    }

                    // Execute the stored procedure to change the password
                    connection.Execute(
                        changePasswordProcedureName, cambioClave, commandType: CommandType.StoredProcedure);
                    var latestPassword = connection.QueryFirstOrDefault<string>(
                        "SELECT TOP (1) KH.KEYSEC FROM US_KEYHISTORY KH INNER JOIN US_usuarios U ON KH.IDKEYSEC = U.USERID WHERE U.USUARIO = @Usuario ORDER BY KH.ID DESC",
                        new { cambioClave.Usuario });

                    if (string.Equals(latestPassword?.Trim(), cambioClave.PassNuevo.Trim(), StringComparison.Ordinal))
                    {
                        resp.Code = 0;
                        resp.Description = "La clave de acceso ha sido cambiada exitosamente.";
                    }
                    else
                    {
                        resp.Code = -1;
                        resp.Description = "No se pudo cambiar la clave de acceso.";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public bool CambiarClaveParaAutenticacion(ClaveCambiarDto cambioClave)
        {
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            var userExists = connection.QueryFirstOrDefault<int>(
                "SELECT COUNT(1) FROM US_usuarios WHERE Usuario = @Usuario",
                new { cambioClave.Usuario });

            if (userExists == 0)
            {
                return false;
            }

            connection.Execute(
                changePasswordProcedureName,
                cambioClave,
                commandType: CommandType.StoredProcedure);
            var latestPassword = connection.QueryFirstOrDefault<string>(
                "SELECT TOP (1) KH.KEYSEC FROM US_KEYHISTORY KH INNER JOIN US_usuarios U ON KH.IDKEYSEC = U.USERID WHERE U.USUARIO = @Usuario ORDER BY KH.ID DESC",
                new { cambioClave.Usuario });
            return string.Equals(latestPassword?.Trim(), cambioClave.PassNuevo.Trim(), StringComparison.Ordinal);
        }

        public int ValidarTokenParaRecuperacion(string usuario, string token)
        {
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            return connection.QuerySingle<int>(
                "SELECT dbo.fxSEG_Token_Valida(@Usuario, @Token)",
                new { Usuario = usuario, Token = token });
        }

        public int CambiarClaveParaRecuperacion(ClaveCambiarDto cambioClave, string token)
        {
            using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var usuario = cambioClave.Usuario.Trim();
            var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes($"{usuario.ToUpperInvariant()}\0{token}"));

            var tokenStatus = connection.QuerySingle<int>(
                "SELECT dbo.fxSEG_Token_Valida(@Usuario, @Token)",
                new { Usuario = usuario, Token = token },
                transaction);
            if (tokenStatus != 1)
            {
                return tokenStatus;
            }

            var used = connection.QuerySingle<int>(
                "SELECT COUNT(1) FROM dbo.SEG_PASSWORD_RECOVERY_USED WITH (UPDLOCK, HOLDLOCK) WHERE USUARIO = @Usuario AND TOKEN_HASH = @TokenHash",
                new { Usuario = usuario, TokenHash = tokenHash },
                transaction);
            if (used > 0)
            {
                return 0;
            }

            connection.Execute(
                changePasswordProcedureName,
                cambioClave,
                transaction,
                commandType: CommandType.StoredProcedure);
            var latestPassword = connection.QueryFirstOrDefault<string>(
                "SELECT TOP (1) KH.KEYSEC FROM US_KEYHISTORY KH INNER JOIN US_usuarios U ON KH.IDKEYSEC = U.USERID WHERE U.USUARIO = @Usuario ORDER BY KH.ID DESC",
                new { Usuario = usuario },
                transaction);
            if (!string.Equals(latestPassword?.Trim(), cambioClave.PassNuevo.Trim(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("El procedimiento no confirmó el cambio de contraseña.");
            }

            connection.Execute(
                "INSERT INTO dbo.SEG_PASSWORD_RECOVERY_USED (USUARIO, TOKEN_HASH) VALUES (@Usuario, @TokenHash)",
                new { Usuario = usuario, TokenHash = tokenHash },
                transaction);
            transaction.Commit();
            return 1;
        }


        public ErrorDto CambiarClave3(ClaveCambiarDto cambioClave)
        {
            ErrorDto resp = DbHelper.CreateOkResponse();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    resp.Code = connection.QueryFirst<int>(changePasswordProcedureName, cambioClave, commandType: CommandType.StoredProcedure);
                    resp.Description = "La clave de acceso ha sido cambiada.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }



    }
}
