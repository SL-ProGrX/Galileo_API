using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/FrmPreaEstudiov2")]
    [ApiController]
    [Authorize]
    public sealed class FrmPreaEstudiov2ScoreCrediticioController : ControllerBase
    {
        private readonly FrmPreaEstudiov2ScoreCrediticioBL _bl;

        public FrmPreaEstudiov2ScoreCrediticioController(
            IConfiguration config,
            IHostEnvironment environment)
        {
            _bl = new FrmPreaEstudiov2ScoreCrediticioBL(config, environment);
        }

        [HttpGet("Prea_frmPreaEstudiov2_ScoreCrediticio_Consultar")]
        public ErrorDto<PreaScoreCrediticioHistorialDto> Prea_frmPreaEstudiov2_ScoreCrediticio_Consultar(
            int codEmpresa,
            [FromQuery] string cod_preanalisis,
            [FromQuery] string cedula)
        {
            return _bl.Consultar(codEmpresa, cod_preanalisis, cedula);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Prea_frmPreaEstudiov2_ScoreCrediticio_Actualizar")]
        public Task<ErrorDto<PreaScoreCrediticioRegistroDto>> Prea_frmPreaEstudiov2_ScoreCrediticio_Actualizar(
            int codEmpresa,
            [FromBody] PreaScoreCrediticioRequest request,
            CancellationToken cancellationToken)
        {
            var usuario = User.FindFirst("UserName")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value
                ?? string.Empty;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            return _bl.ActualizarAsync(codEmpresa, request, usuario, ip, cancellationToken);
        }
    }
}
