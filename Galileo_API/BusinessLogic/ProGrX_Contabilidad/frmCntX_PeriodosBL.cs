using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXPeriodosBl
    {
        private readonly FrmCntXPeriodosDb _db;

        public FrmCntXPeriodosBl(IConfiguration config) =>
            _db = new FrmCntXPeriodosDb(config);

        public ErrorDto<List<CntxPeriodoListaData>> CntxPeriodos_Listar(
            int codEmpresa,
            int codConta,
            string estado)
        {
            return _db.CntxPeriodos_Listar(codEmpresa, codConta, estado);
        }
    }
}
