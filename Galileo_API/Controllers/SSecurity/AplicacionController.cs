using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AplicacionController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AplicacionController(IConfiguration config)
        {
            _config = config;
        }


        #region MÉTODOS APP_BANK

        [HttpGet("Aplicacion_ObtenerTodos")]
        // [Authorize]
        public List<Aplicacion> Aplicacion_ObtenerTodos()
        {
            return new AplicacionBL(_config).Aplicacion_ObtenerTodos();
        }

        [HttpPost("Aplicacion_Insertar")]
        // [Authorize]
        public ErrorDto Aplicacion_Insertar(Aplicacion request)
        {
            return new AplicacionBL(_config).Aplicacion_Insertar(request);
        }

        [HttpPost("Aplicacion_Actualizar")]
        //[Authorize]
        public ErrorDto Aplicacion_Actualizar(Aplicacion request)
        {
            return new AplicacionBL(_config).Aplicacion_Actualizar(request);
        }

        #endregion


        #region MÉTODOS APP_BLOCK

        [HttpGet("Bloqueo_ObtenerTodos")]
        // [Authorize]
        public List<Bloqueo> Bloqueo_ObtenerTodos(string Cod_App)
        {
            return new AplicacionBL(_config).Bloqueo_ObtenerTodos(Cod_App);
        }

        [HttpPost("Bloqueo_Insertar")]
        // [Authorize]
        public ErrorDto Bloqueo_Insertar(Bloqueo request)
        {
            return new AplicacionBL(_config).Bloqueo_Insertar(request);
        }

        [HttpPost("Bloqueo_Eliminar")]
        //[Authorize]
        public ErrorDto Bloqueo_Eliminar(Bloqueo request)
        {
            return new AplicacionBL(_config).Bloqueo_Eliminar(request);
        }

        #endregion


        #region MÉTODOS APP_UPDATES

        [HttpGet("Actualizacion_ObtenerTodos")]
        // [Authorize]
        public List<Actualizacion> Actualizacion_ObtenerTodos(string Cod_App)
        {
            return new AplicacionBL(_config).Actualizacion_ObtenerTodos(Cod_App);
        }


        [HttpPost("Actualizacion_Insertar")]
        // [Authorize]
        public ErrorDto Actualizacion_Insertar(Actualizacion request)
        {
            return new AplicacionBL(_config).Actualizacion_Insertar(request);
        }

        [HttpPost("Actualizacion_Eliminar")]
        //[Authorize]
        public ErrorDto Actualizacion_Eliminar(Actualizacion request)
        {
            return new AplicacionBL(_config).Actualizacion_Eliminar(request);
        }

        #endregion
    }
}
