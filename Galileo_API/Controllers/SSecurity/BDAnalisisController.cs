using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
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

        [HttpGet("PaisObtener")]
        [Authorize]
        public List<string> TablasCargar()
        {
            var bl = new BDAnalisisBL(_config);
            return bl.TablasCargar();
        }

    }
}
