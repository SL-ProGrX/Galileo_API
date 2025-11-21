using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CambiarContrasenaController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CambiarContrasenaController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("ParametrosObtener")]
        //[Authorize]
        public ParametrosObtenerDto ParametrosObtener()
        {
            return new CambiarContrasenaBL(_config).ParametrosObtener();
        }

        [HttpGet("KeyHistoryObtener")]
        //[Authorize]
        public List<string> KeyHistoryObtener(string Usuario, int topQuantity)
        {
            return new CambiarContrasenaBL(_config).KeyHistoryObtener(Usuario, topQuantity);
        }

        [HttpPatch("CambiarClave")]
        //[Authorize]
        public ErrorDto CambiarClave(ClaveCambiarDto cambioClave)
        {
            return new CambiarContrasenaBL(_config).CambiarClave(cambioClave);
        }
    }
}
