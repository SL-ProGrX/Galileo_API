using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXConciliacionMovBl
    {
        private readonly FrmCntXConciliacionMovDb _db;

        public FrmCntXConciliacionMovBl(IConfiguration config) =>
            _db = new FrmCntXConciliacionMovDb(config);

        public ErrorDto<CntXConciliacionResult> CntXConciliacionMov_Conciliar(int codEmpresa, CntXConciliacionMovRequest request)
        {
            return _db.CntXConciliacionMov_Conciliar(codEmpresa, request);
        }
    }
}
