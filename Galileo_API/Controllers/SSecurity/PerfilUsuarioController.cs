using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    /// <summary>
    /// Controlador para la gestión del perfil de usuario, incluyendo la obtención y actualización de información del perfil.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PerfilUsuarioController : ControllerBase
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de perfil de usuario.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public PerfilUsuarioController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la información del perfil de usuario para el usuario especificado.
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        [HttpGet("PerfilUsuario_Obtener")]
        public ErrorDto<PerfilUsuarioDto> PerfilUsuario_Obtener(string usuario)
        {
            return new PerfilUsuarioBL(_config).PerfilUsuario_Obtener(usuario);
        }

        /// <summary>
        /// Actualiza la información del perfil de usuario con los datos proporcionados en la solicitud.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("PerfilUsuario_Actualizar")]
        [Authorize]
        public ErrorDto PerfilUsuario_Actualizar(PerfilUsuarioDto request)
        {
            return new PerfilUsuarioBL(_config).PerfilUsuario_Actualizar(request);
        }
    }
}