using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCrRemesasRetencionesController : ControllerBase
    {
        private readonly FrmCrRemesasRetencionesBl _bl;

        public FrmCrRemesasRetencionesController(IConfiguration config)
        {
            _bl = new FrmCrRemesasRetencionesBl(config);
        }

        [HttpGet("Cr_RemesasRetenciones_Pantalla_Obtener")]
        public ErrorDto<CrRemesasRetencionesPantallaData> Cr_RemesasRetenciones_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
            => _bl.Cr_RemesasRetenciones_Pantalla_Obtener(codEmpresa, usuario);

        [HttpPost("Cr_RemesasRetenciones_Validar")]
        public ErrorDto<CrRemesasRetencionesValidarData> Cr_RemesasRetenciones_Validar(
            int codEmpresa,
            [FromBody] CrRemesasRetencionesValidarRequest request)
            => _bl.Cr_RemesasRetenciones_Validar(codEmpresa, request);

        [HttpPost("Cr_RemesasRetenciones_Aplicar")]
        public ErrorDto Cr_RemesasRetenciones_Aplicar(
            int codEmpresa,
            string usuario,
            [FromBody] CrRemesasRetencionesAplicarRequest request)
            => _bl.Cr_RemesasRetenciones_Aplicar(codEmpresa, usuario, request);
    }
}
