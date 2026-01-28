using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasRoeReimpresionController : ControllerBase
    {
        private readonly FrmCajasRoeReimpresionBL _bl;

        public FrmCajasRoeReimpresionController(IConfiguration config)
        {
            _bl = new FrmCajasRoeReimpresionBL(config);
        }

        [Authorize]
        [HttpPost("CajasRoe_Consulta")]
        public ErrorDto<List<CajasRoeConsultaResult>> CajasRoe_Consulta(int codEmpresa, [FromBody] CajasRoeConsultaParams param)
        {
            return _bl.CajasRoe_Consulta(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CajasRoe_Imprime_Valida")]
        public ErrorDto<CajasRoeImprimeValidaResult?> CajasRoe_Imprime_Valida(int codEmpresa, int idRoe)
        {
            return _bl.CajasRoe_Imprime_Valida(codEmpresa, idRoe);
        }

        [Authorize]
        [HttpPost("CajasRoe_Imprime")]
        public ErrorDto<CajasRoeImprimeResult?> CajasRoe_Imprime(int codEmpresa, [FromBody] CajasRoeImprimeParams param)
        {
            return _bl.CajasRoe_Imprime(codEmpresa, param);
        }
    }
}
