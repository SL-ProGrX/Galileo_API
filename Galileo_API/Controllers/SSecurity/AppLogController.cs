using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppLogController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AppLogController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("AppLog_ObtenerTodos")]
        // [Authorize]
        public List<AppLog> TiposId_ObtenerTodos(int empresa, string ini, string fin)
        {
            return new AppLogBL(_config).AppLog_ObtenerTodos(empresa, ini, fin);
        }
    }
}
