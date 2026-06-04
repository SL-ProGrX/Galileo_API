using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAhConciliadorPatronalController : ControllerBase
    {
        private readonly FrmAhConciliadorPatronalBL _bl;

        public FrmAhConciliadorPatronalController(IConfiguration config)
        {
            _bl = new FrmAhConciliadorPatronalBL(config);
        }

        [HttpGet("Ah_ConciliadorPatronal_Instituciones_Obtener")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> Ah_ConciliadorPatronal_Instituciones_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.Ah_ConciliadorPatronal_Instituciones_Obtener(codEmpresa);
        }

        [HttpPost("Ah_ConciliadorPatronal_Cargado")]
        public static ActionResult<ErrorDto<List<FrmAhConciliadorPatronalHistoricoDto>>> Ah_ConciliadorPatronal_Cargado(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhConciliadorPatronalCargadoRequest request)
        {
            return FrmAhConciliadorPatronalBL.Ah_ConciliadorPatronal_Cargado(codEmpresa, request);
        }

        [HttpPost("Ah_ConciliadorPatronal_Aplicar")]
        public ActionResult<ErrorDto<FrmAhConciliadorPatronalAplicarResponse>> Ah_ConciliadorPatronal_Aplicar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhConciliadorPatronalCargadoRequest request)
        {
            return _bl.Ah_ConciliadorPatronal_Aplicar(codEmpresa, request);
        }

        [HttpPost("Ah_ConciliadorPatronal_Conciliacion_Obtener")]
        public ActionResult<ErrorDto<List<FrmAhConciliadorPatronalConciliacionDto>>> Ah_ConciliadorPatronal_Conciliacion_Obtener(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhConciliadorPatronalConciliacionRequest request)
        {
            return _bl.Ah_ConciliadorPatronal_Conciliacion_Obtener(codEmpresa, request);
        }

        [HttpPost("Ah_ConciliadorPatronal_Resultados_Obtener")]
        public ActionResult<ErrorDto<List<FrmAhConciliadorPatronalResultadoDto>>> Ah_ConciliadorPatronal_Resultados_Obtener(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhConciliadorPatronalResultadosRequest request)
        {
            return _bl.Ah_ConciliadorPatronal_Resultados_Obtener(codEmpresa, request);
        }
    }
}
