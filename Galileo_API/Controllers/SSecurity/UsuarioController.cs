using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UsuarioController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("UsuarioCuentaRevisar")]
        public ErrorDto UsuarioCuentaRevisar(UsuarioCuentaRevisarDto usuarioCuentaRevisarDto)
        {
            return new UsuarioBL(_config).UsuarioCuentaRevisar(usuarioCuentaRevisarDto);
        }

        [HttpGet("UsuarioCuentaObtener")]
        public UsuarioCuentaRevisarDto? UsuarioCuentaObtener(string nombreUsuario)
        {
            return new UsuarioBL(_config).UsuarioCuentaObtener(nombreUsuario);
        }

        [HttpPost("UsuarioCuentaMovimientosObtener")]
        public List<UsuarioCuentaMovimientoResultDto> UsuarioCuentaMovimientosObtener([FromBody] UsuarioCuentaMovimientoRequestDto usuarioCuentaMovimientoRequestDto)
        {
            return new UsuarioBL(_config).UsuarioCuentaMovimientosObtener(usuarioCuentaMovimientoRequestDto);
        }

        [HttpPost("UsuarioCuentaMovimientoRevisar")]
        public ErrorDto UsuarioCuentaMovimientoRevisar([FromBody] UsuarioCuentaMovimientoRevisarDto movimiento)
        {
            return new UsuarioBL(_config).UsuarioCuentaMovimientoRevisar(movimiento);
        }
    }
}
