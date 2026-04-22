using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivTiemposSeguimientosController : ControllerBase
    {
        private readonly FrmVivTiemposSeguimientosBl _bl;

        public FrmVivTiemposSeguimientosController(IConfiguration config)
        {
            _bl = new FrmVivTiemposSeguimientosBl(config);
        }

        [HttpGet("VivTiemposSeguimiento_Obtener")]
        public ErrorDto<List<VivTiemposSeguimientoData>> VivTiemposSeguimiento_Obtener(int codEmpresa)
        {
            return _bl.VivTiemposSeguimiento_Obtener(codEmpresa);
        }

        [HttpPost("VivTiemposSeguimiento_Guardar")]
        public ErrorDto VivTiemposSeguimiento_Guardar(int codEmpresa, VivTiemposSeguimientoData request)
        {
            return _bl.VivTiemposSeguimiento_Guardar(codEmpresa, request);
        }
    }
}
