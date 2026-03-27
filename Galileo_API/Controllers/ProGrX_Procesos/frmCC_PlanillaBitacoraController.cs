using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCCPlanillaBitacoraController : ControllerBase
    {
        private readonly FrmCCPlanillaBitacoraBL BL;

        public FrmCCPlanillaBitacoraController(IConfiguration config)
        {
            BL = new FrmCCPlanillaBitacoraBL(config);
        }
        [Authorize]
        [HttpGet("CC_Instituciones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CC_Instituciones_Dropdown_Obtener(CodEmpresa);
        }
        //[Authorize]
        //[HttpGet("CC_PlanillaBitacora_Proceso_Scroll_Obtener")]
        //public ErrorDto<CcPlanillaProcesoScrollDto> CC_PlanillaBitacora_Proceso_Scroll_Obtener(int CodEmpresa, int scrollCode, decimal procesoActual)
        //{
        //    return BL.CC_PlanillaBitacora_Proceso_Scroll_Obtener(CodEmpresa, scrollCode, procesoActual);
        //}
        [Authorize]
        [HttpGet("CC_PlanillaBitacora_Lista_Obtener")]
        public ErrorDto<CcPlanillaBitacoraListaResult> CC_PlanillaBitacora_Lista_Obtener(int CodEmpresa, decimal proceso, string parametros)
        {
            return BL.CC_PlanillaBitacora_Lista_Obtener(CodEmpresa, proceso, parametros);
        }
        [Authorize]
        [HttpGet("CC_PlanillaBitacora_Lista_Export")]
        public ErrorDto<CcPlanillaBitacoraListaResult> CC_PlanillaBitacora_Lista_Export(int CodEmpresa, decimal proceso, string parametros)
        {
            return BL.CC_PlanillaBitacora_Lista_Export(CodEmpresa, proceso, parametros);
        }
    }
}