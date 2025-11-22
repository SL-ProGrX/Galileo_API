using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BDAnalisisController : ControllerBase
    {

        public BDAnalisisController(IConfiguration config)
        {
        }

        [HttpGet("PaisObtener")]
        public List<string> TablasCargar()
        {
            return BDAnalisisBL.TablasCargar();
        }

    }
}
