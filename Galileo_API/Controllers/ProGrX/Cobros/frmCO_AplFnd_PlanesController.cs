using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOAplFndPlanesController : ControllerBase
    {
        private readonly FrmCOAplFndPlanesBL _bl;

        public FrmCOAplFndPlanesController(IConfiguration config)
        {
            _bl = new FrmCOAplFndPlanesBL(config);
        }

        [Authorize]
        [HttpGet("FondosAplConfigPrioridades_Lista")]
        public ErrorDto<List<FondosAplConfigPrioridadResult>> FondosAplConfigPrioridades_Lista([FromQuery] int codEmpresa)
        {
            return _bl.FondosAplConfigPrioridades_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("FondosAplConfigFondosDisponibles_Lista")]
        public ErrorDto<List<FondosAplConfigFondoDisponibleResult>> FondosAplConfigFondosDisponibles_Lista([FromQuery] int codEmpresa)
        {
            return _bl.FondosAplConfigFondosDisponibles_Lista(codEmpresa);
        }

        [Authorize]
        [HttpPost("FondosAplConfigPrioridad_Add")]
        public ErrorDto<FondosAplConfigPrioridadAddResult?> FondosAplConfigPrioridad_Add([FromQuery] int codEmpresa, [FromBody] FondosAplConfigPrioridadAddParams param)
        {
            return _bl.FondosAplConfigPrioridad_Add(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("FondosAplConfigPrioridad_Del")]
        public ErrorDto<FondosAplConfigPrioridadDelResult?> FondosAplConfigPrioridad_Del([FromQuery] int codEmpresa, [FromBody] FondosAplConfigPrioridadDelParams param)
        {
            return _bl.FondosAplConfigPrioridad_Del(codEmpresa, param);
        }
    }
}
