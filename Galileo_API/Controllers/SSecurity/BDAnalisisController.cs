using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;

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

        [HttpGet("PaisObtener")]
        public List<string> TablasCargar()
        {
            var bl = new BDAnalisisBL(_config);
            return bl.TablasCargar();
        }

    }
}
