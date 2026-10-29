using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Proc_Cupones")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrProcCuponesController
        : ControllerBase
    {
        private readonly FrmIvrProcCuponesBl _bl;

        public FrmIvrProcCuponesController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmIvrProcCuponesBl(config);
        }

        [HttpGet(
            "IVR_Proc_Cupones_Inicializar")]
        public ErrorDto<IvrProcCuponesInicialData>
            IVR_Proc_Cupones_Inicializar(
                [FromQuery] int codEmpresa,
                [FromQuery]
                IvrProcCuponesInicializarRequest? request)
        {
            return _bl.IVR_Proc_Cupones_Inicializar(
                codEmpresa,
                request);
        }

        [HttpGet(
            "IVR_Proc_Cupones_Transacciones_Obtener")]
        public ErrorDto<IvrProcCuponesTransaccionesData>
            IVR_Proc_Cupones_Transacciones_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] int transacId)
        {
            return _bl
                .IVR_Proc_Cupones_Transacciones_Obtener(
                    codEmpresa,
                    transacId);
        }

        [ValidateAntiForgeryToken]
        [HttpPost(
            "IVR_Proc_Cupones_Solicitud_Guardar")]
        public ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Solicitud_Guardar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrProcCuponesGuardarRequest? request)
        {
            return _bl
                .IVR_Proc_Cupones_Solicitud_Guardar(
                    codEmpresa,
                    request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut(
            "IVR_Proc_Cupones_Procesar")]
        public ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Procesar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrProcCuponesGuardarRequest? request)
        {
            return _bl.IVR_Proc_Cupones_Procesar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete(
            "IVR_Proc_Cupones_Eliminar")]
        public ErrorDto<IvrProcCuponesGuardarData>
            IVR_Proc_Cupones_Eliminar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrProcCuponesGuardarRequest? request)
        {
            return _bl.IVR_Proc_Cupones_Eliminar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete(
            "IVR_Proc_Cupones_Transaccion_Eliminar")]
        public ErrorDto
            IVR_Proc_Cupones_Transaccion_Eliminar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrProcCuponesTransaccionEliminarRequest?
                    request)
        {
            return _bl
                .IVR_Proc_Cupones_Transaccion_Eliminar(
                    codEmpresa,
                    request);
        }
    }
}