using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AH;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_ExcedentesRenta_TablaDB
    {
        private readonly IConfiguration _config;

        public frmAH_ExcedentesRenta_TablaDB(IConfiguration config)
        {
            _config = config;
        }

        public List<RentaExcedenteDto> obtener_RentaExcedente(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<RentaExcedenteDto> info = new List<RentaExcedenteDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select ID_RENTA,DESDE,HASTA,PORCENTAJE from EXC_RENTA_TABLA order by ID_RENTA";

                    info = connection.Query<RentaExcedenteDto>(query).ToList();

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