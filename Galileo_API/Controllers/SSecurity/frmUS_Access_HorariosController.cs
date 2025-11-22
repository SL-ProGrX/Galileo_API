using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmUsAccessHorariosController : ControllerBase
    {
        private readonly IConfiguration _config;

        public FrmUsAccessHorariosController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("HorariosObtener")]
        public List<HorarioDto> HorariosObtener(int empresaId)
        {
            return new FrmUsAccessHorariosBl(_config).HorariosObtener(empresaId);
        }

        [HttpPost("HorarioRegistrar")]
        // [Authorize]
        public ErrorDto HorarioRegistrar(HorarioDto request)
        {
            return new FrmUsAccessHorariosBl(_config).HorarioRegistrar(request);
        }

        [HttpPost("HorarioEliminar")]
        //[Authorize]
        public ErrorDto HorarioEliminar(HorarioDto request)
        {
            return new FrmUsAccessHorariosBl(_config).HorarioEliminar(request);
        }
    }
}