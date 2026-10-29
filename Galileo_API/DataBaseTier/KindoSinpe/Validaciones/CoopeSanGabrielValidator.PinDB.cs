using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;

namespace Galileo_API.DataBaseTier
{
    public partial class CoopeSanGabrielValidator
    {
        private ErrorDto? VerificarServicioDisponiblePIN(string uriConn, ReqBase context)
        {
            if (_servicioDisponibleLote == true)
                return null;

            var servicio = _sinpePIN.IsServiceAvailable(uriConn, context);
            if (!servicio.ServiceAvailable)
                return DbHelper.ErrorResponse("Servicio no disponible: " + servicio.Errors?[0].Message);

            _servicioDisponibleLote = true;
            return null;
        }
    }
}
