using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.Security;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivacionCuentaController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ActivacionCuentaController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene el estado de un usuario
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("UsuarioEstado_Obtener")]
        // [Authorize]
        public ActivacionCuentaDto UsuarioEstado_Obtener(string user)
        {
            return new ActivacionCuentaBL(_config).UsuarioEstado_Obtener(user);
        }


        /// <summary>
        /// Actualiza el estado de un usuario
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        /// Ejemplo prueba:
        ///
        ///     POST 
        ///     {
        ///         "userId": 4,
        ///         "usuarioActual": "pedro",
        ///         "usuarioAfectado": "userTest1",
        ///         "estado": "A",
        ///         "notas": "string"
        ///     }
        ///
        /// </remarks>
        /// <returns></returns>
        [HttpPost("UsuarioEstado_Actualizar")]
        //[Authorize]
        public ErrorDto UsuarioEstado_Actualizar(ActivacionCuentaDto request)
        {
            return new ActivacionCuentaBL(_config).UsuarioEstado_Actualizar(request);
        }
    }
}
