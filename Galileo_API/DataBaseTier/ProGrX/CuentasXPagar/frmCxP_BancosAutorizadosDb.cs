using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCxP_BancosAutorizadosDB
    {
        private readonly IConfiguration _config;

        public frmCxP_BancosAutorizadosDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDTO<List<BancosAutorizadosDTO>> ObtenerBancosAutorizados(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<BancosAutorizadosDTO>>
            {
                Code = 0
            };
            string sql = "spObtenerTodosBancosAutorizados";
            var values = new
            {
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                response.Result = connection.Query<BancosAutorizadosDTO>(sql, values).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;


        }

        public ErrorDTO IngresarTesBancosNuevos(string Usuario, int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDTO
            {
                Code = 0
            };

            try
            {
                string query = $@"insert into CxP_Bancos_Autorizados(id_banco,cheques,transferencias,registro_fecha,registro_usuario) 
                    select id_banco,0,0,dbo.MyGetdate(),'{Usuario}' from Tes_Bancos 
                    where id_Banco not in (select id_Banco from CxP_Bancos_Autorizados)";
                using var connection = new SqlConnection(clienteConnString);
                resp.Code = connection.Query<int>(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDTO ActualizarTransferencia(int BancoId, bool Valor, int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDTO
            {
                Code = 0
            };
            string sql = "spActualizarBancoAutorizacionTransferencia";
            var values = new
            {

                Valor,
                BancoId

            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                resp.Code = connection.Query<int>(sql, values).FirstOrDefault();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDTO ActualizarCheque(int BancoId, bool Valor, int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDTO
            {
                Code = 0
            };
            string sql = "spActualizarBancoAutorizacionCheques";
            var values = new
            {

                Valor,
                BancoId

            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                resp.Code = connection.Query<int>(sql, values).FirstOrDefault();
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
