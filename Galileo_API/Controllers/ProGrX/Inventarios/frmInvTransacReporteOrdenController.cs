using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public sealed class FrmInvTransacReporteOrdenController : ControllerBase
    {
        private readonly FrmInvTransacReporteOrdenBL _bl;

        public FrmInvTransacReporteOrdenController(IConfiguration config)
        {
            _bl = new FrmInvTransacReporteOrdenBL(config);
        }

        [HttpGet("INV_TransacReporteOrden_Reporte_Obtener")]
        public ErrorDto<InvTransacReporteOrdenReporteDto?>
            INV_TransacReporteOrden_Reporte_Obtener(
                int CodEmpresa,
                [FromQuery] InvTransacReporteOrdenRequest request)
        {
            return _bl.INV_TransacReporteOrden_Reporte_Obtener(CodEmpresa, request);
        }
    }
}
