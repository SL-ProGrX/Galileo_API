using Dapper;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficioReporteDB
    {
        private readonly IConfiguration _config;

        public frmAF_BeneficioReporteDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene reportes de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<afiBeneficiosData>> BeneficioLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<afiBeneficiosData>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(cod_Beneficio) as item, rtrim(descripcion) as descripcion from afi_beneficios ";
                    response.Result = connection.Query<afiBeneficiosData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneficioLista_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }
    }
}