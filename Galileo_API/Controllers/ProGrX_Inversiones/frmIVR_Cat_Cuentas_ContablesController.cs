using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Cat_Cuentas_Contables")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrCatCuentasContablesController
        : ControllerBase
    {
        private readonly FrmIvrCatCuentasContablesBl _bl;

        public FrmIvrCatCuentasContablesController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmIvrCatCuentasContablesBl(config);
        }

        [HttpGet(
            "IVR_Cat_Cuentas_Contables_Inicializar")]
        public ErrorDto<IvrCatCuentasContablesInicialData?>
            IVR_Cat_Cuentas_Contables_Inicializar(
                [FromQuery] int codEmpresa,
                [FromQuery]
                IvrCatCuentasContablesInicializarRequest? request)
        {
            return _bl.IVR_Cat_Cuentas_Contables_Inicializar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut(
            "IVR_Cat_Cuentas_Contables_Guardar")]
        public ErrorDto<IvrCatCuentasContablesData?>
            IVR_Cat_Cuentas_Contables_Guardar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrCatCuentasContablesGuardarRequest? request)
        {
            return _bl.IVR_Cat_Cuentas_Contables_Guardar(
                codEmpresa,
                request);
        }
    }
}