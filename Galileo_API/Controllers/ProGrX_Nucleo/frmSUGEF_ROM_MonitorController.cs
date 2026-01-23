using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Nucleo;
using Galileo_API.Models.ProGrX_Nucleo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSugefRomMonitorController : ControllerBase
    {
        private readonly FrmSugefRomMonitorBL _bl;

        public FrmSugefRomMonitorController(IConfiguration config)
        {
            _bl = new FrmSugefRomMonitorBL(config);
        }

        [Authorize]
        [HttpGet("SUGEF_TipoCambio_Obtener")]
        public ErrorDto<SugefTipoCambioResult?> SUGEF_TipoCambio_Obtener(int codEmpresa, [FromQuery] DateTime fecha)
        {
            return _bl.SUGEF_TipoCambio_Obtener(codEmpresa, fecha);
        }

        [Authorize]
        [HttpGet("SUGEF_ROM_Monitor_Consulta")]
        public ErrorDto<List<SugefRomMonitorConsultaResult>> SUGEF_ROM_Monitor_Consulta(int codEmpresa, [FromQuery] DateTime corte)
        {
            return _bl.SUGEF_ROM_Monitor_Consulta(codEmpresa, corte);
        }
    }
}
