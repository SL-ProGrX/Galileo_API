using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaNotificacionController : ControllerBase
    {
        private readonly FrmPreaNotificacionBL _bl;

        public FrmPreaNotificacionController(IConfiguration config)
        {
            _bl = new FrmPreaNotificacionBL(config);
        }

        [HttpPost("Prea_frmPreaNotificacion_Cargar")]
        public ErrorDto<FrmPreaNotificacionCargarResponse> Prea_frmPreaNotificacion_Cargar(
            int codEmpresa,
            [FromBody] FrmPreaNotificacionCargarRequest request)
        {
            return _bl.Prea_frmPreaNotificacion_Cargar(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaNotificacion_Notificar")]
        public ErrorDto<FrmPreaNotificacionEnviarResponse> Prea_frmPreaNotificacion_Notificar(
            int codEmpresa,
            [FromBody] FrmPreaNotificacionEnviarRequest request)
        {
            return _bl.Prea_frmPreaNotificacion_Notificar(codEmpresa, request);
        }
    }
}
