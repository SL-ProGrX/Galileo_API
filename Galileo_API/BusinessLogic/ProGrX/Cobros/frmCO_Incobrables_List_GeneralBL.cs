using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoIncobrablesListGeneralBL
    {

        private readonly FrmCoIncobrablesListGeneralDb _db;

        public FrmCoIncobrablesListGeneralBL(IConfiguration config) => _db = new FrmCoIncobrablesListGeneralDb(config);

        public ErrorDto<List<CbrIncobrableMovimientos>> CoIncobrablesListMovimiento_Obtener(int CodEmpresa, string pOperacion, string pCxC_Operacion)
        {
            return _db.CoIncobrablesListMovimiento_Obtener(CodEmpresa, pOperacion, pCxC_Operacion);
        }
        public ErrorDto<List<CbrIncobrableGeneral>> CoIncobrablesListGeneral_Obtener(int CodEmpresa, CbrIncobrableFiltros filtros)
        {
            return _db.CoIncobrablesListGeneral_Obtener(CodEmpresa, filtros);
        }
    }
}
