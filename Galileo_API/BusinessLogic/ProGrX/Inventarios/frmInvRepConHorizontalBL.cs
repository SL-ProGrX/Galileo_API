using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvRepConHorizontalBL
    {
        private readonly FrmInvRepConHorizontalDB _db;

        public FrmInvRepConHorizontalBL(IConfiguration config)
        {
            _db = new FrmInvRepConHorizontalDB(config);
        }

        public ErrorDto<List<RepBodegaDto>> Obtener_Bodegas(int CodEmpresa)
        {
            return _db.Obtener_Bodegas(CodEmpresa);
        }
    }
}