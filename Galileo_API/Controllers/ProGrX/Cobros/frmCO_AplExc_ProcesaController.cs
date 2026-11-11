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
    public class FrmCoAplExcProcesaController : ControllerBase
    {
        private readonly FrmCoAplExcProcesaBl _bl;

        public FrmCoAplExcProcesaController(IConfiguration config, CoAplExcProcesoQueue queue) =>
            _bl = new FrmCoAplExcProcesaBl(config, queue);

        [Authorize]
        [HttpGet("CO_AplExcProc_Informacion_Obtener")]
        public ErrorDto<List<CoAplExcProcInformacionData>> CO_AplExcProc_Informacion_Obtener(int codEmpresa)
        {
            return _bl.CO_AplExcProc_Informacion_Obtener(codEmpresa);
        }

        /// <summary>
        /// Encola la aplicación de excedentes y devuelve el ProcesoId para consultar el avance.
        /// </summary>
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("CO_AplExc_Procesa_Aplicar")]
        public ErrorDto<GenProcesoIniciadoResult> CO_AplExc_Procesa_Aplicar(int CodEmpresa, ExcedenteAplicarRequest request)
        {
            return _bl.CO_AplExc_Procesa_Iniciar(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("CO_AplExc_Procesa_Estado")]
        public ErrorDto<GenProcesoEstadoResult> CO_AplExc_Procesa_Estado(int CodEmpresa, Guid ProcesoId)
        {
            return _bl.CO_AplExc_Procesa_Estado(CodEmpresa, ProcesoId);
        }
    }
}
