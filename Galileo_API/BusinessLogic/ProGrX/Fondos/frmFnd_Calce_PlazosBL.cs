using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndCalcePlazosBL
    {
        private readonly FrmFndCalcePLazosDB _Db;

        public FrmFndCalcePlazosBL(IConfiguration config)
        {
            _Db = new FrmFndCalcePLazosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Periodos_Lista(int CodEmpresa)
        {
            return _Db.Periodos_Lista(CodEmpresa);
        }

    }
}