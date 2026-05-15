using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndCierreMensualFndBL
    {
        private readonly FrmFndCierreMensualFndDB _db;

        public FrmFndCierreMensualFndBL(IConfiguration config)
        {
            _db = new FrmFndCierreMensualFndDB(config);
        }

        public ErrorDto Fnd_CierreMensual_Aplicar(int CodEmpresa)
        {
            return _db.Fnd_CierreMensual_Aplicar(CodEmpresa);
        }
    }
}