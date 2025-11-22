using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmUsAccessEstacionesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public FrmUsAccessEstacionesController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("EstacionesObtener")]
        public List<EstacionDto> EstacionesObtener(int empresaCod)
        {
            return new FrmUsAccessEstacionesBl(_config).EstacionesObtener(empresaCod);
        }


        [HttpPost("EstacionRegistrar")]
        // [Authorize]
        public ErrorDto EstacionRegistrar(EstacionGuardarDto request)
        {
            return new FrmUsAccessEstacionesBl(_config).EstacionRegistrar(request);
        }


        [HttpGet("EstacionesSinVincularObtener")]
        // [Authorize]
        public List<EstacionSinVincularDto> EstacionesSinVincularObtener(int empresaCod)
        {
            return new FrmUsAccessEstacionesBl(_config).EstacionesSinVincularObtener(empresaCod);
        }


        [HttpPost("EstacionVincular")]
        // [Authorize]
        public ErrorDto EstacionVincular(EstacionVinculaDto estacionDto)
        {
            return new FrmUsAccessEstacionesBl(_config).EstacionVincular(estacionDto);
        }


        [HttpPost("EstacionEliminar")]
        // [Authorize]
        public ErrorDto EstacionEliminar(EstacionEliminarDto estacionDto)
        {
            return new FrmUsAccessEstacionesBl(_config).EstacionEliminar(estacionDto);
        }
    }
}
