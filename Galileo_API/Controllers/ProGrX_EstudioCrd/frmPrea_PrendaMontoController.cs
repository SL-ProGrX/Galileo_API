using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmPreaPrendaMontoController : ControllerBase
    {
        private readonly FrmPreaPrendaMontoBL _bl;
        public FrmPreaPrendaMontoController(IConfiguration config)
        {
            _bl = new FrmPreaPrendaMontoBL(config);
        }

        [HttpGet("CrdPrea_Prendas_Gastos")]
        public ActionResult<ErrorDto<List<PrendaGastoDto>>> CrdPrea_Prendas_Gastos(
            [FromQuery] int codEmpresa,
            [FromQuery] string preanalisis,
            [FromQuery] string tipo)
            => _bl.CrdPrea_Prendas_Gastos(codEmpresa, preanalisis, tipo);

        [HttpPost("CrdPrea_AsignaHonorariosPren")]
        public ActionResult<ErrorDto<PreaAsignaHonorariosPrenResultDto>> CrdPrea_AsignaHonorariosPren(
            [FromQuery] int codEmpresa,
            [FromBody] PreaAsignaHonorariosPrenRequestDto request)
            => _bl.CrdPrea_AsignaHonorariosPren(codEmpresa, request);
    }
}
