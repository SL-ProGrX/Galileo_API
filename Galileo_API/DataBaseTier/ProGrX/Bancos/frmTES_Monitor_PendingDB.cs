using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.TES;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesMonitorPendingDB
    {
        private readonly PortalDB _portalDB;

        public FrmTesMonitorPendingDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el listado de pendientes del monitor de tesorería
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <returns>Lista de pendientes del monitor de tesorería</returns>
        public ErrorDto<List<TesMonitorPending>> TES_MonitorPending_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"exec spTes_Monitor_Pending";
                var result = conn.Query<TesMonitorPending>(query).ToList();
                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesMonitorPending>>(ex.Message);
            }
        }
    }
}