using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class ActivacionCuentaDB
    {
        private readonly IConfiguration _config;

        public ActivacionCuentaDB(IConfiguration config)
        {
            _config = config;
        }

        public ActivacionCuentaDto UsuarioEstado_Obtener(string user)
        {
            ActivacionCuentaDto data = new ActivacionCuentaDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var query = "SELECT * FROM US_USUARIOS WHERE USUARIO = @user";


                    var result = connection.Query<ActivacionCuentaDto>(query, new { user }).FirstOrDefault();
                    if (result != null)
                    {
                        data = result;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }


        public ErrorDto UsuarioEstado_Actualizar(ActivacionCuentaDto request)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                var procedure = "[spPGX_W_Usuario_Activacion]";

                var parameters = new DynamicParameters();
                parameters.Add("UserID", request.UserId, DbType.Int32);
                parameters.Add("Estado", request.Estado, DbType.String);
                parameters.Add("Notas", request.Notas, DbType.String);
                parameters.Add("UsuarioActual", request.UsuarioActual, DbType.String);
                parameters.Add("UsuarioAfectado", request.UsuarioAfectado, DbType.String);



                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    resp.Code = connection.Query<int>(procedure, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
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
    }
}
