using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Galileo.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FrmPgxClientesClasificaController : ControllerBase
    {
        readonly FrmPgxClientesClasificaBl ClientesClasificaBL;

        public FrmPgxClientesClasificaController(IConfiguration config)
        {
            ClientesClasificaBL = new FrmPgxClientesClasificaBl(config);
        }

        [HttpGet("ClienteClasifica_ObtenerTodos")]
        [Authorize]
        public ErrorDto<List<ClienteClasifica>> Cliente_Clasifica_ObtenerTodos()
        {
            return ClientesClasificaBL.Cliente_Clasifica_ObtenerTodos();
        }


        [HttpPost("ClienteClasifica_Guardar")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ErrorDto Cliente_Clasifica_Guardar(ClienteClasifica request)
        {
            return ClientesClasificaBL.Cliente_Clasifica_Guardar(request);
        }


        [HttpDelete("ClienteClasifica_Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ErrorDto Cliente_Clasifica_Eliminar(string request, int codEmpresa = 0, string usuario = "")
        {
            return ClientesClasificaBL.Cliente_Clasifica_Eliminar(request, codEmpresa, usuario);
        }


        [HttpGet("Cliente_Selecciona_ObtenerTodos")]
        [Authorize]
        public ActionResult<List<ClienteSelecciona>> Cliente_Selecciona_ObtenerTodos(string usuario)
        {
            var usuarioAutenticado = User.FindFirst("UserName")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(usuarioAutenticado) ||
                !string.Equals(usuarioAutenticado.Trim(), usuario?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return Ok(ClientesClasificaBL.Cliente_Selecciona_ObtenerTodos(usuarioAutenticado));
        }

        [HttpGet("Cliente_Selecciona_ObtenerPagina")]
        [Authorize]
        public ActionResult<ClienteSeleccionaPagina> Cliente_Selecciona_ObtenerPagina(
            [FromQuery] string? usuario,
            [FromQuery] string? filtro = "",
            [FromQuery] int pagina = 1)
        {
            var usuarioAutenticado = User.FindFirst("UserName")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(usuarioAutenticado) ||
                string.IsNullOrWhiteSpace(usuario) ||
                !string.Equals(usuarioAutenticado.Trim(), usuario.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            if (pagina < 1)
            {
                return BadRequest("El número de página debe ser mayor que cero.");
            }

            return Ok(ClientesClasificaBL.Cliente_Selecciona_ObtenerPagina(usuarioAutenticado, filtro, pagina));
        }
    }
}
