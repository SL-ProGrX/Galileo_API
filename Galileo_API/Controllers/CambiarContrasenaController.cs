using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CambiarContrasenaController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CambiarContrasenaController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("ParametrosObtener")]
        public ErrorDto<ParametrosObtenerDto> ParametrosObtener()
        {
            return new CambiarContrasenaBL(_config).ParametrosObtener();
        }

        [HttpGet("KeyHistoryObtener")]
        public ErrorDto<List<string>> KeyHistoryObtener(string Usuario, int topQuantity)
        {
            return new CambiarContrasenaBL(_config).KeyHistoryObtener(Usuario, topQuantity);
        }

        [HttpPatch("CambiarClave")]
        public ErrorDto CambiarClave(ClaveCambiarDto cambioClave)
        {
            return new CambiarContrasenaBL(_config).CambiarClave(cambioClave);
        }
    }
}
