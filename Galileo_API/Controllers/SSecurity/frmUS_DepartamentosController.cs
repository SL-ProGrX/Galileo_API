using Galileo.BusinessLogic;
using Galileo.Models.Auth;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Galileo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class FrmUsDepartamentosController(IConfiguration configuration, ILogger<FrmUsDepartamentosController> logger) : ControllerBase
{
    private readonly FrmUsDepartamentosBl _departamentos = new(configuration);

    [HttpGet("Departamento_ObtenerTodos")]
    public ActionResult<ErrorDto<List<Departamento>>> Departamento_ObtenerTodos()
        => Execute(true, _ => _departamentos.Departamento_ObtenerTodos());

    [HttpGet("Departamento_CatalogoObtener")]
    public ActionResult<ErrorDto<List<Departamento>>> Departamento_CatalogoObtener([FromQuery] string? codigoActual = null, [FromQuery] int codEmpresa = 0)
        => Execute(false, _ => _departamentos.Departamento_ObtenerTodos(codigoActual, true), codEmpresa);

    [HttpPost("Departamento_Insertar")]
    [ValidateAntiForgeryToken]
    public ActionResult<ErrorDto> Departamento_Insertar([FromBody] DepartamentoRequest request)
        => Execute(true, actor => _departamentos.Departamento_Guardar(request, actor, true));

    [HttpPost("Departamento_Actualizar")]
    [ValidateAntiForgeryToken]
    public ActionResult<ErrorDto> Departamento_Actualizar([FromBody] DepartamentoRequest request)
        => Execute(true, actor => _departamentos.Departamento_Guardar(request, actor, false));

    [HttpDelete("Departamento_Eliminar")]
    [ValidateAntiForgeryToken]
    public ActionResult<ErrorDto> Departamento_Eliminar([FromQuery] string codigo)
        => Execute(true, _ => _departamentos.Departamento_Eliminar(codigo));

    private ActionResult<T> Execute<T>(bool requireAdministrator, Func<string, T> action, int codEmpresa = 0)
    {
        try
        {
            var actor = User.FindFirst("UserName")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(actor) || actor.Length > 30 ||
                !string.Equals(User.FindFirst("app")?.Value, AuthApplications.SSecurity, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            if (codEmpresa < 0)
                return BadRequest();
            var permissions = new SeguridadPortalBl(configuration).sbAdmin_Rols_Load(actor, codEmpresa);
            var authorized = permissions.Admin_Portal ||
                (!requireAdministrator && codEmpresa > 0 && permissions.Rol_LocalUsers);
            if (!authorized)
                return Forbid();

            return Ok(action(actor.Trim()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No fue posible completar la operación de departamentos de SSecurity.");
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "No fue posible completar la operación de departamentos.", detail: ex.GetBaseException().Message);
        }
    }
}
