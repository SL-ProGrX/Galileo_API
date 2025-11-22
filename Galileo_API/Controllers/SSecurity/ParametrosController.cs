using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParametrosController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ParametrosController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("Parametros_Obtener")]
        // [Authorize]
        public ParametrosDto Parametros_Obtener()
        {
            return new ParametrosBL(_config).Parametros_Obtener();
        }


        [HttpPost("Parametros_Insertar")]
        // [Authorize]
        public ErrorDto Parametros_Insertar(ParametrosDto request)
        {
            return new ParametrosBL(_config).Parametros_Insertar(request);
        }
    }
}
