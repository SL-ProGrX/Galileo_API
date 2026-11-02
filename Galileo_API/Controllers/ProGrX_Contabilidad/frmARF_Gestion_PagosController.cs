using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Galileo_API.Controllers.ProGrX_Contabilidad;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FrmArfGestionPagosController : ControllerBase
{
    private readonly FrmArfGestionPagosBl _bl;

    public FrmArfGestionPagosController(IConfiguration config)
    {
        _bl = new FrmArfGestionPagosBl(config);
    }

    [HttpGet("FechaServidor")]
    public ErrorDto<DateTime> FechaServidor(int codEmpresa) => _bl.FechaServidor(codEmpresa);

    [ValidateAntiForgeryToken]
    [HttpPost("Consultar")]
    public ErrorDto<List<ArfGestionPagoDto>> Consultar(int codEmpresa, [FromBody] ArfGestionPagosFiltroDto filtro)
    {
        return _bl.Consultar(codEmpresa, filtro, User.FindFirst("UserName")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("Generar")]
    public ErrorDto<int> Generar(int codEmpresa, [FromBody] List<ArfGenerarPagoDto> pagos)
    {
        return _bl.Generar(codEmpresa, pagos, User.FindFirst("UserName")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty);
    }
}
