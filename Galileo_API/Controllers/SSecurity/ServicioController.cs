using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicioController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ServicioController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("Servicio_ObtenerTodos")]
        // [Authorize]
        public List<ServicioSuscripcion> Servicio_ObtenerTodos()
        {
            return new ServicioBL(_config).Servicio_ObtenerTodos();
        }


        [HttpPost("Servicio_Insertar")]
        // [Authorize]
        public ErrorDto Servicio_Insertar(ServicioSuscripcion request)
        {
            return new ServicioBL(_config).Servicio_Insertar(request);
        }


        [HttpPost("Servicio_Eliminar")]
        //[Authorize]
        public ErrorDto Servicio_Eliminar(ServicioSuscripcion request)
        {
            return new ServicioBL(_config).Servicio_Eliminar(request);
        }


        [HttpPost("Servicio_Actualizar")]
        //[Authorize]
        public ErrorDto Servicio_Actualizar(ServicioSuscripcion request)
        {
            return new ServicioBL(_config).Servicio_Actualizar(request);
        }
    }
}