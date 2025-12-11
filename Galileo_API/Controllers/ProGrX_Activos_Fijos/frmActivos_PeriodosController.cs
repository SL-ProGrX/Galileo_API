using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosPeriodosController : ControllerBase
    {
        private readonly FrmActivosPeriodosBL _bl;
        public FrmActivosPeriodosController(IConfiguration config)
        {
            _bl = new FrmActivosPeriodosBL(config);
        }
             
        [Authorize]
        [HttpGet("Activos_Periodos_Consultar")]
        public ErrorDto<ActivosPeriodosDataLista> Activos_Periodos_Consultar(int CodEmpresa, string estado)
        {
            return _bl.Activos_Periodos_Consultar(CodEmpresa, estado);
        }
    }
}