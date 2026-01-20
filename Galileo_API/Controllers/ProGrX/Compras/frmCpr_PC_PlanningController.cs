using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprPCPlanningController : ControllerBase
    {
        private readonly FrmCprPCPlanningBL PC_PlanningBL;
        public FrmCprPCPlanningController(IConfiguration config)
        {
            PC_PlanningBL = new FrmCprPCPlanningBL(config);
        }

        [HttpGet("CprPlanCompras_Obtener")]
        public ErrorDto<List<CprPlanComprasDto>> CprPlanCompras_Obtener(int CodEmpresa)
        {
            return PC_PlanningBL.CprPlanCompras_Obtener(CodEmpresa);
        }

        [HttpGet("CprPlanDT_Obtener")]
        public ErrorDto<CprPlanDTDto> CprPlanDT_Obtener(int CodEmpresa, int PlanCompras, string CodProducto)
        {
            return PC_PlanningBL.CprPlanDT_Obtener(CodEmpresa, PlanCompras, CodProducto);
        }

        [HttpGet("CprPlanDTCortes_Obtener")]
        public ErrorDto<List<CprPlanDTCortesDto>> CprPlanDTCortes_Obtener(int CodEmpresa, int PlanCompras, string CodProducto)
        {
            return PC_PlanningBL.CprPlanDTCortes_Obtener(CodEmpresa, PlanCompras, CodProducto);
        }

        [HttpGet("CprResumenPlan_Obtener")]
        public ErrorDto<CprResumenPlanLista> CprResumenPlan_Obtener(int CodEmpresa, string parametros)
        {
            return PC_PlanningBL.CprResumenPlan_Obtener(CodEmpresa, parametros);
        }

        [HttpGet("CprResumenPlan_ObtenerxCuenta")]
        public ErrorDto<CprResumenPlanLista> CprResumenPlan_ObtenerxCuenta(int CodEmpresa, string parametros)
        {
            return PC_PlanningBL.CprResumenPlan_ObtenerxCuenta(CodEmpresa, parametros);
        }

        [HttpGet("CprPlanContable_Obtener")]
        public ErrorDto<CprPlanContableLista> CprPlanContable_Obtener(int CodEmpresa, string parametros)
        {
            return PC_PlanningBL.CprPlanContable_Obtener(CodEmpresa, parametros);
        }

        [HttpGet("CprBitacora_Obtener")]
        public ErrorDto<CprBitacoraLista> CprBitacora_Obtener(int CodEmpresa, string parametros)
        {
            return PC_PlanningBL.CprBitacora_Obtener(CodEmpresa, parametros);
        }

        [HttpPost("CprPlanCompras_Insert")]
        public ErrorDto CprPlanCompras_Insert(int CodEmpresa, CprPlanComprasDto request)
        {
            return PC_PlanningBL.CprPlanCompras_Insert(CodEmpresa, request);
        }

        [HttpPost("CprPlanCompras_Update")]
        public ErrorDto CprPlanCompras_Update(int CodEmpresa, CprPlanComprasDto request)
        {
            return PC_PlanningBL.CprPlanCompras_Update(CodEmpresa, request);
        }

        [HttpPost("CprPlanDT_Upsert")]
        public ErrorDto CprPlanDT_Upsert(int CodEmpresa, string parametros, List<CprPlanDTCortesDto> cortes)
        {
            return PC_PlanningBL.CprPlanDT_Upsert(CodEmpresa, parametros, cortes);
        }
    }
}