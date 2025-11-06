using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.AH;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_PlanillaDirectaDB
    {
        private readonly IConfiguration _config;

        public frmAH_PlanillaDirectaDB(IConfiguration config)
        {
            _config = config;
        }

        public List<InstitucionesConciliarDto> obtener_InstitucionesConciliar(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<InstitucionesConciliarDto> info = new List<InstitucionesConciliarDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT 
                                    COD_INSTITUCION AS Idx, 
                                    '[' + COD_DIVISA + ']  ' + DESCRIPCION AS ItmX
                                FROM 
                                    INSTITUCIONES 
                                WHERE 
                                    ACTIVA = 1
                                ORDER BY 
                                    COD_INSTITUCION;";

                    info = connection.Query<InstitucionesConciliarDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }
    }
}