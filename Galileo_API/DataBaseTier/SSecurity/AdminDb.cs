using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.Security;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class AdminDb
    {

        private readonly IConfiguration _config;

        public AdminDb(IConfiguration config)
        {
            _config = config;
        }

        public List<LoginDbResult> Login(string username, string passw)
        {
            List<LoginDbResult> resp = new List<LoginDbResult>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spGA_AdminUsuarioLogin]";
                    var values = new
                    {
                        UserName = username,
                        Password = passw
                    };
                    resp = connection.Query<LoginDbResult>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }





    }//end class
}//end namespace
