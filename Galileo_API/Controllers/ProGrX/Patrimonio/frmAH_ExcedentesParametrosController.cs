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
    public class FrmAhExcedentesParametrosController : ControllerBase
    {
        private readonly FrmAhExcedentesParametrosBL _bl;

        public FrmAhExcedentesParametrosController(IConfiguration config)
        {
            _bl = new FrmAhExcedentesParametrosBL(config);
        }

        [HttpGet("Ah_ExcedentesParametros_Lista")]
        public ErrorDto<List<FrmAhExcedentesParametroDto>> Ah_ExcedentesParametros_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Ah_ExcedentesParametros_Lista(codEmpresa);
        }

        [HttpPut("Ah_ExcedentesParametros_Actualizar")]
        public ErrorDto<bool> Ah_ExcedentesParametros_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesParametroActualizarRequest request)
        {
            return _bl.Ah_ExcedentesParametros_Actualizar(codEmpresa, request);
        }
    }
}
