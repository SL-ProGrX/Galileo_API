using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    /// <summary>
    /// Entrega el token anti-forgery (PILOTO CSRF). El cliente debe enviarlo en el header
    /// X-XSRF-TOKEN en las operaciones que modifican datos.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public sealed class AntiforgeryController : ControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        /// <summary>
        /// Inicializa el controlador con el servicio anti-forgery.
        /// </summary>
        /// <param name="antiforgery"></param>
        public AntiforgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        /// <summary>
        /// Genera el token, deja la cookie asociada en la respuesta y devuelve el token de solicitud.
        /// </summary>
        /// <returns></returns>
        [HttpGet("token")]
        [ProducesResponseType(typeof(AntiforgeryTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ObtenerToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

            return Ok(new AntiforgeryTokenResponse(tokens.RequestToken));
        }
    }

    /// <summary>
    /// Respuesta con el token de solicitud anti-forgery.
    /// </summary>
    /// <param name="Token">Token que el cliente envía en el header X-XSRF-TOKEN.</param>
    public sealed record AntiforgeryTokenResponse(string? Token);
}
