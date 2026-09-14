using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BDAnalisisController : ControllerBase
    {
        private readonly IConfiguration _config;

        public BDAnalisisController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("TablasObtener")]
        [HttpGet("PaisObtener")] // Ruta anterior conservada para compatibilidad.
        [Authorize]
        public ErrorDto<List<string>> TablasCargar()
        {
            var bl = new BDAnalisisBL(_config);
            return bl.TablasCargar();
        }

        [HttpGet("ResultadosObtener")]
        [Authorize]
        public ErrorDto<List<Dictionary<string, object?>>> ResultadosObtener(string objeto)
        {
            return new BDAnalisisBL(_config).ResultadosObtener(objeto);
        }

        [HttpGet("EstructuraObtener")]
        [Authorize]
        public ErrorDto<List<BDAnalisisEstructuraDto>> EstructuraObtener(string objeto)
        {
            return new BDAnalisisBL(_config).EstructuraObtener(objeto);
        }

    }
}
