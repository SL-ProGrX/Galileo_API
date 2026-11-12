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
    public class FrmCoAplFndContratosAplicacionController : ControllerBase
    {
        private readonly FrmCoAplFndContratosAplicacionBl _bl;

        public FrmCoAplFndContratosAplicacionController(IConfiguration config, CoAplFndContrProcesoQueue queue) =>
            _bl = new FrmCoAplFndContratosAplicacionBl(config, queue);

        [HttpGet("CO_AplFndContrApl_Informacion_Obtener")]
        public ErrorDto<List<CoAplFndContrAplInformacionData>> CO_AplFndContrApl_Informacion_Obtener(int codEmpresa)
        {
            return _bl.CO_AplFndContrApl_Informacion_Obtener(codEmpresa);
        }

        /// <summary>
        /// Encola la aplicación de fondos y devuelve el ProcesoId para consultar el avance.
        /// </summary>
        [ValidateAntiForgeryToken]
        [HttpPost("CO_AplFndContrApl_Procesa_Aplicar")]
        public ErrorDto<GenProcesoIniciadoResult> CO_AplFndContrApl_Procesa_Aplicar(int codEmpresa, ContratosAplicarRequest request)
        {
            return _bl.CO_AplFndContrApl_Procesa_Iniciar(codEmpresa, request);
        }

        [HttpGet("CO_AplFndContrApl_Procesa_Estado")]
        public ErrorDto<CoAplFndContrProcesoEstadoResult>
            CO_AplFndContrApl_Procesa_Estado(
                int codEmpresa,
                Guid procesoId)
        {
            return _bl.CO_AplFndContrApl_Procesa_Estado(
                codEmpresa,
                procesoId);
        }
    }
}
