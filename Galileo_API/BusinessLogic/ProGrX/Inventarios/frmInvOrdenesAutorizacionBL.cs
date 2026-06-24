using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvOrdenesAutorizacionBL
    {
        private readonly FrmInvOrdenesAutorizacionDB _db;

        public FrmInvOrdenesAutorizacionBL(IConfiguration config)
        {
            _db = new FrmInvOrdenesAutorizacionDB(config);
        }

        public ErrorDto<List<ResolucionTransaccionDto>> resolucionTransaccion_Obtener(int CodCliente, string filtros)
        {
            return _db.resolucionTransaccion_Obtener(CodCliente, filtros);
        }

        public ErrorDto ResolucionTransaccion_Autorizar(int CodCliente, string tipo, string usuario, List<ResolucionTransaccionDto> lista)
        {
            return _db.ResolucionTransaccion_Autorizar(CodCliente, tipo, usuario, lista);
        }

        public ErrorDto ResolucionTransaccion_Rechazo(int CodCliente, string tipo, string usuario, List<ResolucionTransaccionDto> lista)
        {
            return _db.ResolucionTransaccion_Rechazo(CodCliente, tipo, usuario, lista);
        }
    }
}