using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrSeguimientoRetencionesController : ControllerBase
    {
        private readonly FrmCrSeguimientoRetencionesBl _bl;

        public FrmCrSeguimientoRetencionesController(IConfiguration config)
        {
            _bl = new FrmCrSeguimientoRetencionesBl(config);
        }

        [HttpPost("CR_SeguimientoRetenciones_Inicializar")]
        public ErrorDto<CrSeguimientoRetencionesPantallaData> CR_SeguimientoRetenciones_Inicializar(
            int codEmpresa,
            [FromBody] CrSeguimientoRetencionesInicializarRequest request)
            => _bl.CR_SeguimientoRetenciones_Inicializar(codEmpresa, request);

        [HttpPost("CR_SeguimientoRetenciones_Guardar")]
        public ErrorDto CR_SeguimientoRetenciones_Guardar(
            int codEmpresa,
            [FromBody] CrSeguimientoRetencionesGuardarRequest request)
            => _bl.CR_SeguimientoRetenciones_Guardar(codEmpresa, request);

        [HttpPost("CR_SeguimientoRetenciones_Eliminar")]
        public ErrorDto CR_SeguimientoRetenciones_Eliminar(
            int codEmpresa,
            [FromBody] CrSeguimientoRetencionesEliminarRequest request)
            => _bl.CR_SeguimientoRetenciones_Eliminar(codEmpresa, request);
    }
}