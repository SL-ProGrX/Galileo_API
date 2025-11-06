using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvRepConHorizontalDB
    {
        private readonly IConfiguration _config;

        public frmInvRepConHorizontalDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<RepBodegaDto>> Obtener_Bodegas(int CodEmpresa)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<RepBodegaDto>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "SELECT * FROM pv_Bodegas";

                    response.Result = connection.Query<RepBodegaDto>(query).ToList();
                    foreach (RepBodegaDto dt in response.Result )
                    {
                        dt.Descripcion = dt.Descripcion;


                    }

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