using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrSeguimientoAutorizacionesController : ControllerBase
    {
        private readonly FrmCrSeguimientoAutorizacionesBl _bl;

        public FrmCrSeguimientoAutorizacionesController(IConfiguration config)
        {
            _bl = new FrmCrSeguimientoAutorizacionesBl(config);
        }

        [HttpGet("Cr_SeguimientoAutorizaciones_Detalle_Obtener")]
        public ErrorDto<CrSeguimientoAutorizacionesDetalleData?> Cr_SeguimientoAutorizaciones_Detalle_Obtener(
            int codEmpresa,
            int operacion)
            => _bl.Cr_SeguimientoAutorizaciones_Detalle_Obtener(codEmpresa, operacion);

        [HttpPost("Cr_SeguimientoAutorizaciones_Autorizar")]
        public ErrorDto Cr_SeguimientoAutorizaciones_Autorizar(
            int codEmpresa,
            [FromBody] CrSeguimientoAutorizacionesAutorizarRequest request)
            => _bl.Cr_SeguimientoAutorizaciones_Autorizar(codEmpresa, request);
    }
}