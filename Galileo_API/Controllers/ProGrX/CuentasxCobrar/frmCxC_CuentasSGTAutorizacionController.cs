
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCCuentasSgtAutorizacionModels;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCCuentasSgtAutorizacionController : ControllerBase
    {
        private readonly FrmCxCCuentasSgtAutorizacionBL _bl;

        public FrmCxCCuentasSgtAutorizacionController(IConfiguration config)
            => _bl = new FrmCxCCuentasSgtAutorizacionBL(config);

        [Authorize]
        [HttpGet("CxCCuentasSGTAutorizacion_Consulta")]
        public ErrorDto<CuentasSgtAutorizacionDto?> CxCCuentasSGTAutorizacion_Consulta(int codEmpresa, int operacion)
        {

            return _bl.CxCCuentasSGTAutorizacion_Consulta(codEmpresa, operacion);
        }

        [Authorize]
        [HttpPost("CxCCuentasSGTAutorizacion_Actualizar")]
        public ErrorDto CxCCuentasSGTAutorizacion_Actualizar(int codEmpresa, string usuario, string estado, int operacion, string notas)
        {
            return _bl.CxCCuentasSGTAutorizacion_Actualizar(codEmpresa, usuario, estado, operacion, notas);
        }
    }

}