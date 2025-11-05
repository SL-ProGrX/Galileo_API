using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmInvTransacProcesaDB
    {
        private readonly IConfiguration _config;

        public frmInvTransacProcesaDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto InvTransacProcesa_SP(int CodEmpresa, InvTransacProcesa request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto result = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "spINVTranProcesa";
                    var parameters = new
                    {
                        @Tipo = request.Tipo,
                        @Boleta = request.Boleta,
                        @Usuario = request.Usuario
                    };

                    connection.Execute(procedure, parameters, commandType: CommandType.StoredProcedure);
                    result.Code = 0;
                    result.Description = "Procesamiento Finalizado Satisfactoriamente...";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

    }
}