using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Cat_Reservas")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrCatReservasController : ControllerBase
    {
        private readonly FrmIvrCatReservasBl _bl;

        public FrmIvrCatReservasController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmIvrCatReservasBl(config);
        }

        [HttpGet("IVR_Cat_Reservas_Lista_Obtener")]
        public ErrorDto<List<IvrCatReservaDto>> IVR_Cat_Reservas_Lista_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.IVR_Cat_Reservas_Lista_Obtener(codEmpresa);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("IVR_Cat_Reservas_Registrar")]
        public ErrorDto IVR_Cat_Reservas_Registrar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatReservaGuardarRequest? request)
        {
            return _bl.IVR_Cat_Reservas_Registrar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut("IVR_Cat_Reservas_Actualizar")]
        public ErrorDto IVR_Cat_Reservas_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatReservaGuardarRequest? request)
        {
            return _bl.IVR_Cat_Reservas_Actualizar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete("IVR_Cat_Reservas_Eliminar")]
        public ErrorDto IVR_Cat_Reservas_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] string? codReserva,
            [FromQuery] string? usuario)
        {
            return _bl.IVR_Cat_Reservas_Eliminar(codEmpresa, codReserva, usuario);
        }

        [HttpGet("IVR_Cat_Reservas_Cuentas_Obtener")]
        public ErrorDto<List<IvrCatReservaCuentaDto>> IVR_Cat_Reservas_Cuentas_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string? codReserva)
        {
            return _bl.IVR_Cat_Reservas_Cuentas_Obtener(codEmpresa, codReserva);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("IVR_Cat_Reservas_Cuenta_Registrar")]
        public ErrorDto IVR_Cat_Reservas_Cuenta_Registrar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatReservaCuentaRequest? request)
        {
            return _bl.IVR_Cat_Reservas_Cuenta_Registrar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut("IVR_Cat_Reservas_Cuenta_Actualizar")]
        public ErrorDto IVR_Cat_Reservas_Cuenta_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatReservaCuentaRequest? request)
        {
            return _bl.IVR_Cat_Reservas_Cuenta_Actualizar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete("IVR_Cat_Reservas_Cuenta_Eliminar")]
        public ErrorDto IVR_Cat_Reservas_Cuenta_Eliminar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrCatReservaCuentaRequest? request)
        {
            return _bl.IVR_Cat_Reservas_Cuenta_Eliminar(codEmpresa, request);
        }
    }
}
