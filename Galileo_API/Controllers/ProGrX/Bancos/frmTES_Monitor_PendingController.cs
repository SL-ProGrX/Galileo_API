using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic.TES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesMonitorPendingController : ControllerBase
    {
        private readonly FrmTesMonitorPendingBL _bl;

        public FrmTesMonitorPendingController(IConfiguration config)
        {
            _bl = new FrmTesMonitorPendingBL(config);
        }

        
        [HttpGet("TES_MonitorPending_Obtener")]
        public ErrorDto<List<TesMonitorPending>> TES_MonitorPending_Obtener(int CodEmpresa)
        {
            return _bl.TES_MonitorPending_Obtener(CodEmpresa);
        }
    }
}