using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class PresAlertasJustificacionesBL
    {
        private readonly PresAlertasJustificacionesDB _db;

        public PresAlertasJustificacionesBL(IConfiguration config)
        {
            _db = new PresAlertasJustificacionesDB(config);
        }

        public ErrorDto<List<PresAlertaJustificacionBitacoraData>> PresAlertaJustificacionBit_Obtener(PresAlertaJustificacionBitRequest resquest)
        {
            return _db.PresAlertaJustificacionBit_Obtener(resquest);
        }
    }
}
