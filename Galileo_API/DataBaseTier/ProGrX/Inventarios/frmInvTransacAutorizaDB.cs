using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmInvTransacAutorizaDB
    {
        private readonly IConfiguration _config;

        public frmInvTransacAutorizaDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Autorizaci�n de Transacciones de Inventario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto InvTransacAutoriza_Actualizar(int CodEmpresa, InvTransacAutoriza request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                    const string queryMancomunado = @"
                                SELECT TOP 1 1 
                                FROM pv_entrada_salida 
                                WHERE MANCOMUNADO = 1
                                  AND COD_ENTSAL = @tipo";

                    bool esMancomunado = connection.ExecuteScalar<int?>(queryMancomunado, new { tipo = request.Tipo }) == 1;


                    if (esMancomunado)
                    {
                        const string queryUsuario = @"
                              SELECT TOP 1 1 
                              FROM pv_requisiciones 
                              WHERE GENERA_USER = @Autoriza_User";

                        bool usuarioCoincide = connection.ExecuteScalar<int?>(queryUsuario, new { request.Autoriza_User }) == 1;

                        if (usuarioCoincide)
                        {
                            resp.Code = 2; 
                            return resp;
                        }
                    }


                    var query = "update pv_InvTranSac set estado = @Estado, Autoriza_user = @Autoriza_User, autoriza_fecha = getdate()" +
                        "where tipo = @Tipo and Boleta = @Boleta";

                    var parameters = new DynamicParameters();
                    parameters.Add("Boleta", request.Boleta, DbType.String);
                    parameters.Add("Tipo", request.Tipo, DbType.String);
                    parameters.Add("Autoriza_User", request.Autoriza_User, DbType.String);
                    parameters.Add("Estado", request.Estado, DbType.String);


                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Resoluci�n Ejecutada Satisfactoriamente...";
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