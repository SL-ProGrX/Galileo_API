using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivConsultaHonorariosDetalleController : ControllerBase
    {
        private readonly FrmVivConsultaHonorariosDetalleBL _bl;

        public FrmVivConsultaHonorariosDetalleController(IConfiguration config)
        {
            _bl = new FrmVivConsultaHonorariosDetalleBL(config);
        }

        [HttpPost("Viv_ConsultaHonorariosDetalle_Obtener")]
        public ErrorDto<FrmVivConsultaHonorariosDetalleResponse> Viv_ConsultaHonorariosDetalle_Obtener(
            int codEmpresa,
            FrmVivConsultaHonorariosDetalleRequest request)
        {
            return _bl.Viv_ConsultaHonorariosDetalle_Obtener(codEmpresa, request);
        }
    }
}
