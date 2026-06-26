using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTranReversionBL
    {
        private readonly FrmInvTranReversionDB _db;

        public FrmInvTranReversionBL(IConfiguration config)
        {
            _db = new FrmInvTranReversionDB(config);
        }

        public ErrorDto<TranReversionData> InvTranReversion_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            return _db.InvTranReversion_Obtener(CodEmpresa, CodBoleta, TipoTran);
        }

        public ErrorDto<List<InvProducReversion>> InvProducLineas_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            return _db.InvProducLineas_Obtener(CodEmpresa, CodBoleta, TipoTran);
        }

        public ErrorDto<TranReversionData> InvTranReversion_scroll(int CodEmpresa, int scrollValue, string? CodBoleta, string TipoTran)
        {
            return _db.InvTranReversion_scroll(CodEmpresa, scrollValue, CodBoleta, TipoTran);
        }

        public ErrorDto InvTranReversion_Insertar(int CodEmpresa, TranReversionInsert request)
        {
            return _db.InvTranReversion_Insertar(CodEmpresa, request);
        }
    }
}