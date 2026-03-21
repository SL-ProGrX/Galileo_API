using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXPeriodosReversaController : ControllerBase
    {
        private readonly FrmCntXPeriodosReversaBl _bl;

        public FrmCntXPeriodosReversaController(IConfiguration config) 
            => _bl = new FrmCntXPeriodosReversaBl(config);

        [HttpGet("CntXPeriodos_Cierres_Obtener")]
        public ErrorDto<List<CntXPeriodosData>> CntXPeriodos_Cierres_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXPeriodos_Cierres_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXPeriodos_Bitacora_Obtener")]
        public ErrorDto<List<CntXPeriodosLogData>> CntXPeriodos_Bitacora_Obtener(int codEmpresa, string request)
        {
            return _bl.CntXPeriodos_Bitacora_Obtener(codEmpresa, request);
        }

        [HttpPost("CntXPeriodos_Reversar")]
        public ErrorDto CntXPeriodos_Reversar(int codEmpresa, ReversaPeriodoRequest request)
        {
            return _bl.CntXPeriodos_Reversar(codEmpresa, request);
        }
    }
}