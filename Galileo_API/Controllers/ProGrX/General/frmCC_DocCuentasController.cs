using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/frmCC_DocCuentas")]
    [ApiController]
    [Authorize]
    public class FrmCcDocCuentasController : ControllerBase
    {
        private readonly FrmCcDocCuentasBl _bl;

        public FrmCcDocCuentasController(IConfiguration config)
        {
            _bl = new FrmCcDocCuentasBl(config);
        }

        [HttpGet("CC_DocCuentas_Cuenta_Obtener")]
        public ErrorDto<CcDocCuentasCuentaData> CC_DocCuentas_Cuenta_Obtener(int CodEmpresa, string? cuenta)
        {
            return _bl.CC_DocCuentas_Cuenta_Obtener(CodEmpresa, cuenta);
        }

        [HttpGet("CC_DocCuentas_Documento_Verificar")]
        public ErrorDto<CcDocCuentasVerificarData> CC_DocCuentas_Documento_Verificar(int CodEmpresa, string? tipo)
        {
            return _bl.CC_DocCuentas_Documento_Verificar(CodEmpresa, tipo);
        }

        [HttpGet("CC_DocCuentas_Documento_Validar")]
        public ErrorDto<CcDocCuentasResultadoData> CC_DocCuentas_Documento_Validar(
            int CodEmpresa,
            [FromQuery] CcDocCuentasValidarRequest request)
        {
            return _bl.CC_DocCuentas_Documento_Validar(CodEmpresa, request);
        }
    }
}
