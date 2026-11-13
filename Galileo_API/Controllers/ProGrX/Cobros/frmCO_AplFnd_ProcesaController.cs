using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Services.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCoAplFndProcesaController : ControllerBase
    {
        private readonly FrmCoAplFndProcesaBl _bl;

        public FrmCoAplFndProcesaController(IConfiguration config, CoAplFndProcesoQueue queue) =>
            _bl = new FrmCoAplFndProcesaBl(config, queue);

        [Authorize]
        [HttpGet("CO_AplFndProc_Informacion_Obtener")]
        public ErrorDto<List<CoAplFndProcInformacionData>> CO_AplFndProc_Informacion_Obtener(int codEmpresa)
        {
            return _bl.CO_AplFndProc_Informacion_Obtener(codEmpresa);
        }

        /// <summary>
        /// Encola la aplicación de fondos y devuelve el ProcesoId para consultar el avance.
        /// </summary>
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("CO_AplFnd_Procesa_Aplicar")]
        public ErrorDto<GenProcesoIniciadoResult> CO_AplFnd_Procesa_Aplicar(int codEmpresa, FondosAplicarRequest request)
        {
            return _bl.CO_AplFnd_Procesa_Iniciar(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("CO_AplFnd_Procesa_Estado")]
        public ErrorDto<CoAplFndProcesoEstadoResult>
            CO_AplFnd_Procesa_Estado(
                int codEmpresa,
                Guid procesoId)
        {
            return _bl.CO_AplFnd_Procesa_Estado(
                codEmpresa,
                procesoId);
        }
    }
}
