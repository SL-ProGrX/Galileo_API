using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class AppLogDB
    {
        private readonly IConfiguration _config;

        public AppLogDB(IConfiguration config)
        {
            _config = config;
        }

        public List<AppLog> AppLog_ObtenerTodos(int empresa, string ini, string fin)
        {
            List<AppLog> types = new List<AppLog>();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                {
                    var procedure = "[spAPP_Estadistica]";
                    var values = new
                    {
                        EmpresaId = empresa,
                        Inicio = ini,
                        Corte = fin,

                    };

                    types = connection.Query<AppLog>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return types;
        }


    }
}
