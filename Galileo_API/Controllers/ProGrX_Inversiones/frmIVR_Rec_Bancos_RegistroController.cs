using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Rec_Bancos_Registro")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrRecBancosRegistroController
        : ControllerBase
    {
        private readonly FrmIvrRecBancosRegistroBl _bl;

        public FrmIvrRecBancosRegistroController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmIvrRecBancosRegistroBl(config);
        }

        [HttpGet("IVR_Rec_Bancos_Registro_Inicializar")]
        public ErrorDto<IvrRecBancosRegistroInicialData>
            IVR_Rec_Bancos_Registro_Inicializar(
                [FromQuery] int codEmpresa,
                [FromQuery]
                IvrRecBancosRegistroContextoRequest? request)
        {
            return _bl.IVR_Rec_Bancos_Registro_Inicializar(
                codEmpresa,
                request);
        }

        [HttpGet("IVR_Rec_Bancos_Registro_Transacciones_Obtener")]
        public ErrorDto<IvrRecBancosRegistroListaData>
            IVR_Rec_Bancos_Registro_Transacciones_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery]
                IvrRecBancosRegistroContextoRequest? request)
        {
            return _bl.IVR_Rec_Bancos_Registro_Transacciones_Obtener(
                codEmpresa,
                request);
        }

        [HttpGet("IVR_Rec_Bancos_Registro_Cuenta_Obtener")]
        public ErrorDto<CtnxCuentasDto>
            IVR_Rec_Bancos_Registro_Cuenta_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery]
                IvrRecBancosRegistroCuentaRequest? request)
        {
            return _bl.IVR_Rec_Bancos_Registro_Cuenta_Obtener(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("IVR_Rec_Bancos_Registro_Guardar")]
        public ErrorDto<IvrRecBancosRegistroGuardarData>
            IVR_Rec_Bancos_Registro_Guardar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrRecBancosRegistroGuardarRequest? request)
        {
            return _bl.IVR_Rec_Bancos_Registro_Guardar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete("IVR_Rec_Bancos_Registro_Eliminar")]
        public ErrorDto<int>
            IVR_Rec_Bancos_Registro_Eliminar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrRecBancosRegistroEliminarRequest? request)
        {
            return _bl.IVR_Rec_Bancos_Registro_Eliminar(
                codEmpresa,
                request);
        }
    }
}