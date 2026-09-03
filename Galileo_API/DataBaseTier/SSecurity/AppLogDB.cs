using Dapper;
using Microsoft.Data.SqlClient;
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

        public List<AppLog> AppLog_ObtenerTodos(int empresa, string ini, string fin)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));

            var values = new
            {
                EmpresaId = empresa,
                Inicio = ini,
                Corte = fin,
            };

            return connection.Query<AppLog>(
                "[spAPP_Estadistica]",
                values,
                commandType: CommandType.StoredProcedure).ToList();
        }
    }
}
