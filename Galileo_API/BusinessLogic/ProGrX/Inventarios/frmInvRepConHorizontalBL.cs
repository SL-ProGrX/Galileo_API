using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmInvRepConHorizontalBl
    {
        private readonly FrmInvRepConHorizontalDb _db;

        public FrmInvRepConHorizontalBl(IConfiguration config)
        {
            _db = new FrmInvRepConHorizontalDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Obtener_Bodegas(int CodEmpresa)
        {
            return _db.Obtener_Bodegas(CodEmpresa);
        }
    }
}