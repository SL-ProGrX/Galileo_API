using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppHitsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AppHitsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("AppHits_ObtenerTodos")]
        // [Authorize]
        public List<AppHits> TiposId_ObtenerTodos()
        {
            return new AppHitsBL(_config).AppHits_ObtenerTodos();
        }


        [HttpPost("AppHits_Insertar")]
        // [Authorize]
        public ErrorDto AppHits_Insertar(AppHits request)
        {
            return new AppHitsBL(_config).AppHits_Insertar(request);
        }

        [HttpPost("AppHits_Eliminar")]
        //[Authorize]
        public ErrorDto AppHits_Eliminar(AppHits request)
        {
            return new AppHitsBL(_config).AppHits_Eliminar(request);
        }


        [HttpPost("AppHits_Actualizar")]
        //[Authorize]
        public ErrorDto AppHits_Actualizar(AppHits request)
        {
            return new AppHitsBL(_config).AppHits_Actualizar(request);
        }
    }
}
