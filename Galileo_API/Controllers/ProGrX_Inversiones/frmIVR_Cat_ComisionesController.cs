using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Cat_Comisiones")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrCatComisionesController : ControllerBase
    {
        private readonly FrmIvrCatComisionesBl _bl;

        public FrmIvrCatComisionesController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmIvrCatComisionesBl(config);
        }

        [HttpGet("IVR_Cat_Comisiones_Lista_Obtener")]
        public ErrorDto<List<IvrCatComisionDto>> IVR_Cat_Comisiones_Lista_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.IVR_Cat_Comisiones_Lista_Obtener(codEmpresa);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("IVR_Cat_Comisiones_Registrar")]
        public ErrorDto IVR_Cat_Comisiones_Registrar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatComisionGuardarRequest? request)
        {
            return _bl.IVR_Cat_Comisiones_Registrar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut("IVR_Cat_Comisiones_Actualizar")]
        public ErrorDto IVR_Cat_Comisiones_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatComisionGuardarRequest? request)
        {
            return _bl.IVR_Cat_Comisiones_Actualizar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete("IVR_Cat_Comisiones_Eliminar")]
        public ErrorDto IVR_Cat_Comisiones_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] string? codComision,
            [FromQuery] string? usuario)
        {
            return _bl.IVR_Cat_Comisiones_Eliminar(codEmpresa, codComision, usuario);
        }
    }
}
