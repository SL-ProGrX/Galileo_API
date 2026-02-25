using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizaProcPrevistaBL
    {
        private readonly FrmCrPolizaProcPrevistaDB _db;

        public FrmCrPolizaProcPrevistaBL(IConfiguration config)
        {
            _db = new FrmCrPolizaProcPrevistaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolProcPrevista_PolizaFacturables_Lista(int CodEmpresa)
        {
            return _db.Cr_PolProcPrevista_PolizaFacturables_Lista(CodEmpresa);
        }
    }
}
