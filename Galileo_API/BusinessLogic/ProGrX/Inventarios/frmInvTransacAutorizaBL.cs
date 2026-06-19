using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTransacAutorizaBL
    {
        private readonly FrmInvTransacAutorizaDB _db;

        public FrmInvTransacAutorizaBL(IConfiguration config)
        {
            _db = new FrmInvTransacAutorizaDB(config);
        }

        public ErrorDto InvTransacAutoriza_Actualizar(int CodEmpresa, InvTransacAutoriza request)
        {
            return _db.InvTransacAutoriza_Actualizar(CodEmpresa, request);
        }
    }
}