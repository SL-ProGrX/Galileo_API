using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.BusinessLogic.ProGrX.Fondos;
using PgxAPI.Models.ProGrX.Fondos;


namespace Galileo_API.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndLiquidacionPlanController : ControllerBase
    {
        private readonly FrmFndLiquidacionPlanBL _bl;
        public FrmFndLiquidacionPlanController(IConfiguration config)
        {
            _bl = new FrmFndLiquidacionPlanBL(config);
        }

        [Authorize]
        [HttpGet("FND_LiquidacionPlan_Listas")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Listas(int CodEmpresa, string usuario, string catalogo)
        {
            return _bl.FND_LiquidacionPlan_Listas(CodEmpresa, usuario, catalogo);
        }

        [Authorize]
        [HttpGet("FND_LiquidacionPlan_buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_buscar(int CodEmpresa, int codOperadora)
        {
            return _bl.FND_LiquidacionPlan_buscar(CodEmpresa, codOperadora);
        }

        [Authorize]
        [HttpGet("FND_LiquidacionPlan_Operadora_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Operadora_Obtener(int CodEmpresa)
        {
            return _bl.FND_LiquidacionPlan_Operadora_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("FND_LiquidacionPlanContartos_Buscar")]
        public ErrorDto<List<FndConsultaPlanRowDto>> FND_LiquidacionPlanContartos_Buscar(
          int CodEmpresa,
          FndLiquidacionPlanFiltrosData filtro)
        {
            return _bl.FND_LiquidacionPlanContartos_Buscar(CodEmpresa, filtro);
        }


       

    }
}
