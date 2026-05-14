using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ProGrX.Fondos;

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
        [HttpGet("FND_LiquidacionPlan_Plan_Scroll_Obtener")]
        public ErrorDto<DropDownListaGenericaModel> FND_LiquidacionPlan_Plan_Scroll_Obtener(
         int CodEmpresa,
         int codOperadora,
         string? codPlanActual,
         int scrollCode)
        {
            return _bl.FND_LiquidacionPlan_Plan_Scroll_Obtener(CodEmpresa, codOperadora, codPlanActual, scrollCode);
        }

        [Authorize]
        [HttpGet("FND_LiquidacionPlan_Operadora_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Operadora_Obtener(int CodEmpresa)
        {
            return _bl.FND_LiquidacionPlan_Operadora_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("FND_LiquidacionPlanContratos_Buscar")]
        public ErrorDto<List<FndConsultaPlanRowDto>> FND_LiquidacionPlanContratos_Buscar(
          int CodEmpresa,
          FndLiquidacionPlanFiltrosData filtro)
        {
            return _bl.FND_LiquidacionPlanContratos_Buscar(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("FND_LiquidacionPlan_Catalogo_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Catalogo_Buscar(int CodEmpresa)
        {
            return _bl.FND_LiquidacionPlan_Catalogo_Buscar(CodEmpresa);
        }

        [Authorize]
        [HttpPost("FND_LiquidacionPlan_Liquidar")]
        public ErrorDto<FndLiquidacionPlanLiquidarResult> FND_LiquidacionPlan_Liquidar(
         int codEmpresa,
         FndLiquidacionPlanLiquidarRequest request)
        {
            return _bl.FND_LiquidacionPlan_Liquidar(codEmpresa, request);
        }

        [Authorize]
        [HttpPost("FND_LiquidacionPlan_ArchivoRef_Cargar")]
        public ErrorDto<int> FND_LiquidacionPlan_ArchivoRef_Cargar(
               int codEmpresa,
               FndArchivoRefCargaRequest request)
        {
            return _bl.FND_LiquidacionPlan_ArchivoRef_Cargar(codEmpresa, request);
        }
    }
}