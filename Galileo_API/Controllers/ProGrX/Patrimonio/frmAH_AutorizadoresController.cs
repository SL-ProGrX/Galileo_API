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
    public class FrmAhAutorizadoresController : ControllerBase
    {
        private readonly FrmAhAutorizadoresBL _bl;

        public FrmAhAutorizadoresController(IConfiguration config)
        {
            _bl = new FrmAhAutorizadoresBL(config);
        }

        [HttpGet("Patrimonio_frmAH_Autorizadores_Obtener")]
        public ActionResult<ErrorDto<AutorizadorePatrimonioDto>> Patrimonio_frmAH_Autorizadores_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string usuario)
        {
            return _bl.Patrimonio_frmAH_Autorizadores_Obtener(codEmpresa, usuario);
        }

        [HttpGet("Patrimonio_frmAH_Autorizadores_ConsultaAscDesc")]
        public ActionResult<ErrorDto<string>> Patrimonio_frmAH_Autorizadores_ConsultaAscDesc(
            int codEmpresa,
            string? usuario = "A",
            string? tipo = "ASC")
        {
            return _bl.Patrimonio_frmAH_Autorizadores_ConsultaAscDesc(codEmpresa, usuario ?? "A", tipo ?? "ASC");
        }

        [HttpGet("Patrimonio_frmAH_Autorizadores_Lista")]
        public ActionResult<ErrorDto<List<AutorizadorePatrimonioDto>>> Patrimonio_frmAH_Autorizadores_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] string? filtro = "")
        {
            return _bl.Patrimonio_frmAH_Autorizadores_Lista(codEmpresa, filtro);
        }

        [HttpPost("Patrimonio_frmAH_Autorizadores_Insertar")]
        public ActionResult<ErrorDto<FrmAhAutorizadoresGuardarResponse>> Patrimonio_frmAH_Autorizadores_Insertar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhAutorizadoresGuardarRequest request)
        {
            return _bl.Patrimonio_frmAH_Autorizadores_Insertar(codEmpresa, request);
        }

        [HttpPut("Patrimonio_frmAH_Autorizadores_Actualizar")]
        public ActionResult<ErrorDto<FrmAhAutorizadoresGuardarResponse>> Patrimonio_frmAH_Autorizadores_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] FrmAhAutorizadoresGuardarRequest request)
        {
            return _bl.Patrimonio_frmAH_Autorizadores_Actualizar(codEmpresa, request);
        }

        [HttpDelete("Patrimonio_frmAH_Autorizadores_Eliminar")]
        public ActionResult<ErrorDto<bool>> Patrimonio_frmAH_Autorizadores_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] string usuario,
            [FromQuery] string registroUsuario)
        {
            return _bl.Patrimonio_frmAH_Autorizadores_Eliminar(codEmpresa, usuario, registroUsuario);
        }
    }
}
