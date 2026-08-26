using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Conciliacion
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmConTransitoriasController : ControllerBase
    {
        private readonly FrmConTransitoriasBl _bl;

        public FrmConTransitoriasController(IConfiguration config)
        {
            _bl = new FrmConTransitoriasBl(config);
        }

        [HttpGet("Conciliacion_ConTransitorias_Inicializar")]
        public ErrorDto<ConTransitoriasInicializaData>
            Conciliacion_ConTransitorias_Inicializar(int codEmpresa)
        {
            return _bl.Conciliacion_ConTransitorias_Inicializar(codEmpresa);
        }

        [HttpGet("Conciliacion_ConTransitorias_Consultar")]
        public ErrorDto<List<ConTransitoriasData>>
            Conciliacion_ConTransitorias_Consultar(
                int codEmpresa,
                string request)
        {
            return _bl.Conciliacion_ConTransitorias_Consultar(
                codEmpresa,
                request);
        }
    }
}
