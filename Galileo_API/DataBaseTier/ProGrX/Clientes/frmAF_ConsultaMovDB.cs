using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_ConsultaMovDB
    {
        private readonly IConfiguration _config;

        public frmAF_ConsultaMovDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene Movimiento ingresos
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfiConsultaMovIngresos>> ConsultaMovIngresos_Obtener(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiConsultaMovIngresos>>();
            try
            {
                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"exec spAFI_ConsultaMovIngresos '{cedula}' ";
                    response.Result = db.Query<AfiConsultaMovIngresos>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ConsultaMovIngresos_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }

        /// <summary>
        /// Obtiene Movimientos Renuncias
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfiConsultaMovRenuncias>> ConsultaMovRenuncias_Obtener(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiConsultaMovRenuncias>>();
            try
            {
                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"exec spAFI_ConsultaMovRenuncias '{cedula}' ";
                    response.Result = db.Query<AfiConsultaMovRenuncias>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ConsultaMovRenuncias_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }

        /// <summary>
        /// Obtiene Movimientos Liquidaciones
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfiConsultaMovLiquidaciones>> ConsultaMovLiquidaciones_Obtener(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiConsultaMovLiquidaciones>>();
            try
            {
                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"exec spAFI_ConsultaMovLiquidaciones '{cedula}' ";
                    response.Result = db.Query<AfiConsultaMovLiquidaciones>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ConsultaMovLiquidaciones_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }


        /// <summary>
        /// Aplica reversion de liquidacion.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="idLiquidacion"></param>
        /// <returns></returns>
        public ErrorDTO AF_MovLiquidaciones_Reversion(int CodEmpresa, string usuario, string idLiquidacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Guardado correctamente"
            };

            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var sql = @"EXEC spAFI_Liquidacion_Reversa @IdLiquidacion, @Usuario;";

                    connection.Execute(sql, new
                    {
                        IdLiquidacion = idLiquidacion,
                        Usuario = usuario
                    });
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

    }
}