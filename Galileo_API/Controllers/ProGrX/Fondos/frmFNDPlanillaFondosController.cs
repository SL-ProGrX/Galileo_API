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
    public class FrmFndPlanillaFondosController : ControllerBase
    {
        private readonly FrmFndPlanillaFondosBl BlFndPlanillaFondos;

        public FrmFndPlanillaFondosController(IConfiguration config)
        {
            BlFndPlanillaFondos = new FrmFndPlanillaFondosBl(config);
        }

        [Authorize]
        [HttpGet("FND_PlanillaFondos_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaFondos_Instituciones_Obtener(int CodEmpresa)
        {
            return BlFndPlanillaFondos.FND_PlanillaFondos_Instituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FND_PlanillaFondos_Operadoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaFondos_Operadoras_Obtener(int CodEmpresa)
        {
            return BlFndPlanillaFondos.FND_PlanillaFondos_Operadoras_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FND_PlanillaFondos_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaFondos_Planes_Obtener(int CodEmpresa, int CodOperadora)
        {
            return BlFndPlanillaFondos.FND_PlanillaFondos_Planes_Obtener(CodEmpresa, CodOperadora);
        }

        [Authorize]
        [HttpGet("FND_PlanillaFondos_Comprobante_Obtener")]
        public ErrorDto<string> FND_PlanillaFondos_Comprobante_Obtener(int CodEmpresa, int CodInstitucion, int Proceso)
        {
            return BlFndPlanillaFondos.FND_PlanillaFondos_Comprobante_Obtener(CodEmpresa, CodInstitucion, Proceso);
        }

        [Authorize]
        [HttpGet("FND_PlanillaFondos_Cuenta_Obtener")]
        public ErrorDto<DropDownListaGenericaModel> FND_PlanillaFondos_Cuenta_Obtener(int CodEmpresa, string Tipo, int CodInstitucion, int CodOperadora, string CodPlan, int CodConta)
        {
            return BlFndPlanillaFondos.FND_PlanillaFondos_Cuenta_Obtener(CodEmpresa, Tipo, CodInstitucion, CodOperadora, CodPlan, CodConta);
        }

        [Authorize]
        [HttpGet("FND_PlanillaFondos_Procesos_ObtenerRango")]
        public ErrorDto<List<int>> FND_PlanillaFondos_Procesos_ObtenerRango(int CodEmpresa, int Proceso)
        {
            return BlFndPlanillaFondos.FND_PlanillaFondos_Procesos_ObtenerRango(CodEmpresa, Proceso);
        }

        [Authorize]
        [HttpGet("FND_PlanillaFondos_Deducciones_Cargar")]
        public ErrorDto<FndPlanillaFondosData> FND_PlanillaFondos_Deducciones_Cargar(int CodEmpresa, string Request)
        {
            return BlFndPlanillaFondos.FND_PlanillaFondos_Deducciones_Cargar(CodEmpresa, Request);
        }

        [Authorize]
        [HttpPost("FND_PlanillaFondos_Procesar")]
        public ErrorDto<object> FND_PlanillaFondos_Procesar(int CodEmpresa, string Request)
        {
            return BlFndPlanillaFondos.FND_PlanillaFondos_Procesar(CodEmpresa, Request);
        }
    }
}