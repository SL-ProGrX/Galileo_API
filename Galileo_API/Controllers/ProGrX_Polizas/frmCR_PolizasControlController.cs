using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPolizasControlController : ControllerBase
    {
        private readonly FrmCrPolizasControlBL _bl;

        public FrmCrPolizasControlController(IConfiguration config)
        {
            _bl = new FrmCrPolizasControlBL(config);
        }

        [HttpGet("Cr_PolizasControl_Obtener")]
        public ErrorDto<PolizaLookupResponseDto> Cr_PolizasControl_Obtener(int CodEmpresa, string CodPoliza)
        {
            return _bl.Cr_PolizasControl_Obtener(CodEmpresa, CodPoliza);
        }

        [HttpGet("ObtenerPolizaScroll")]
        public ErrorDto<PolizaLookupResponseDto?> Cr_PolizasControl_Scroll(
                int codEmpresa,
                string codPolizaActual,
                int direccion)
        {
            return _bl.Cr_PolizasControl_Scroll(codEmpresa, codPolizaActual, direccion);
        }

    }
}
