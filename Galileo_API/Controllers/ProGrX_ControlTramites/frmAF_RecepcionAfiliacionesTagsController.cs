using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class FrmAfRecepcionAfiliacionesTagsController
        : ControllerBase
    {
        private readonly FrmAfRecepcionAfiliacionesTagsBl _bl;

        public FrmAfRecepcionAfiliacionesTagsController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmAfRecepcionAfiliacionesTagsBl(config);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionAfiliacionesTags_Inicializar")]
        public ErrorDto<AfRecepcionAfiliacionesTagsInicializarResponse>
            AF_frmAF_RecepcionAfiliacionesTags_Inicializar(int codEmpresa)
        {
            return _bl.AF_frmAF_RecepcionAfiliacionesTags_Inicializar(
                codEmpresa);
        }

        [Authorize]
        [HttpPost(
            "AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Antiguos_Eliminar")]
        public ErrorDto<AfRecepcionAfiliacionesTagsMantenimientoResponse>
            AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Antiguos_Eliminar(
                int codEmpresa)
        {
            return _bl
                .AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Antiguos_Eliminar(
                    codEmpresa);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionAfiliacionesTags_Boletas_Obtener")]
        public ErrorDto<List<AfRecepcionAfiliacionesTagsBoletaResponse>>
            AF_frmAF_RecepcionAfiliacionesTags_Boletas_Obtener(
                int codEmpresa,
                string cedula,
                string movimiento)
        {
            return _bl.AF_frmAF_RecepcionAfiliacionesTags_Boletas_Obtener(
                codEmpresa,
                cedula,
                movimiento);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Obtener")]
        public ErrorDto<AfRecepcionAfiliacionesTagsAfiliacionResponse?>
            AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Obtener(
                int codEmpresa,
                string cedula,
                long numeroBoleta,
                string movimiento)
        {
            return _bl.AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Obtener(
                codEmpresa,
                cedula,
                numeroBoleta,
                movimiento);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Obtener")]
        public ErrorDto<List<AfRecepcionAfiliacionesTagsPendienteResponse>>
            AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Obtener(
                int codEmpresa,
                string movimiento)
        {
            return _bl.AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Obtener(
                codEmpresa,
                movimiento);
        }

        [Authorize]
        [HttpPost("AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Aplicar")]
        public ErrorDto<AfRecepcionAfiliacionesTagsAplicarResponse>
            AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Aplicar(
                int codEmpresa,
                AfRecepcionAfiliacionesTagsAplicarRequest request)
        {
            request.usuario = User.Identity?.Name?.Trim() ?? string.Empty;
            return _bl.AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Aplicar(
                codEmpresa,
                request);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionAfiliacionesTags_Historial_Obtener")]
        public ErrorDto<List<AfRecepcionAfiliacionesTagsHistorialResponse>>
            AF_frmAF_RecepcionAfiliacionesTags_Historial_Obtener(
                int codEmpresa,
                [FromQuery] AfRecepcionAfiliacionesTagsHistorialRequest request)
        {
            return _bl.AF_frmAF_RecepcionAfiliacionesTags_Historial_Obtener(
                codEmpresa,
                request);
        }
    }
}
