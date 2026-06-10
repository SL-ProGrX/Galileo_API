namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    using Galileo.Models.ERROR;
    using Galileo_API.DataBaseTier.ProGrX.Creditos;
    using Galileo_API.Models.ProGrX.Creditos;

    public class FrmCrMonitorCancelacionBL
    {
        private readonly FrmCrMonitorCancelacionDB _db;

        public FrmCrMonitorCancelacionBL(IConfiguration config)
        {
            _db = new FrmCrMonitorCancelacionDB(config);
        }

        public ErrorDto<List<CrMonitorCancelacionModel>> CrMonitorCancelacion_Obtener(int CodEmpresa, CrMonitorCancelacionRequest request)
        {
            return _db.CrMonitorCancelacion_Obtener(CodEmpresa, request);
        }
    }
}
