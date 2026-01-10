using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCoAplFndProcesaController : ControllerBase
    {
        private readonly FrmCoAplFndProcesaBl _bl;

        public FrmCoAplFndProcesaController(IConfiguration config) => _bl = new FrmCoAplFndProcesaBl(config);

        [Authorize]
        [HttpGet("CO_AplFndProc_Informacion_Obtener")]
        public ErrorDto<List<CoAplFndProcInformacionData>> CO_AplFndProc_Informacion_Obtener(int codEmpresa)
        {
            return _bl.CO_AplFndProc_Informacion_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpPost("CO_AplFnd_Procesa_Aplicar")]
        public ErrorDto<CoAplFndProcesadosResult> CO_AplFnd_Procesa_Aplicar(int CodEmpresa, FondosAplicarRequest request)
        {
            return _bl.CO_AplFnd_Procesa_Aplicar(CodEmpresa, request);
        }
    }
}


