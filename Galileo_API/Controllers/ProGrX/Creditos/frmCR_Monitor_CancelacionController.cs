namespace Galileo_API.Controllers.ProGrX.Creditos
{
    using Galileo.Models.ERROR;
    using Galileo_API.BusinessLogic.ProGrX.Creditos;
    using Galileo_API.Models.ProGrX.Creditos;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrMonitorCancelacionController : ControllerBase
    {
        private readonly FrmCrMonitorCancelacionBL _bl;

        public FrmCrMonitorCancelacionController(IConfiguration config)
        {
            _bl = new FrmCrMonitorCancelacionBL(config);
        }

        [HttpPost("CrMonitorCancelacion_Obtener")]
        public ErrorDto<List<CrMonitorCancelacionModel>> CrMonitorCancelacion_Obtener(int CodEmpresa, CrMonitorCancelacionRequest request)
        {
            return _bl.CrMonitorCancelacion_Obtener(CodEmpresa, request);
        }
    }
}
