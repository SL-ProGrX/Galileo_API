using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ARF;

namespace Galileo_API.BusinessLogic.ProGrX_ARF
{
    public class FrmArfInformesBl
    {
        private readonly FrmArfInformesDb _db;

        public FrmArfInformesBl(IConfiguration config)
        {
            _db = new FrmArfInformesDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ARF_Unidades_Listar(int codEmpresa)
        {
            return _db.ARF_Unidades_Listar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ARF_Arrendadores_Listar(int codEmpresa)
        {
            return _db.ARF_Arrendadores_Listar(codEmpresa);
        }
    }
}