using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndPlanesCopiaBl
    {
        private readonly FrmFndPlanesCopiaDb _Db;

        public FrmFndPlanesCopiaBl(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndPlanesCopiaDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_Planes_Obtener(int CodEmpresa)
        {
            return _Db.FND_Planes_Obtener(CodEmpresa);
        }

        public ErrorDto<DropDownListaGenericaModel> AF_Plan_Scroll_Obtener(int CodEmpresa,
    string plan, int scrollCode)
        {
            return _Db.AF_Plan_Scroll_Obtener(CodEmpresa, plan, scrollCode);
        }

        public ErrorDto FND_Planes_Copiar(int CodEmpresa, string usuario, FndPlanesCopiaRequestDto dto)
        {
            return _Db.FND_Planes_Copiar(CodEmpresa, usuario, dto);
        }

    }
}