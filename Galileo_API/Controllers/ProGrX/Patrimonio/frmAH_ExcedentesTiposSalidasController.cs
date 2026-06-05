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
    public class FrmAhExcedentesTiposSalidasController : ControllerBase
    {
        private readonly FrmAhExcedentesTiposSalidasBL _bl;

        public FrmAhExcedentesTiposSalidasController(IConfiguration config)
        {
            _bl = new FrmAhExcedentesTiposSalidasBL(config);
        }

        [HttpGet("Ah_ExcedentesTiposSalidas_Lista")]
        public ActionResult<ErrorDto<List<FrmAhExcedentesTiposSalidasDto>>> Ah_ExcedentesTiposSalidas_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Ah_ExcedentesTiposSalidas_Lista(codEmpresa);
        }

        [HttpGet("Ah_ExcedentesTiposSalidas_Planes_Lista")]
        public ActionResult<ErrorDto<List<FrmAhExcedentesTiposSalidasPlanDto>>> Ah_ExcedentesTiposSalidas_Planes_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Ah_ExcedentesTiposSalidas_Planes_Lista(codEmpresa);
        }

        [HttpGet("Ah_ExcedentesTiposSalidas_Bancos_Lista")]
        public ActionResult<ErrorDto<List<FrmAhExcedentesTiposSalidasBancoDto>>> Ah_ExcedentesTiposSalidas_Bancos_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Ah_ExcedentesTiposSalidas_Bancos_Lista(codEmpresa);
        }

        [HttpPost("Ah_ExcedentesTiposSalidas_Insertar")]
        public ActionResult<ErrorDto<FrmAhExcedentesTiposSalidasGuardarResponse>> Ah_ExcedentesTiposSalidas_Insertar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesTiposSalidasGuardarRequest request)
        {
            return _bl.Ah_ExcedentesTiposSalidas_Insertar(codEmpresa, request);
        }

        [HttpPut("Ah_ExcedentesTiposSalidas_Actualizar")]
        public ActionResult<ErrorDto<FrmAhExcedentesTiposSalidasGuardarResponse>> Ah_ExcedentesTiposSalidas_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesTiposSalidasGuardarRequest request)
        {
            return _bl.Ah_ExcedentesTiposSalidas_Actualizar(codEmpresa, request);
        }

        [HttpDelete("Ah_ExcedentesTiposSalidas_Eliminar")]
        public ActionResult<ErrorDto<bool>> Ah_ExcedentesTiposSalidas_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] string codSalida,
            [FromQuery] string usuario)
        {
            return _bl.Ah_ExcedentesTiposSalidas_Eliminar(codEmpresa, codSalida, usuario);
        }
    }
}
