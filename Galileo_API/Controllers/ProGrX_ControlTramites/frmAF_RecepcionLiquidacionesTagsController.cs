using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route(
        "api/FrmAFRecepcionLiquidacionesTags")]
    [Authorize]
    public sealed class
        FrmAfRecepcionLiquidacionesTagsController :
        ControllerBase
    {
        private readonly
            FrmAfRecepcionLiquidacionesTagsBl _bl;

        public FrmAfRecepcionLiquidacionesTagsController(
            IConfiguration config)
        {
            _bl =
                new FrmAfRecepcionLiquidacionesTagsBl(
                    config);
        }

        [HttpGet(
            "AF_frmAF_RecepcionLiquidacionesTag_Inicializar")]
        public ErrorDto<
            AfRecepcionLiquidacionesTagInicializarResponse>
            AF_frmAF_RecepcionLiquidacionesTag_Inicializar(
                int codEmpresa)
        {
            return _bl
                .AF_frmAF_RecepcionLiquidacionesTag_Inicializar(
                    codEmpresa);
        }

        [HttpGet(
            "AF_frmAF_RecepcionLiquidacionesTag_Liquidacion_Obtener")]
        public ErrorDto<
            AfRecepcionLiquidacionesTagLiquidacionResponse?>
            AF_frmAF_RecepcionLiquidacionesTag_Liquidacion_Obtener(
                int codEmpresa,
                long numeroBoleta,
                string movimiento)
        {
            return _bl
                .AF_frmAF_RecepcionLiquidacionesTag_Liquidacion_Obtener(
                    codEmpresa,
                    numeroBoleta,
                    movimiento);
        }

        [HttpPost(
            "AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Aplicar")]
        public ErrorDto<
            AfRecepcionLiquidacionesTagAplicarResponse>
            AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Aplicar(
                int codEmpresa,
                AfRecepcionLiquidacionesTagAplicarRequest?
                    request)
        {
            return _bl
                .AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Aplicar(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "AF_frmAF_RecepcionLiquidacionesTag_Pendientes_Obtener")]
        public ErrorDto<List<
            AfRecepcionLiquidacionesTagPendienteResponse>>
            AF_frmAF_RecepcionLiquidacionesTag_Pendientes_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .AF_frmAF_RecepcionLiquidacionesTag_Pendientes_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "AF_frmAF_RecepcionLiquidacionesTag_Historial_Obtener")]
        public ErrorDto<List<
            AfRecepcionLiquidacionesTagHistorialResponse>>
            AF_frmAF_RecepcionLiquidacionesTag_Historial_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .AF_frmAF_RecepcionLiquidacionesTag_Historial_Obtener(
                    codEmpresa,
                    request);
        }
    }
}