using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTransacQryBL
    {
        private readonly FrmInvTransacQryDB _db;

        public FrmInvTransacQryBL(IConfiguration config)
        {
            _db = new FrmInvTransacQryDB(config);
        }

        public ErrorDto<TransacQryDataList> TransacInv_Obtener(int CodEmpresa, TransacQryParametros parametros)
        {
            return _db.TransacInv_Obtener(CodEmpresa, parametros);
        }
    }
}