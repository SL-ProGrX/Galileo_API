using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class AppLogDB
    {
        private readonly IConfiguration _config;

        public AppLogDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<AppLog>> AppLog_ObtenerTodos(int empresa, string ini, string fin)
        {
            var response = new ErrorDto<List<AppLog>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AppLog>(),
            };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));

                var values = new
                {
                    EmpresaId = empresa,
                    Inicio = ini,
                    Corte = fin,
                };

                response.Result = connection.Query<AppLog>(
                    "[spAPP_Estadistica]",
                    values,
                    commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}
