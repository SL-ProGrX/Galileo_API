using Dapper;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class MAfilicacionDB
    {
        private readonly IConfiguration _config;

        public MAfilicacionDB(IConfiguration config)
        {
            _config = config;
        }

        public string fxgAFIParametroComision(int CodEmpresa, string pCodigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string result = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {   
                    var query = $@"select valor from AFI_COMISIONES_PARAMETROS where cod_parametro = @codigo";
                    result = connection.QueryFirstOrDefault<string>(query, new { codigo = pCodigo }) ?? "";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public string fxNombre(int CodEmpresa, string strCedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string result = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select nombre from socios where cedula = @cedula";
                    result = connection.QueryFirstOrDefault<string>(query, new { cedula = strCedula }) ?? "";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}
