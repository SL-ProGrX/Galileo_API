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
    public class FrmCrConveniosAhorrosController : ControllerBase
    {
        private readonly FrmCrConveniosAhorrosBl _bl;

        public FrmCrConveniosAhorrosController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmCrConveniosAhorrosBl(config);
        }

        [HttpGet("CR_Convenios_Ahorros_Obtener")]
        public ErrorDto<CrConveniosAhorrosData?>
            CR_Convenios_Ahorros_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codConvenio,
                [FromQuery] int codOrden)
        {
            return _bl.CR_Convenios_Ahorros_Obtener(
                codEmpresa,
                codConvenio,
                codOrden);
        }

        [ValidateAntiForgeryToken]
        [HttpPut("CR_Convenios_Ahorros_Monto_Actualizar")]
        public ErrorDto<CrConveniosAhorrosData?>
            CR_Convenios_Ahorros_Monto_Actualizar(
                [FromQuery] int codEmpresa,
                [FromBody] CrConveniosAhorrosActualizarRequest? request)
        {
            return _bl.CR_Convenios_Ahorros_Monto_Actualizar(
                codEmpresa,
                request);
        }
    }
}