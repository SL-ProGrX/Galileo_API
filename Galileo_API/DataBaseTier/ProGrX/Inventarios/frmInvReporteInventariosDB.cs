using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvReporteInventariosDB
    {
        private readonly IConfiguration _config;

        public frmInvReporteInventariosDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDto<List<BodegaReporteInvMCdto>> Obtener_Bodegas(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<BodegaReporteInvMCdto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT COD_BODEGA, DESCRIPCION FROM PV_BODEGAS";

                    response.Result = connection.Query<BodegaReporteInvMCdto>(query).ToList();

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


        public ErrorDto<List<LineasInvMCdto>> Obtener_Lineas(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<LineasInvMCdto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select COD_PRODCLAS,DESCRIPCION from PV_PROD_CLASIFICA";

                    response.Result = connection.Query<LineasInvMCdto>(query).ToList();

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