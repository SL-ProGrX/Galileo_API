using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.BusinessLogic.ProGrX_Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasCorrientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCCPlanillaReportesController : ControllerBase
    {
        private readonly FrmCCPlanillaReportesBL BL;

        public FrmCCPlanillaReportesController(IConfiguration config)
        {
            BL = new FrmCCPlanillaReportesBL(config);
        }
        [Authorize]
        [HttpGet("CC_PlanillaReportes_Catalogo_Obtener")]
        public ErrorDto<List<CcPlanillaReporteCatalogoDto>> CC_PlanillaReportes_Catalogo_Obtener(int CodEmpresa)
        {
            return BL.CC_PlanillaReportes_Catalogo_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CC_PlanillaReportes_TiposReporte_Obtener")]
        public ErrorDto<List<CcPlanillaReporteTipoDto>> CC_PlanillaReportes_TiposReporte_Obtener(int CodEmpresa, string? codigoOpcion)
        {
            return FrmCCPlanillaReportesBL.CC_PlanillaReportes_TiposReporte_Obtener(CodEmpresa, codigoOpcion);
        }
        [Authorize]
        [HttpGet("CC_PlanillaReportes_ParametrosIniciales_Obtener")]
        public ErrorDto<CcPlanillaReportesParametrosInicialesDto> CC_PlanillaReportes_ParametrosIniciales_Obtener(
            int CodEmpresa,
            int codInstitucion)
        {
            return BL.CC_PlanillaReportes_ParametrosIniciales_Obtener(CodEmpresa, codInstitucion);
        }
        [Authorize]
        [HttpGet("CC_Instituciones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CC_Instituciones_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CC_PlanillaReportes_Institucion_Obtener")]
        public ErrorDto<CcPlanillaInstitucionInfoDto> CC_PlanillaReportes_Institucion_Obtener(int CodEmpresa, int codInstitucion)
        {
            return BL.CC_PlanillaReportes_Institucion_Obtener(CodEmpresa, codInstitucion);
        }
        [Authorize]
        [HttpGet("CC_PlanillaReportes_Proceso_Scroll_Obtener")]
        public ErrorDto<CcPlanillaProcesoScrollDto> CC_PlanillaReportes_Proceso_Scroll_Obtener(int CodEmpresa, int scrollCode, decimal procesoActual)
        {
            return BL.CC_PlanillaReportes_Proceso_Scroll_Obtener(CodEmpresa, scrollCode, procesoActual);
        }
        [Authorize]
        [HttpGet("CC_PlanillaReportes_Lineas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaReportes_Lineas_Dropdown_Obtener(
            int CodEmpresa,
            decimal proceso,
            int? codInstitucion,
            bool todasInstituciones)
        {
            return BL.CC_PlanillaReportes_Lineas_Dropdown_Obtener(CodEmpresa, proceso, codInstitucion, todasInstituciones);
        }
        [Authorize]
        [HttpGet("CC_PlanillaReportes_TiposCobro_Obtener")]
        public  ErrorDto<List<CcPlanillaTipoCobroDto>> CC_PlanillaReportes_TiposCobro_Obtener(int CodEmpresa)
        {
            return FrmCCPlanillaReportesBL.CC_PlanillaReportes_TiposCobro_Obtener(CodEmpresa);
        }
    }
}