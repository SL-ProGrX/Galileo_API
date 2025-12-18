using Dapper;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class BDAnalisisDB
    {
        private readonly IConfiguration _config;

        public BDAnalisisDB(IConfiguration config)
        {
            _config = config;
        }

        // =======================
        // API pública
        // =======================

        public List<string> TablasCargar()
        {
            try
            {
                using var connection =
                    new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                const string sql = @"
                    SELECT name
                    FROM sys.objects
                    WHERE type = 'U'
                    ORDER BY name";

                return connection.Query<string>(sql).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        
    }
}