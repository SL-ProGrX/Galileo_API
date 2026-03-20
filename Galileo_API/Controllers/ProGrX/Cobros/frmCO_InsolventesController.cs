using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoInsolventesModels;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOInsolventesController : ControllerBase
    {

        private readonly FrmCOInsolventesBL _bl;

        public FrmCOInsolventesController(IConfiguration config)
            => _bl = new FrmCOInsolventesBL(config);

        [Authorize]
        [HttpGet("CoInsolventes_Buscar")]
        public ErrorDto<List<CbrInsolventeGridItem>> CoInsolventes_Buscar(int codEmpresa, [FromQuery] CbrInsolventesBuscarRequest request)
                => _bl.CoInsolventes_Buscar(codEmpresa, request);

        [Authorize]
        [HttpPost("CoInsolventes_Registrar")]
        public ErrorDto<CbrSpMovimientoResult> CoInsolventes_Registrar(
                 int codEmpresa,
                  [FromBody] CbrInsolventeRegistrarRequest request,
                 string usuario)
             => _bl.CoInsolventes_Registrar(codEmpresa, request, usuario);

        [Authorize]
        [HttpPost("CoInsolventes_Reversar")]
        public ErrorDto<CbrSpMovimientoResult> CoInsolventes_Reversar(
             int codEmpresa,
              [FromBody] CbrInsolventeRegistrarRequest request,
             string usuario)
           => _bl.CoInsolventes_Reversar(codEmpresa, request, usuario);

        [Authorize]
        [HttpGet("CoInsolventes_Socios_Obtener")]
        public ErrorDto<List<CbrInsolventeSocioResult>> CoInsolventes_Socios_Obtener(int codEmpresa)
            => _bl.CoInsolventes_Socios_Obtener(codEmpresa);
    }
}
