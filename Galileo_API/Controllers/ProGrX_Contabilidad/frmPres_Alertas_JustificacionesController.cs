using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PresAlertasJustificacionesController : ControllerBase
    {
        private readonly PresAlertasJustificacionesBL _BL;

        public PresAlertasJustificacionesController(IConfiguration config)
        {
            _BL = new PresAlertasJustificacionesBL(config);
        }

        [HttpPost("PresAlertaJustificacionBit_Obtener")]
        public ErrorDto<List<PresAlertaJustificacionBitacoraData>> PresAlertaJustificacionBit_Obtener(PresAlertaJustificacionBitRequest resquest)
        {
            return _BL.PresAlertaJustificacionBit_Obtener(resquest);
        }

    }
}
