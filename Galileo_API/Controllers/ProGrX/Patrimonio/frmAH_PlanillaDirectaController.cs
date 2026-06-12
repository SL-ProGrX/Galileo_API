using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Galileo_API.Models.ProGrX;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAhPlanillaDirectaController : ControllerBase
    {
        private readonly FrmAhPlanillaDirectaBL _bl;

        public FrmAhPlanillaDirectaController(IConfiguration config)
        {
            _bl = new FrmAhPlanillaDirectaBL(config);
        }

        [HttpGet("Ah_PlanillaDirecta_Instituciones_Obtener")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> Ah_PlanillaDirecta_Instituciones_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.Ah_PlanillaDirecta_Instituciones_Obtener(codEmpresa);
        }

        [HttpGet("Ah_PlanillaDirecta_Periodos_Obtener")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> Ah_PlanillaDirecta_Periodos_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.Ah_PlanillaDirecta_Periodos_Obtener(codEmpresa);
        }

        [HttpGet("Ah_PlanillaDirecta_Comprobante_Obtener")]
        public ActionResult<ErrorDto<string>> Ah_PlanillaDirecta_Comprobante_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int codInstitucion,
            [FromQuery] int proceso,
            [FromQuery] string tipoAporte)
        {
            return _bl.Ah_PlanillaDirecta_Comprobante_Obtener(
                codEmpresa,
                codInstitucion,
                proceso,
                tipoAporte);
        }

        [HttpPost("Ah_PlanillaDirecta_Cargado")]
        public ActionResult<ErrorDto<List<FrmAhPlanillaDirectaCargadoDto>>> Ah_PlanillaDirecta_Cargado(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhPlanillaDirectaCargadoRequest request)
        {
            return _bl.Ah_PlanillaDirecta_Cargado(codEmpresa, request);
        }

        [HttpGet("Ah_PlanillaDirecta_Inconsistencias_Obtener")]
        public ActionResult<ErrorDto<List<FrmAhPlanillaDirectaInconsistenciaDto>>> Ah_PlanillaDirecta_Inconsistencias_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string numDoc)
        {
            return _bl.Ah_PlanillaDirecta_Inconsistencias_Obtener(codEmpresa, numDoc);
        }

        [HttpPost("Ah_PlanillaDirecta_Procesar")]
        public ActionResult<ErrorDto<FrmAhPlanillaDirectaProcesarResponse>> Ah_PlanillaDirecta_Procesar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhPlanillaDirectaProcesarRequest request)
        {
            return _bl.Ah_PlanillaDirecta_Procesar(codEmpresa, request);
        }
    }
}
