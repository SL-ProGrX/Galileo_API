using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOControlListaController : ControllerBase
    {
        private readonly FrmCOControlListaBL _bl;

        public FrmCOControlListaController(IConfiguration config)
        {
            _bl = new FrmCOControlListaBL(config);
        }

        [Authorize]
        [HttpGet("CoControlLista_Buscar")]
        public ErrorDto<CoControlListaBuscarResponse> CoControlLista_Buscar(
            int codEmpresa,
            [FromQuery] CoControlListaBuscarRequest request)
        {
            return _bl.CoControlLista_Buscar(codEmpresa, request);
        }
    }
}
