using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvTransacReporteOrdenBL
    {
        private readonly FrmInvTransacReporteOrdenDB _db;

        public FrmInvTransacReporteOrdenBL(IConfiguration config)
        {
            _db = new FrmInvTransacReporteOrdenDB(config);
        }

        public ErrorDto<InvTransacReporteOrdenReporteDto?>
            INV_TransacReporteOrden_Reporte_Obtener(
                int CodEmpresa,
                InvTransacReporteOrdenRequest request)
        {
            return _db.INV_TransacReporteOrden_Reporte_Obtener(CodEmpresa, request);
        }
    }
}
