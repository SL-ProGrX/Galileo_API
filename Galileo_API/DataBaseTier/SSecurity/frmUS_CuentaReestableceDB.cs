using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class FrmUsCuentaReestableceDb
    {
        private readonly IConfiguration _config;
        readonly MSecurityMainDb DBBitacora;

        public FrmUsCuentaReestableceDb(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        public ErrorDto UsuarioCuentaReestablecer(CuentaReestablecer datos)
        {
            ErrorDto resultado = new ErrorDto();
            try
            {
                if (VerifyPasswordHistory(datos.Nuevo, datos.UsuarioId))
                {
                    using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                    {
                        var datosCtaReestablecer = new
                        {
                            UsuarioId = datos.UsuarioId,
                            UsuarioNombre = datos.UsuarioNombre,
                            CambiaSesion = datos.CambiaSesion ? 1 : 0,
                            Nuevo = datos.Nuevo,
                            Notas = datos.Notas,
                            UsuarioMovimiento = datos.UsuarioMovimiento
                        };

                        resultado.Code = connection.Execute("spPGX_W_Cuenta_Reestablecer", datosCtaReestablecer, commandType: CommandType.StoredProcedure);
                        resultado.Description = "Ok";
                    }
                }
                else
                {
                    resultado.Code = -1;
                    resultado.Description = "-La contraseña nueva ya ha sido utilizada con anterioridad, por favor ingrese una nueva";
                }
            }
            catch (Exception ex)
            {
                resultado.Code = -1;
                resultado.Description = ex.Message;
            }
            return resultado;
        }

        public bool VerifyPasswordHistory(string newPassword, int userId)
        {
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
            {


                // Execute the SQL function and retrieve the result
                var result = connection.ExecuteScalar<int>("SELECT dbo.fxSEG_VerificaPasswordHistory(@txtNuevo, @UserID)", new
                {
                    txtNuevo = newPassword,
                    UserID = userId
                });

                // Return true if the result is 1 (valid), otherwise false
                return result == 1;
            }
        }

    }
}