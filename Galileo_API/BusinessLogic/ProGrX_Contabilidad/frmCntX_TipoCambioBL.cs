using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXTipoCambioBl
    {
        private readonly FrmCntXTipoCambioDb _db;

        public FrmCntXTipoCambioBl(IConfiguration config)
            => _db = new FrmCntXTipoCambioDb(config);

        public ErrorDto<CntXTipoCambioInicializaData> CntX_TipoCambio_Inicializa(
            int codEmpresa,
            int codConta,
            CntXTipoCambioInicializaRequest request)
        {
            return _db.CntX_TipoCambio_Inicializa(codEmpresa, codConta, request);
        }
    }
}
