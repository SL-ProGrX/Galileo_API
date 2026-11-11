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
    [Authorize]
    public class FrmCoAplExcContratosAplicacionController : ControllerBase
    {
        private readonly FrmCoAplExcContratosAplicacionBl _bl;

        public FrmCoAplExcContratosAplicacionController(IConfiguration config, CoAplExcContrProcesoQueue queue) =>
            _bl = new FrmCoAplExcContratosAplicacionBl(config, queue);

        [HttpGet("CO_AplExcContrApl_Informacion_Obtener")]
        public ErrorDto<List<CoAplExcContrAplInformacionData>> CO_AplExcContrApl_Informacion_Obtener(int codEmpresa)
        {
            return _bl.CO_AplExcContrApl_Informacion_Obtener(codEmpresa);
        }

        /// <summary>
        /// Encola la aplicación de excedentes y devuelve el ProcesoId para consultar el avance.
        /// </summary>
        [ValidateAntiForgeryToken]
        [HttpPost("CO_AplExcContrApl_Procesa_Aplicar")]
        public ErrorDto<GenProcesoIniciadoResult> CO_AplExcContrApl_Procesa_Aplicar(int CodEmpresa, ExcContratosAplicarRequest request)
        {
            return _bl.CO_AplExcContrApl_Procesa_Iniciar(CodEmpresa, request);
        }

        [HttpGet("CO_AplExcContrApl_Procesa_Estado")]
        public ErrorDto<GenProcesoEstadoResult> CO_AplExcContrApl_Procesa_Estado(int CodEmpresa, Guid ProcesoId)
        {
            return _bl.CO_AplExcContrApl_Procesa_Estado(CodEmpresa, ProcesoId);
        }
    }
}
