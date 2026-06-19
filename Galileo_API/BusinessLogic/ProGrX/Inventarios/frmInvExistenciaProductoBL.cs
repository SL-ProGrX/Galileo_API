using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvExistenciaProductoBL
    {
        private readonly FrmInvExistenciaProductoDB _db;

        public FrmInvExistenciaProductoBL(IConfiguration config)
        {
            _db = new FrmInvExistenciaProductoDB(config);
        }

        public ErrorDto<List<ExistenciaProductoDto>> existenciaProducto_Obtener(int CodCliente, string filtros)
        {
            return _db.existenciaProducto_Obtener(CodCliente, filtros);
        }
    }
}