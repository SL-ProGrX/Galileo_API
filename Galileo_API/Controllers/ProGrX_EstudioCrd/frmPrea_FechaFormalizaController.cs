using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd.Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaFechaFormalizaController : ControllerBase
    {
        private readonly FrmPreaFechaFormalizaBL _bl;

        public FrmPreaFechaFormalizaController(IConfiguration config)
        {
            _bl = new FrmPreaFechaFormalizaBL(config);
        }

        [HttpPost("Prea_frmPreaFechaFormaliza_Cargar")]
        public ErrorDto<FrmPreaFechaFormalizaCargarResponse> Prea_frmPreaFechaFormaliza_Cargar(
            int codEmpresa,
            [FromBody] FrmPreaFechaFormalizaCargarRequest request)
        {
            return _bl.Prea_frmPreaFechaFormaliza_Cargar(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaFechaFormaliza_Calcular")]
        public ErrorDto<FrmPreaFechaFormalizaCalcularResponse> Prea_frmPreaFechaFormaliza_Calcular(
            int codEmpresa,
            [FromBody] FrmPreaFechaFormalizaCalcularRequest request)
        {
            return _bl.Prea_frmPreaFechaFormaliza_Calcular(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaFechaFormaliza_Cambiar")]
        public ErrorDto<FrmPreaFechaFormalizaCambiarResponse> Prea_frmPreaFechaFormaliza_Cambiar(
            int codEmpresa,
            [FromBody] FrmPreaFechaFormalizaCambiarRequest request)
        {
            return _bl.Prea_frmPreaFechaFormaliza_Cambiar(codEmpresa, request);
        }
    }
}
