using PgxAPI.Models.TES;
using Dapper;
using PgxAPI.Models.ERROR;
using Microsoft.Data.SqlClient;


namespace PgxAPI.DataBaseTier.TES
{
    public class frmTES_MonitorPendingDB
    {
        private readonly IConfiguration? _config;

        public frmTES_MonitorPendingDB(IConfiguration? config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtiene el listado de pendientes del monitor de tesorería
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <returns>Lista de pendientes del monitor de tesorería</returns>
        public ErrorDTO<List<TES_MonitorPending>> TES_MonitorPending_Obtener(int CodEmpresa)
        {
            if (_config == null)
            {
                throw new ArgumentNullException(nameof(_config), "Configuración es nula");
            }

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TES_MonitorPending>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = @"exec spTes_Monitor_Pending";
                    response.Result = connection.Query<TES_MonitorPending>(query).ToList();
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