using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UsuarioController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("UsuarioCuentaRevisar")]
        //[Authorize]
        public ErrorDto UsuarioCuentaRevisar(UsuarioCuentaRevisarDto usuarioCuentaRevisarDto)
        {
            return new UsuarioBL(_config).UsuarioCuentaRevisar(usuarioCuentaRevisarDto);
        }

        [HttpGet("UsuarioCuentaObtener")]
        //[Authorize]
        public UsuarioCuentaRevisarDto UsuarioCuentaObtener(string nombreUsuario)
        {
            return new UsuarioBL(_config).UsuarioCuentaObtener(nombreUsuario);
        }

        [HttpPost("UsuarioCuentaMovimientosObtener")]
        //[Authorize]
        public List<UsuarioCuentaMovimientoResultDto> UsuarioCuentaMovimientosObtener([FromBody] UsuarioCuentaMovimientoRequestDto usuarioCuentaMovimientoRequestDto)
        {
            return new UsuarioBL(_config).UsuarioCuentaMovimientosObtener(usuarioCuentaMovimientoRequestDto);
        }
    }
}