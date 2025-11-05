using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class ActivacionCuentaDB
    {
        private readonly IConfiguration _config;

        public ActivacionCuentaDB(IConfiguration config)
        {
            _config = config;
        }

        public ActivacionCuentaDTO UsuarioEstado_Obtener(string user)
        {
            ActivacionCuentaDTO data = new ActivacionCuentaDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var query = "SELECT * FROM US_USUARIOS WHERE USUARIO = @user";


                    data = connection.Query<ActivacionCuentaDTO>(query, new { user }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }


        public ErrorActivacionCuentaDTO UsuarioEstado_Actualizar(ActivacionCuentaDTO request)
        {
            ErrorActivacionCuentaDTO resp = new ErrorActivacionCuentaDTO();
            try
            {
                var procedure = "[spPGX_W_Usuario_Activacion]";

                //  var query = "Update US_Usuarios set Estado = @estado where UserID = @user";

                var parameters = new DynamicParameters();
                parameters.Add("UserID", request.UserId, DbType.Int32);
                parameters.Add("Estado", request.Estado, DbType.String);
                parameters.Add("Notas", request.Notas, DbType.String);
                parameters.Add("UsuarioActual", request.UsuarioActual, DbType.String);
                parameters.Add("UsuarioAfectado", request.UsuarioAfectado, DbType.String);



                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    resp.Code = connection.Query<int>(procedure, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    //resp.Code = connection.ExecuteAsync(query, parameters).Result;
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
