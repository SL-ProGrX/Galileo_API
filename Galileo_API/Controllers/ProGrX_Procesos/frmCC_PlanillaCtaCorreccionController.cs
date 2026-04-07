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
    public class FrmCcPlanillaCtaCorreccionController : ControllerBase
    {
        private readonly FrmCcPlanillaCtaCorreccionBL BL;

        public FrmCcPlanillaCtaCorreccionController(IConfiguration config)
        {
            BL = new FrmCcPlanillaCtaCorreccionBL(config);
        }

        [Authorize]
        [HttpGet("CC_PlanillaCtaCorreccion_Instituciones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaCtaCorreccion_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CC_PlanillaCtaCorreccion_Instituciones_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CC_PlanillaCtaCorreccion_Proceso_Scroll_Obtener")]
        public ErrorDto<CcPlanillaCtaCorreccionProcesoScrollDto> CC_PlanillaCtaCorreccion_Proceso_Scroll_Obtener(int CodEmpresa, int scrollCode, decimal procesoActual)
        {
            return BL.CC_PlanillaCtaCorreccion_Proceso_Scroll_Obtener(CodEmpresa, scrollCode, procesoActual);
        }

        [Authorize]
        [HttpGet("CC_PlanillaCtaCorreccion_Personas_F4_Obtener")]
        public ErrorDto<List<CcPlanillaCtaCorreccionPersonaF4Dto>> CC_PlanillaCtaCorreccion_Personas_F4_Obtener(int CodEmpresa, string? texto)
        {
            return BL.CC_PlanillaCtaCorreccion_Personas_F4_Obtener(CodEmpresa, texto);
        }

        [Authorize]
        [HttpPost("CC_PlanillaCtaCorreccion_Lista_Obtener")]
        public ErrorDto<CcPlanillaCtaCorreccionListaResult> CC_PlanillaCtaCorreccion_Lista_Obtener(int CodEmpresa, CcPlanillaCtaCorreccionListaRequest parametros)
        {
            return BL.CC_PlanillaCtaCorreccion_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CC_PlanillaCtaCorreccion_Lista_Export")]
        public ErrorDto<CcPlanillaCtaCorreccionListaResult> CC_PlanillaCtaCorreccion_Lista_Export(int CodEmpresa, CcPlanillaCtaCorreccionListaRequest parametros)
        {
            return BL.CC_PlanillaCtaCorreccion_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CC_PlanillaCtaCorreccion_Cuota_Actualizar")]
        public ErrorDto CC_PlanillaCtaCorreccion_Cuota_Actualizar(int CodEmpresa, CcPlanillaCtaCorreccionActualizarCuotaRequest parametros)
        {
            return BL.CC_PlanillaCtaCorreccion_Cuota_Actualizar(CodEmpresa, parametros);
        }
    }
}