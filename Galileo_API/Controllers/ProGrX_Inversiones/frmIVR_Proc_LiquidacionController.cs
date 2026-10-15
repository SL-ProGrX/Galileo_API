using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Proc_Liquidacion")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrProcLiquidacionController
        : ControllerBase
    {
        private readonly FrmIvrProcLiquidacionBl _bl;

        public FrmIvrProcLiquidacionController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmIvrProcLiquidacionBl(config);
        }

        [HttpGet(
            "IVR_Proc_Liquidacion_Inicializar")]
        public ErrorDto<IvrProcLiquidacionInicialData>
            IVR_Proc_Liquidacion_Inicializar(
                [FromQuery] int CodEmpresa,
                [FromQuery]
                IvrProcLiquidacionInicializarRequest? request)
        {
            return _bl.IVR_Proc_Liquidacion_Inicializar(
                CodEmpresa,
                request);
        }

        [HttpGet(
            "IVR_Proc_Liquidacion_Transacciones_Obtener")]
        public ErrorDto<IvrProcLiquidacionTransaccionesData>
            IVR_Proc_Liquidacion_Transacciones_Obtener(
                [FromQuery] int CodEmpresa,
                [FromQuery] int transacId)
        {
            return _bl
                .IVR_Proc_Liquidacion_Transacciones_Obtener(
                    CodEmpresa,
                    transacId);
        }

        [ValidateAntiForgeryToken]
        [HttpPost(
            "IVR_Proc_Liquidacion_Solicitud_Guardar")]
        public ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Solicitud_Guardar(
                [FromQuery] int CodEmpresa,
                [FromBody]
                IvrProcLiquidacionGuardarRequest? request)
        {
            return _bl
                .IVR_Proc_Liquidacion_Solicitud_Guardar(
                    CodEmpresa,
                    request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut(
            "IVR_Proc_Liquidacion_Procesar")]
        public ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Procesar(
                [FromQuery] int CodEmpresa,
                [FromBody]
                IvrProcLiquidacionGuardarRequest? request)
        {
            return _bl.IVR_Proc_Liquidacion_Procesar(
                CodEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete(
            "IVR_Proc_Liquidacion_Eliminar")]
        public ErrorDto<IvrProcLiquidacionGuardarData>
            IVR_Proc_Liquidacion_Eliminar(
                [FromQuery] int CodEmpresa,
                [FromBody]
                IvrProcLiquidacionGuardarRequest? request)
        {
            return _bl.IVR_Proc_Liquidacion_Eliminar(
                CodEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete(
            "IVR_Proc_Liquidacion_Transaccion_Eliminar")]
        public ErrorDto
            IVR_Proc_Liquidacion_Transaccion_Eliminar(
                [FromQuery] int CodEmpresa,
                [FromBody]
                IvrProcLiquidacionTransaccionEliminarRequest?
                    request)
        {
            return _bl
                .IVR_Proc_Liquidacion_Transaccion_Eliminar(
                    CodEmpresa,
                    request);
        }
    }
}