using Dapper;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier
{
    public class frmFSL_ReportesDB
    {
        private readonly IConfiguration _config;

        public frmFSL_ReportesDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<Oficina>> FSL_Oficinas_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<Oficina>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select rtrim(cod_oficina) as 'item', rtrim(descripcion) as 'descripcion'
                                 from SIF_Oficinas order by cod_oficina";

                    response.Result = connection.Query<Oficina>(query).ToList();

                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FSL_Oficinas_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }

        public ErrorDto<List<Plan>> FSL_Planes_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<Plan>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select RTRIM(COD_PLAN) item , RTRIM(COD_PLAN) + ' - ' + descripcion as descripcion FROM FSL_PLANES WHERE ACTIVO = 1 order by cod_plan";

                    response.Result = connection.Query<Plan>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FSL_Planes_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }
    }
}