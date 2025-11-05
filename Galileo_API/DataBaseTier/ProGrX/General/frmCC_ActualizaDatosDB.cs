using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCC_ActualizaDatosDB
    {
        private readonly IConfiguration _config;

        public frmCC_ActualizaDatosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto CC_ActualizaDatos_SP(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spCRDActualizaDatos";
                    resp.Code = connection.Execute(query);
                    resp.Description = "Proceso Terminado Satisfactoriamente...";
                }
            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}