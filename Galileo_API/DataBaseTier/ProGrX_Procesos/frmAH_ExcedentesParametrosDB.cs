using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_ExcedentesParametrosDB
    {
        private readonly IConfiguration _config;

        public frmAH_ExcedentesParametrosDB(IConfiguration config)
        {
            _config = config;
        }



        public List<ParametroExcedenteDTO> obtener_ParametrosExcedentes(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ParametroExcedenteDTO> info = new List<ParametroExcedenteDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select * from EXC_PARAMETROS order by cod_parametro asc";

                    info = connection.Query<ParametroExcedenteDTO>(query).ToList();

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