using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class FrmAfRecepcionBeneficiosTagsController : ControllerBase
    {
        private readonly FrmAfRecepcionBeneficiosTagsBl _bl;

        public FrmAfRecepcionBeneficiosTagsController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmAfRecepcionBeneficiosTagsBl(config);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionBeneficiosTags_Inicializar")]
        public ErrorDto<AfRecepcionBeneficiosTagsInicializarResponse>
            AF_frmAF_RecepcionBeneficiosTags_Inicializar(int codEmpresa)
        {
            return _bl.AF_frmAF_RecepcionBeneficiosTags_Inicializar(codEmpresa);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionBeneficiosTags_Beneficio_Obtener")]
        public ErrorDto<AfRecepcionBeneficiosTagsBeneficioResponse?>
            AF_frmAF_RecepcionBeneficiosTags_Beneficio_Obtener(
                int codEmpresa,
                string codBeneficio,
                long consec,
                string movimiento)
        {
            return _bl.AF_frmAF_RecepcionBeneficiosTags_Beneficio_Obtener(
                codEmpresa,
                codBeneficio,
                consec,
                movimiento);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionBeneficiosTags_Pendientes_Obtener")]
        public ErrorDto<List<AfRecepcionBeneficiosTagsPendienteResponse>>
            AF_frmAF_RecepcionBeneficiosTags_Pendientes_Obtener(
                int codEmpresa,
                string movimiento)
        {
            return _bl.AF_frmAF_RecepcionBeneficiosTags_Pendientes_Obtener(
                codEmpresa,
                movimiento);
        }

        [Authorize]
        [HttpPost("AF_frmAF_RecepcionBeneficiosTags_Movimiento_Aplicar")]
        public ErrorDto<AfRecepcionBeneficiosTagsAplicarResponse>
            AF_frmAF_RecepcionBeneficiosTags_Movimiento_Aplicar(
                int codEmpresa,
                AfRecepcionBeneficiosTagsAplicarRequest request)
        {
            request.usuario = User.Identity?.Name?.Trim() ?? string.Empty;
            return _bl.AF_frmAF_RecepcionBeneficiosTags_Movimiento_Aplicar(
                codEmpresa,
                request);
        }

        [Authorize]
        [HttpGet("AF_frmAF_RecepcionBeneficiosTags_Historial_Obtener")]
        public ErrorDto<List<AfRecepcionBeneficiosTagsHistorialResponse>>
            AF_frmAF_RecepcionBeneficiosTags_Historial_Obtener(
                int codEmpresa,
                [FromQuery] AfRecepcionBeneficiosTagsHistorialRequest request)
        {
            return _bl.AF_frmAF_RecepcionBeneficiosTags_Historial_Obtener(
                codEmpresa,
                request);
        }
    }
}
