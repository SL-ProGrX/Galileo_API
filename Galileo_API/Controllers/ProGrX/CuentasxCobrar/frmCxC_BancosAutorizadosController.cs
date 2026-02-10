using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCBancosAutorizadosController : ControllerBase
    {
        private readonly FrmCxCBancosAutorizadosBL _bl;

        public FrmCxCBancosAutorizadosController(IConfiguration config)
        {
            _bl = new FrmCxCBancosAutorizadosBL(config);
        }

        [Authorize]
        [HttpPost("CxcBancosAutorizados_InsertarFaltantes")]
        public ErrorDto<bool> CxcBancosAutorizados_InsertarFaltantes(int codEmpresa, [FromBody] CxcBancoAutorizadoInsertParams param)
        {
            return _bl.CxcBancosAutorizados_InsertarFaltantes(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CxcBancosAutorizados_Lista")]
        public ErrorDto<List<CxcBancoAutorizadoResult>> CxcBancosAutorizados_Lista(int codEmpresa)
        {
            return _bl.CxcBancosAutorizados_Lista(codEmpresa);
        }

        [Authorize]
        [HttpPost("CxcBancosAutorizados_UpdateCheques")]
        public ErrorDto<bool> CxcBancosAutorizados_UpdateCheques(int codEmpresa, [FromBody] CxcBancoAutorizadoUpdateChequesParams param)
        {
            return _bl.CxcBancosAutorizados_UpdateCheques(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CxcBancosAutorizados_UpdateTransferencias")]
        public ErrorDto<bool> CxcBancosAutorizados_UpdateTransferencias(int codEmpresa, [FromBody] CxcBancoAutorizadoUpdateTransferenciasParams param)
        {
            return _bl.CxcBancosAutorizados_UpdateTransferencias(codEmpresa, param);
        }
    }
}
