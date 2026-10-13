using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Convenios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrConveniosParametrosController : ControllerBase
    {
        private readonly FrmCrConveniosParametrosBl _bl;

        public FrmCrConveniosParametrosController(IConfiguration config)
        {
            _bl = new FrmCrConveniosParametrosBl(config);
        }

        [HttpGet("CrConveniosParametros_Obtener")]
        public ErrorDto<List<CrConvenioParametroData>> CrConveniosParametros_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int codContabilidad)
            => _bl.CrConveniosParametros_Obtener(codEmpresa, codContabilidad);

        [ValidateAntiForgeryToken]
        [HttpPut("CrConveniosParametros_Actualizar")]
        public ErrorDto<CrConvenioParametroData> CrConveniosParametros_Actualizar(
            [FromQuery] int codEmpresa,
            [FromQuery] int codContabilidad,
            [FromBody] CrConvenioParametroActualizarRequest request)
            => _bl.CrConveniosParametros_Actualizar(codEmpresa, codContabilidad, request);
    }
}
