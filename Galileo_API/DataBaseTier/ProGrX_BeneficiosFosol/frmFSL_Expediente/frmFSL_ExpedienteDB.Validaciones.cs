using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public partial class FrmFslExpedienteDB
    {
        /// <summary>
        /// Valida que un caso no haya sido presentado anteriormente (uso interno, con conexión abierta).
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <param name="cod_plan">Código del plan.</param>
        /// <param name="cod_comite">Código del comité/causa.</param>
        /// <returns>ErrorDto con Code 0 si es válido.</returns>
        private static ErrorDto fxValida(SqlConnection connection, string cedula, string? cod_plan, string? cod_comite)
        {
            if (cod_plan == null || cod_comite == null)
            {
                return DbHelper.ErrorResponse("\n-Faltan datos por llenar.");
            }

            const string sql = "SELECT dbo.fxFSL_ExpedienteValidaRegistro(@cedula, @cod_plan, @cod_comite, 0) AS Cumple";
            var resp = connection.QueryFirstOrDefault<string>(sql, new { cedula, cod_plan, cod_comite });

            return resp == "0"
                ? DbHelper.ErrorResponse("\n- El caso ya fue presentado anteriormente...verifique!")
                : new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Valida las credenciales de un miembro del comité mediante el SP de logon.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="usuario">Credenciales del usuario.</param>
        /// <returns>Resultado de la validación.</returns>
        public ErrorDto FslMiembroValida(int CodCliente, FslMiembroValida usuario)
        {
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                var res = connection.Query<int>("[spSEG_Logon]", new { Usuario = usuario.usuario, Clave = usuario.clave },
                    commandType: CommandType.StoredProcedure).FirstOrDefault();

                return res == 0
                    ? DbHelper.ErrorResponse("No fue posible validar al usuario.: Verifique su contrasena!")
                    : new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Aplica (procesa) un expediente Fosol mediante el SP de aplicación.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_expediente">Código del expediente.</param>
        /// <param name="usuario">Usuario que aplica.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslExpediente_Aplicar(int CodCliente, long cod_expediente, string usuario)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var res = connection.Query<dynamic>("[spFSL_AplicacionFosol]", new { Expediente = cod_expediente, Usuario = usuario },
                    commandType: CommandType.StoredProcedure).FirstOrDefault();

                return res == null
                    ? DbHelper.ErrorResponse("No fue posible aplicar la operación")
                    : new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
