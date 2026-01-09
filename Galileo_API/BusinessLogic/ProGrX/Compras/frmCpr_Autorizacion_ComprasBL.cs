using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprAutorizacionComprasBL
    {
        readonly FrmCprAutorizacionComprasDB _db;

        public FrmCprAutorizacionComprasBL(IConfiguration config)
        {
            _db = new FrmCprAutorizacionComprasDB(config);
        }

        public ErrorDto<List<CprSolicitudAutoriza>> SolicitudAutorizacion_Obtener(int CodCliente, string filtros)
        {
            return _db.SolicitudAutorizacion_Obtener(CodCliente, filtros);
        }

        public ErrorDto AutorizaSolicitudes(int CodCliente, string solicitudes, string usuario)
        {
            return _db.AutorizaSolicitudes(CodCliente, solicitudes, usuario);
        }

        public ErrorDto RechazaSolicitudes(int CodCliente, string solicitudes, string justificacion, string usuario)
        {
            return _db.RechazaSolicitudes(CodCliente, solicitudes, justificacion, usuario);
        }
    }
}