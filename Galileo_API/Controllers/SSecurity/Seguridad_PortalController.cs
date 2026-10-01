using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Galileo.Models;

namespace Galileo.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class SeguridadPortalController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly ILogger<SeguridadPortalController> _logger;

        public SeguridadPortalController(IConfiguration config, ILogger<SeguridadPortalController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost("sbAdmin_Rols_Load")]
        [ProducesResponseType(typeof(Galileo.Models.AdminAccessDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult sbAdmin_Rols_Load([FromQuery] string? pUsuario, [FromQuery] int EmpresaId)
        {
            var usuario = pUsuario?.Trim();
            if (string.IsNullOrWhiteSpace(usuario) || usuario.Length > 50 || EmpresaId < 0)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "La solicitud de permisos no es válida.");
            }

            var usuarioAutenticado = User.FindFirst("UserName")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(usuarioAutenticado) ||
                !string.Equals(usuario, usuarioAutenticado.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            try
            {
                return Ok(new SeguridadPortalBl(_config).sbAdmin_Rols_Load(usuario, EmpresaId));
            }
            catch (SqlException ex)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex,
                    "Error SQL validando permisos administrativos de SSecurity. EmpresaId={EmpresaId}, SqlErrorNumber={SqlErrorNumber}, TraceId={TraceId}",
                    EmpresaId,
                    ex.Number,
                    traceId);

                var detail = ex.Number == 229
                    ? $"Permisos SQL insuficientes (código {ex.Number}): {ex.Message}"
                    : $"Error de SQL Server (código {ex.Number}): {ex.Message}";

                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "No fue posible validar los permisos administrativos.",
                    detail: $"{detail} Referencia: {traceId}.");
            }
            catch (Exception ex)
            {
                var traceId = HttpContext.TraceIdentifier;
                _logger.LogError(ex,
                    "Error validando permisos administrativos de SSecurity. EmpresaId={EmpresaId}, TraceId={TraceId}",
                    EmpresaId,
                    traceId);

                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "No fue posible validar los permisos administrativos.",
                    detail: $"La consulta de permisos falló. Referencia: {traceId}.");
            }
        }


        [HttpPost("sbSIFMenuOptionClick")]
        public string sbSIFMenuOptionClick(int pNodo, int Cliente, string Usuario)
        {
            return new SeguridadPortalBl(_config).sbSIFMenuOptionClick(pNodo, Cliente, Usuario);
        }
    }
}
