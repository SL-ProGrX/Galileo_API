using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndPlanillaBitacoraController : ControllerBase
    {
        private readonly FrmFndPlanillaBitacoraBl BlFndPlanillaBitacora;

        public FrmFndPlanillaBitacoraController(IConfiguration config)
        {
            BlFndPlanillaBitacora = new FrmFndPlanillaBitacoraBl(config);
        }

        [Authorize]
        [HttpGet("FND_PlanillaBitacora_Obtener")]
        public ErrorDto<List<FndPrmBitacoraDto>> FND_PlanillaBitacora_Obtener(int CodEmpresa, int CodInstitucion, int Proceso)
        {
            return BlFndPlanillaBitacora.FND_PlanillaBitacora_Obtener(CodEmpresa, CodInstitucion, Proceso);
        }

        [Authorize]
        [HttpGet("FND_PlanillaBitacora_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaBitacora_Instituciones_Obtener(int CodEmpresa)
        {
            return BlFndPlanillaBitacora.FND_PlanillaBitacora_Instituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FND_PlanillaBitacora_Operadoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaBitacora_Operadoras_Obtener(int CodEmpresa)
        {
            return BlFndPlanillaBitacora.FND_PlanillaBitacora_Operadoras_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FND_PlanillaBitacora_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaBitacora_Planes_Obtener(int CodEmpresa)
        {
            return BlFndPlanillaBitacora.FND_PlanillaBitacora_Planes_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FND_PlanillaBitacora_Proceso_Obtener")]
        public ErrorDto<int> FND_PlanillaBitacora_Proceso_Obtener(int CodEmpresa, int Proceso, int Direccion)
        {
            return BlFndPlanillaBitacora.FND_PlanillaBitacora_Proceso_Obtener(CodEmpresa, Proceso, Direccion);
        }
    }
}