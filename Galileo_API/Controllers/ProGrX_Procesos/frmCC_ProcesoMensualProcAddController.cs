using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Procesos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;

namespace Galileo.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCCProcesoMensualProcAddController : ControllerBase
    {
        private readonly FrmCCProcesoMensualProcAddBL _bl;

        public FrmCCProcesoMensualProcAddController(IConfiguration config)
        {
            _bl = new FrmCCProcesoMensualProcAddBL(config);
        }

        [Authorize]
        [HttpGet("CC_PlanillaProcesosComplementarios_Lista_Obtener")]
        public ErrorDto<CcPlanillaProcesosComplementariosLista> CC_PlanillaProcesosComplementarios_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CC_PlanillaProcesosComplementarios_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("CC_PlanillaProcesosComplementarios_Obtener")]
        public ErrorDto<List<CcPlanillaProcesosComplementariosData>> CC_PlanillaProcesosComplementarios_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CC_PlanillaProcesosComplementarios_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("CC_PlanillaProcesosComplementarios_Guardar")]
        public ErrorDto CC_PlanillaProcesosComplementarios_Guardar(int CodEmpresa, string usuario, CcPlanillaProcesosComplementariosData data)
        {
            return _bl.CC_PlanillaProcesosComplementarios_Guardar(CodEmpresa, usuario, data);
        }

        [Authorize]
        [HttpDelete("CC_PlanillaProcesosComplementarios_Eliminar")]
        public ErrorDto CC_PlanillaProcesosComplementarios_Eliminar(int CodEmpresa, string transaccion, int proc_num, string ejecucion_tipo, string usuario)
        {
            return _bl.CC_PlanillaProcesosComplementarios_Eliminar(CodEmpresa, transaccion, proc_num, ejecucion_tipo, usuario);
        }

        [Authorize]
        [HttpGet("CC_PlanillaProcesosComplementarios_Transacciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaProcesosComplementarios_Transacciones_Obtener(int CodEmpresa)
        {
            return _bl.CC_PlanillaProcesosComplementarios_Transacciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CC_PlanillaProcesosComplementarios_TiposEjecucion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaProcesosComplementarios_TiposEjecucion_Obtener(int CodEmpresa)
        {
            return _bl.CC_PlanillaProcesosComplementarios_TiposEjecucion_Obtener(CodEmpresa);
        }
    }
}