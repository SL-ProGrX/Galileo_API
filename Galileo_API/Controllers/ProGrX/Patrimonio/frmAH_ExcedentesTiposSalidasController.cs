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

        [HttpGet("Patrimonio_frmAH_ExcedentesTiposSalidas_Lista")]
        public ActionResult<ErrorDto<List<FrmAhExcedentesTiposSalidasDto>>> Patrimonio_frmAH_ExcedentesTiposSalidas_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Patrimonio_frmAH_ExcedentesTiposSalidas_Lista(codEmpresa);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesTiposSalidas_Planes_Lista")]
        public ActionResult<ErrorDto<List<FrmAhExcedentesTiposSalidasPlanDto>>> Patrimonio_frmAH_ExcedentesTiposSalidas_Planes_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Patrimonio_frmAH_ExcedentesTiposSalidas_Planes_Lista(codEmpresa);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesTiposSalidas_Bancos_Lista")]
        public ActionResult<ErrorDto<List<FrmAhExcedentesTiposSalidasBancoDto>>> Patrimonio_frmAH_ExcedentesTiposSalidas_Bancos_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Patrimonio_frmAH_ExcedentesTiposSalidas_Bancos_Lista(codEmpresa);
        }

        [HttpPost("Patrimonio_frmAH_ExcedentesTiposSalidas_Insertar")]
        public ActionResult<ErrorDto<FrmAhExcedentesTiposSalidasGuardarResponse>> Patrimonio_frmAH_ExcedentesTiposSalidas_Insertar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesTiposSalidasGuardarRequest request)
        {
            return _bl.Patrimonio_frmAH_ExcedentesTiposSalidas_Insertar(codEmpresa, request);
        }

        [HttpPut("Patrimonio_frmAH_ExcedentesTiposSalidas_Actualizar")]
        public ActionResult<ErrorDto<FrmAhExcedentesTiposSalidasGuardarResponse>> Patrimonio_frmAH_ExcedentesTiposSalidas_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhExcedentesTiposSalidasGuardarRequest request)
        {
            return _bl.Patrimonio_frmAH_ExcedentesTiposSalidas_Actualizar(codEmpresa, request);
        }

        [HttpDelete("Patrimonio_frmAH_ExcedentesTiposSalidas_Eliminar")]
        public ActionResult<ErrorDto<bool>> Patrimonio_frmAH_ExcedentesTiposSalidas_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] string codSalida,
            [FromQuery] string usuario)
        {
            return _bl.Patrimonio_frmAH_ExcedentesTiposSalidas_Eliminar(codEmpresa, codSalida, usuario);
        }
    }
}
