using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndFondosAplCreditosController : ControllerBase
    {
        private readonly FrmFndFondosAplCreditosBl _bl;

        public FrmFndFondosAplCreditosController(IConfiguration config)
        {
            _bl = new FrmFndFondosAplCreditosBl(config);
        }

        [Authorize]
        [HttpGet("FondosAplCreditos_Planes_Obtener")]
        public ErrorDto<List<FndFondosAplCreditosPlanModel>> FondosAplCreditos_Planes_Obtener(int codOperadora, int codEmpresa, string orderBy = "CodPlan")
        {
            return _bl.FondosAplCreditos_Planes_Obtener(codOperadora, codEmpresa, orderBy);
        }

        [Authorize]
        [HttpPost("FondosAplCreditos_Lista")]
        public ErrorDto<List<FndFondosAplCreditosListaResult>> FondosAplCreditos_Lista([FromBody] FndFondosAplCreditosListaParams param)
        {
            return _bl.FondosAplCreditos_Lista(param);
        }

        [Authorize]
        [HttpPost("FondosAplCreditos_AplicacionGeneral")]
        public ErrorDto<FndFondosAplCreditosAplicacionGeneralResult> FondosAplCreditos_AplicacionGeneral([FromBody] FndFondosAplCreditosAplicacionGeneralParams param)
        {
            return _bl.FondosAplCreditos_AplicacionGeneral(param);
        }

        [Authorize]
        [HttpPost("FondosAplCreditos_Aplicacion")]
        public ErrorDto<FndFondosAplCreditosAplicacionResult> FondosAplCreditos_Aplicacion([FromBody] FndFondosAplCreditosAplicacionParams param, [FromQuery] int codEmpresa)
        {
            return _bl.FondosAplCreditos_Aplicacion(param, codEmpresa);
        }

        [Authorize]
        [HttpGet("FondosAplCreditos_Resumen_Obtener")]
        public ErrorDto<List<FndFondosAplCreditosResumenResult>> FondosAplCreditos_Resumen_Obtener(int codEmpresa)
        {
            return _bl.FondosAplCreditos_Resumen_Obtener(codEmpresa);
        }
    }
}