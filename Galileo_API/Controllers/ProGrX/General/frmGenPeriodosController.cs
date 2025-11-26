using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.Controllers
{
    [Route("api/frmGenPeriodos")]
    [ApiController]
    public class FrmGenPeriodosController : ControllerBase
    {

        readonly FrmGenPeriodosBl _bl;
        public FrmGenPeriodosController(IConfiguration config)
        {
            _bl = new FrmGenPeriodosBl(config);
        }

        [HttpGet("Periodos_ObtenerTodos")]
        //[Authorize]
        public ErrorDto<List<PeriodoDto>> Periodos_ObtenerTodos(int CodEmpresa, string estado)
        {
            return _bl.Periodos_ObtenerTodos(CodEmpresa, estado);
        }

        [HttpPost("Periodo_Cerrar")]
        public ErrorDto Periodo_Cerrar(int CodEmpresa, PeriodoDto periodoDto)
        {
            return _bl.Periodo_Cerrar(CodEmpresa, periodoDto);
        }
    }
}
