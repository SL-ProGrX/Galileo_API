using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCoAplExcProcesaController : ControllerBase
    {
        private readonly FrmCoAplExcProcesaBl _bl;

        public FrmCoAplExcProcesaController(IConfiguration config) => _bl = new FrmCoAplExcProcesaBl(config);

        [Authorize]
        [HttpGet("CO_AplExcProc_Informacion_Obtener")]
        public ErrorDto<List<CoAplExcProcInformacionData>> CO_AplExcProc_Informacion_Obtener(int codEmpresa)
        {
            return _bl.CO_AplExcProc_Informacion_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpPost("CO_AplExc_Procesa_Aplicar")]
        public ErrorDto<CoAplExcProcesadosResult> CO_AplExc_Procesa_Aplicar(int CodEmpresa, ExcedenteAplicarRequest request)
        {
            return _bl.CO_AplExc_Procesa_Aplicar(CodEmpresa, request);
        }
    }
}


