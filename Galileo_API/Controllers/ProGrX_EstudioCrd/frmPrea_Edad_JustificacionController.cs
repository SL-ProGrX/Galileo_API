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
    public class FrmPreaEdadJustificacionController : ControllerBase
    {
        private readonly FrmPreaEdadJustificacionBL _bl;

        public FrmPreaEdadJustificacionController(IConfiguration config)
        {
            _bl = new FrmPreaEdadJustificacionBL(config);
        }

        [HttpPost("Prea_frmPreaEdadJustificacion_Cargar")]
        public ErrorDto<FrmPreaEdadJustificacionCargarResponse> Prea_frmPreaEdadJustificacion_Cargar(
            int codEmpresa,
            [FromBody] FrmPreaEdadJustificacionCargarRequest request)
        {
            return _bl.Prea_frmPreaEdadJustificacion_Cargar(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaEdadJustificacion_Guardar")]
        public ErrorDto<FrmPreaEdadJustificacionGuardarResponse> Prea_frmPreaEdadJustificacion_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaEdadJustificacionGuardarRequest request)
        {
            return _bl.Prea_frmPreaEdadJustificacion_Guardar(codEmpresa, request);
        }
    }
}
