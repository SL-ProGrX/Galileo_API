using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class CambiarContrasenaDB
    {
        private const string connectionStringName = "DefaultConnString";

        private readonly IConfiguration _config;

        public CambiarContrasenaDB(IConfiguration config)
        {
            _config = config;
        }

        public ParametrosObtenerDto? ParametrosObtener()
        {
            ParametrosObtenerDto? resp = null;
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

                    resp = connection.Query<ParametrosObtenerDto>(strSQL).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp = null;
                _ = ex.Message;
            }
            return resp;
        }

        public List<string> KeyHistoryObtener(string Usuario, int topQuantity)
        {
            List<string> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var strSQL = @"
                                SELECT TOP (@TopQuantity) [KEYSEC]
                                FROM US_KEYHISTORY KH
                                INNER JOIN US_usuarios U ON KH.IDKEYSEC = U.USERID
                                WHERE U.USUARIO = @Usuario";

                    resp = connection.Query<string>(strSQL, new { TopQuantity = topQuantity, Usuario }).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                resp = new List<string>();
            }
            return resp;
        }


        public ErrorDto CambiarClave(ClaveCambiarDto cambioClave)
        {
            ErrorDto resp = new ErrorDto();
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
                    int rowsAffected = connection.Execute(
                        "spSEG_Password", cambioClave, commandType: CommandType.StoredProcedure);

                    if (rowsAffected > 0)
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


        public ErrorDto CambiarClave3(ClaveCambiarDto cambioClave)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    resp.Code = connection.QueryFirst<int>("spSEG_Password", cambioClave, commandType: CommandType.StoredProcedure);
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
