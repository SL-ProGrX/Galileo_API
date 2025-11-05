using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPParametrosDB
    {
        private readonly IConfiguration _config;

        public frmCxPParametrosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto ExecParametros(int CodCliente)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto
            {
                Code = 0
            };

            string procedure = "spCxPParametros";//"spCxP_Anticipos";
            var values = new
            {
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;

        }

        public ErrorDto<List<ParametrosDto>> ObtenerParametros(int CodCliente)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<ParametrosDto>>
            {
                Code = 0
            };

            string sql = "select * from cxp_parametros order by cod_parametro";
            var values = new
            {
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                response.Result = connection.Query<ParametrosDto>(sql, values).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;

        }

        public ErrorDto ActualizarDatosParametro(int CodCliente, string Usuario, string Valor, string Parametro)
        {

            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "update cxp_parametros set modifica_usuario = @Usuario, modifica_Fecha = Getdate(),valor = @Valor where cod_parametro = @Parametro";

                    var parameters = new DynamicParameters();
                    parameters.Add("Usuario", Usuario, DbType.String);
                    parameters.Add("Valor", Valor, DbType.String);
                    parameters.Add("Parametro", Parametro, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Parametro actualizado correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;

        }

    }
}
