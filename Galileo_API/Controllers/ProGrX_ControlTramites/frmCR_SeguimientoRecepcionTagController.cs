using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route(
        "api/FrmCRSeguimientoRecepcionTag")]
    [Authorize]
    public sealed class
        FrmCrSeguimientoRecepcionTagController :
        ControllerBase
    {
        private readonly
            FrmCrSeguimientoRecepcionTagBl _bl;

        public FrmCrSeguimientoRecepcionTagController(
            IConfiguration config)
        {
            _bl =
                new FrmCrSeguimientoRecepcionTagBl(
                    config);
        }

        [HttpGet(
            "CR_frmCR_SeguimientoRecepcionTag_Inicializar")]
        public ErrorDto<
            CrSeguimientoRecepcionTagInicializarResponse>
            CR_frmCR_SeguimientoRecepcionTag_Inicializar(
                int codEmpresa)
        {
            return _bl
                .CR_frmCR_SeguimientoRecepcionTag_Inicializar(
                    codEmpresa);
        }

        [HttpGet(
            "CR_frmCR_SeguimientoRecepcionTag_Operacion_Obtener")]
        public ErrorDto<
            CrSeguimientoRecepcionTagOperacionResponse?>
            CR_frmCR_SeguimientoRecepcionTag_Operacion_Obtener(
                int codEmpresa,
                long idSolicitud,
                string movimiento)
        {
            return _bl
                .CR_frmCR_SeguimientoRecepcionTag_Operacion_Obtener(
                    codEmpresa,
                    idSolicitud,
                    movimiento);
        }

        [HttpGet(
            "CR_frmCR_SeguimientoRecepcionTag_Pendientes_Obtener")]
        public ErrorDto<List<
            CrSeguimientoRecepcionTagPendienteResponse>>
            CR_frmCR_SeguimientoRecepcionTag_Pendientes_Obtener(
                int codEmpresa,
                CrSeguimientoRecepcionTagPendientesRequest
                    request)
        {
            return _bl
                .CR_frmCR_SeguimientoRecepcionTag_Pendientes_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpPost(
            "CR_frmCR_SeguimientoRecepcionTag_Movimiento_Aplicar")]
        public ErrorDto<
            CrSeguimientoRecepcionTagAplicarResponse>
            CR_frmCR_SeguimientoRecepcionTag_Movimiento_Aplicar(
                int codEmpresa,
                CrSeguimientoRecepcionTagAplicarRequest
                    request)
        {
            request.usuario =
                User.Identity?.Name?.Trim()
                ?? string.Empty;

            return _bl
                .CR_frmCR_SeguimientoRecepcionTag_Movimiento_Aplicar(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "CR_frmCR_SeguimientoRecepcionTag_Historial_Obtener")]
        public ErrorDto<List<
            CrSeguimientoRecepcionTagHistorialResponse>>
            CR_frmCR_SeguimientoRecepcionTag_Historial_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .CR_frmCR_SeguimientoRecepcionTag_Historial_Obtener(
                    codEmpresa,
                    request);
        }
    }
}