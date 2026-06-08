using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.BusinessLogic.ProGrX_Personas;

namespace Galileo.Controllers.ProGrx_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFPlanMutualController : ControllerBase
    {
        private readonly FrmAFPlanMutualBL _bl;

        public FrmAFPlanMutualController(IConfiguration config)
        {
            _bl = new FrmAFPlanMutualBL(config);
        }

        [Authorize]
        [HttpGet("AF_PlanMutualLista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PlanMutualLista_Obtener(int CodEmpresa)
        {
            return _bl.AF_PlanMutualLista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_PlanMutualPersonas_Obtener")]
        public ErrorDto<AfPlanPersonaslLista> AF_PlanMutualPersonas_Obtener(int CodEmpresa, string filtro, string? plan, string? estado)
        {
            return _bl.AF_PlanMutualPersonas_Obtener(CodEmpresa, filtro ,plan, estado );
        }

        [Authorize]
        [HttpGet("AF_PlanMutual_Obtener")]
        public ErrorDto<AfPlanMutualLista> AF_PlanMutual_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_PlanMutual_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("AF_PlanMutualPersonas_Exportar")]
        public ErrorDto<List<AfPlanMutualPersonasData>> AF_PlanMutualPersonas_Exportar(int CodEmpresa, string plan, string estado, int total)
        {
            return _bl.AF_PlanMutualPersonas_Exportar(CodEmpresa, plan, estado, total);
        }

        [Authorize]
        [HttpPost("AF_PlanMutualPersona_Guardar")]
        public ErrorDto AF_PlanMutualPersona_Guardar(int CodEmpresa, string plan, string usuario, AfPlanMutualPersonasData persona)
        {
            return _bl.AF_PlanMutualPersona_Guardar(CodEmpresa, plan, usuario, persona);
        }

        [Authorize]
        [HttpPost("AF_PlanMutual_Guardar")]
        public ErrorDto AF_PlanMutual_Guardar(int CodEmpresa, string usuario, AfPlanMutualDto plan)
        {
            return _bl.AF_PlanMutual_Guardar(CodEmpresa, usuario, plan);
        }

        [Authorize]
        [HttpDelete("AF_PlanMutual_Eliminar")]
        public ErrorDto AF_PlanMutual_Eliminar(int CodEmpresa, string usuario, string plan)
        {
            return _bl.AF_PlanMutual_Eliminar(CodEmpresa, usuario, plan);
        }

        [Authorize]
        [HttpPut("AF_PlanMutual_Actualizar")]
        public ErrorDto AF_PlanMutual_Actualizar(int CodEmpresa, string usuario, string plan)
        {
            return _bl.AF_PlanMutual_Actualizar(CodEmpresa, usuario, plan);
        }

    }
}
