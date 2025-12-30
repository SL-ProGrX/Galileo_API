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
    public class ParametrosController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ParametrosController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("Parametros_Obtener")]
        public ParametrosDto Parametros_Obtener()
        {
            return new ParametrosBL(_config).Parametros_Obtener();
        }


        [HttpPost("Parametros_Insertar")]
        public ErrorDto Parametros_Insertar(ParametrosDto request)
        {
            return new ParametrosBL(_config).Parametros_Insertar(request);
        }
    }
}
