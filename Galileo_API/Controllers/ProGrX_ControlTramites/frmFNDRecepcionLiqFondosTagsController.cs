using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class FrmFNDRecepcionLiqFondosTagsController :
        ControllerBase
    {
        private readonly FrmFNDRecepcionLiqFondosTagsBl _bl;

        public FrmFNDRecepcionLiqFondosTagsController(
            IConfiguration config)
        {
            _bl = new FrmFNDRecepcionLiqFondosTagsBl(config);
        }

        [HttpGet(
            "FND_frmFNDRecepcionLiqFondosTags_Inicializar")]
        public ErrorDto<
            FndRecepcionLiqFondosTagsInicializarResponse>
            FND_frmFNDRecepcionLiqFondosTags_Inicializar(
                int codEmpresa)
        {
            return _bl
                .FND_frmFNDRecepcionLiqFondosTags_Inicializar(
                    codEmpresa);
        }

        [HttpGet(
            "FND_frmFNDRecepcionLiqFondosTags_Boleta_Obtener")]
        public ErrorDto<
            FndRecepcionLiqFondosTagsBoletaResponse?>
            FND_frmFNDRecepcionLiqFondosTags_Boleta_Obtener(
                int codEmpresa,
                long numeroBoleta,
                string movimiento)
        {
            return _bl
                .FND_frmFNDRecepcionLiqFondosTags_Boleta_Obtener(
                    codEmpresa,
                    numeroBoleta,
                    movimiento);
        }

        [HttpGet(
            "FND_frmFNDRecepcionLiqFondosTags_Pendientes_Obtener")]
        public ErrorDto<List<
            FndRecepcionLiqFondosTagsPendienteResponse>>
            FND_frmFNDRecepcionLiqFondosTags_Pendientes_Obtener(
                int codEmpresa,
                [FromQuery]
                FndRecepcionLiqFondosTagsPendientesRequest request)
        {
            return _bl
                .FND_frmFNDRecepcionLiqFondosTags_Pendientes_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpPost(
            "FND_frmFNDRecepcionLiqFondosTags_Movimiento_Aplicar")]
        public ErrorDto<
            FndRecepcionLiqFondosTagsAplicarResponse>
            FND_frmFNDRecepcionLiqFondosTags_Movimiento_Aplicar(
            int codEmpresa,
            FndRecepcionLiqFondosTagsAplicarRequest request)
        {
            request.usuario =
                User.Identity?.Name?.Trim()
                ?? string.Empty;

            return _bl
                .FND_frmFNDRecepcionLiqFondosTags_Movimiento_Aplicar(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "FND_frmFNDRecepcionLiqFondosTags_Historial_Obtener")]
        public ErrorDto<List<
        FndRecepcionLiqFondosTagsHistorialResponse>>
        FND_frmFNDRecepcionLiqFondosTags_Historial_Obtener(
            int codEmpresa,
            string request)
        {
            return _bl
                .FND_frmFNDRecepcionLiqFondosTags_Historial_Obtener(
                    codEmpresa,
                    request);
        }
    }
}