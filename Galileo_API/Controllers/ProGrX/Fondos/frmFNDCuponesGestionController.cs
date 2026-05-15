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
    public class FrmFndCuponesGestionController : ControllerBase
    {
        private readonly FrmFndCuponesGestionBl _bl;

        public FrmFndCuponesGestionController(IConfiguration config)
        {
            _bl = new FrmFndCuponesGestionBl(config);
        }

        [Authorize]
        [HttpGet("FndCuponesGestion_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FndCuponesGestion_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.FndCuponesGestion_Bancos_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("FndCuponesGestion_Conceptos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FndCuponesGestion_Conceptos_Obtener(int CodEmpresa)
        {
            return _bl.FndCuponesGestion_Conceptos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FndCuponesGestion_PlanExiste")]
        public ErrorDto<FndCuponesGestionPlanExisteResult> FndCuponesGestion_PlanExiste(int CodEmpresa)
        {
            return _bl.FndCuponesGestion_PlanExiste(CodEmpresa);
        }

        [Authorize]
        [HttpPost("FndCuponesGestion_ConsultaVencimiento")]
        public ErrorDto<List<FndCuponesGestionVencimientoResult>> FndCuponesGestion_ConsultaVencimiento([FromBody] FndCuponesGestionVencimientoParams param)
        {
            return _bl.FndCuponesGestion_ConsultaVencimiento(param);
        }

        [Authorize]
        [HttpPost("FndCuponesGestion_Liquida")]
        public ErrorDto<FndCuponesGestionLiquidaResult> FndCuponesGestion_Liquida([FromBody] FndCuponesGestionLiquidaParams param)
        {
            return _bl.FndCuponesGestion_Liquida(param);
        }
    }
}