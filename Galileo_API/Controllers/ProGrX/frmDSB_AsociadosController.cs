using Galileo.BusinessLogic.ProGrX;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Galileo.Controllers.ProGrX
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmDsbAsociadosController : ControllerBase
    {
        private readonly FrmDsbAsociadosBL _bl;

        public FrmDsbAsociadosController(IConfiguration config)
        {
            _bl = new FrmDsbAsociadosBL(config);
        }

        [HttpGet("Asociado_Obtener")]
        public ActionResult<ErrorDto<DashboardAsociadosData>> Asociado_Obtener(
            int CodEmpresa,
            string Cedula)
        {
            var usuario = User.FindFirst("UserName")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(usuario)) return Unauthorized();

            return Ok(_bl.Asociado_Obtener(CodEmpresa, Cedula, usuario));
        }
    }
}
