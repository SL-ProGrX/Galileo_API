using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerfilUsuarioController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PerfilUsuarioController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("PerfilUsuario_Obtener")]
        // [Authorize]
        public ErrorDto<PerfilUsuarioDto> PerfilUsuario_Obtener(string usuario)
        {
            return new PerfilUsuarioBL(_config).PerfilUsuario_Obtener(usuario);
        }

        [HttpPost("PerfilUsuario_Actualizar")]
        // [Authorize]
        public ErrorDto PerfilUsuario_Actualizar(PerfilUsuarioDto request)
        {
            return new PerfilUsuarioBL(_config).PerfilUsuario_Actualizar(request);
        }
    }
}