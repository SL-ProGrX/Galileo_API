using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.TES
{
    public class FrmTesMonitorPendingBL
    {
        private readonly FrmTesMonitorPendingDB _db;

        public FrmTesMonitorPendingBL(IConfiguration config)
        {
            _db = new FrmTesMonitorPendingDB(config);
        }

      public ErrorDto<List<TesMonitorPending>> TES_MonitorPending_Obtener(int CodEmpresa)
      {
          return _db.TES_MonitorPending_Obtener(CodEmpresa);
      }
    }

}