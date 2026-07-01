using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTransacProcesaBL
    {
        private readonly FrmInvTransacProcesaDB _db;

        public FrmInvTransacProcesaBL(IConfiguration config)
        {
            _db = new FrmInvTransacProcesaDB(config);
        }

        public ErrorDto InvTransacProcesa_SP(int CodEmpresa, InvTransacProcesa request)
        {
            return _db.InvTransacProcesa_SP(CodEmpresa, request);
        }
    }
}