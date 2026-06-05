using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAhAutorizacionesController : ControllerBase
    {
        private readonly FrmAhAutorizacionesBL _bl;

        public FrmAhAutorizacionesController(IConfiguration config)
        {
            _bl = new FrmAhAutorizacionesBL(config);
        }

        [HttpGet("Ah_Autorizaciones_Obtener")]
        public ActionResult<ErrorDto<List<PatGestionesPatrimonio>>> Ah_Autorizaciones_Obtener(
            [FromQuery] int codEmpresa,
            string filtros)
        {
            return _bl.Ah_Autorizaciones_Obtener(codEmpresa, filtros);
        }

        [HttpPost("Ah_Autorizaciones_Procesar")]
        public ActionResult<ErrorDto<FrmAhAutorizacionesProcesarResponse>> Ah_Autorizaciones_Procesar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhAutorizacionesProcesarRequest request)
        {
            return _bl.Ah_Autorizaciones_Procesar(codEmpresa, request);
        }
    }
}
