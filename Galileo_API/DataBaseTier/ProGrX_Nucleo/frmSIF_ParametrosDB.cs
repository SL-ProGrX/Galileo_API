using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.SIF;

namespace PgxAPI.DataBaseTier
{
    public class frmSIF_ParametrosDB
    {
        private readonly IConfiguration _config;

        public frmSIF_ParametrosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene los parametros del sistema
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<SifParametrosDto>> obtener_ParametrosSistema(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<SifParametrosDto>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "SELECT cod_parametro, descripcion, valor FROM SIF_PARAMETROS ORDER BY cod_parametro;";
                    response.Result = connection.Query<SifParametrosDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }
    }
}