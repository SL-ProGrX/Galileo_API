using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvKardexBL
    {
        private readonly FrmInvKardexDB _db;

        public FrmInvKardexBL(IConfiguration config)
        {
            _db = new FrmInvKardexDB(config);
        }

        public ErrorDto<List<ConsultaMovimientoBodegaCDdto>> Obtener_Bodegas(int CodEmpresa)
        {
            return _db.Obtener_Bodegas(CodEmpresa);
        }

        public ErrorDto<MovimientosDtoList> consultarMovimientos_Obtener(int CodCliente, string filtros)
        {
            return _db.consultarMovimientos_Obtener(CodCliente, filtros);
        }
    }
}