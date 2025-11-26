using Dapper;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class FrmCcActualizaDatosDb
    {
        private readonly IConfiguration _config;

        public FrmCcActualizaDatosDb(IConfiguration config)
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
                EjecutarActualizaDatos(connection, resp);
            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private static void EjecutarActualizaDatos(SqlConnection connection, ErrorDto resp)
        {
            var query = "exec spCRDActualizaDatos";
            resp.Code = connection.Execute(query);
            resp.Description = "Proceso Terminado Satisfactoriamente...";
        }
    }
}