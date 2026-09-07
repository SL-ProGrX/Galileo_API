using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPgxServiciosController : ControllerBase
    {
        private readonly IConfiguration _config;

        public FrmPgxServiciosController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("Servicio_ObtenerTodos")]
        public ErrorDto<List<ServicioSuscripcion>> Servicio_ObtenerTodos()
        {
            return new FrmPgxServiciosBL(_config).Servicio_ObtenerTodos();
        }


        [HttpPost("Servicio_Insertar")]
        public ErrorDto Servicio_Insertar(ServicioSuscripcion request)
        {
            return new FrmPgxServiciosBL(_config).Servicio_Insertar(request);
        }


        [HttpPost("Servicio_Eliminar")]
        public ErrorDto Servicio_Eliminar(ServicioSuscripcion request)
        {
            return new FrmPgxServiciosBL(_config).Servicio_Eliminar(request);
        }


        [HttpPost("Servicio_Actualizar")]
        public ErrorDto Servicio_Actualizar(ServicioSuscripcion request)
        {
            return new FrmPgxServiciosBL(_config).Servicio_Actualizar(request);
        }
    }
}
