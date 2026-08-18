using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmFndRecepcionFondosTagsController : ControllerBase
    {
        private readonly FrmFndRecepcionFondosTagsBl _BL;

        public FrmFndRecepcionFondosTagsController(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _BL = new FrmFndRecepcionFondosTagsBl(config);
        }

        [Authorize]
        [HttpGet("FND_frmFNDRecepcionFondosTags_Inicializar")]
        public ErrorDto<FndRecepcionFondosTagsInicializarResponse>
            FND_frmFNDRecepcionFondosTags_Inicializar(int codEmpresa)
        {
            return _BL.FND_frmFNDRecepcionFondosTags_Inicializar(codEmpresa);
        }

        [Authorize]
        [HttpGet("FND_frmFNDRecepcionFondosTags_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            FND_frmFNDRecepcionFondosTags_Planes_Obtener(int codEmpresa)
        {
            return _BL.FND_frmFNDRecepcionFondosTags_Planes_Obtener(
                codEmpresa);
        }

        [Authorize]
        [HttpGet("FND_frmFNDRecepcionFondosTags_Contratos_Obtener")]
        public ErrorDto<List<FndRecepcionFondosTagsContratoBusquedaResponse>>
            FND_frmFNDRecepcionFondosTags_Contratos_Obtener(
                int codEmpresa,
                string codPlan,
                string cedula = "")
        {
            return _BL.FND_frmFNDRecepcionFondosTags_Contratos_Obtener(
                codEmpresa,
                codPlan,
                cedula);
        }

        [Authorize]
        [HttpGet("FND_frmFNDRecepcionFondosTags_Contrato_Obtener")]
        public ErrorDto<FndRecepcionFondosTagsContratoResponse?>
            FND_frmFNDRecepcionFondosTags_Contrato_Obtener(
                int codEmpresa,
                string codPlan,
                long codContrato,
                string movimiento)
        {
            return _BL.FND_frmFNDRecepcionFondosTags_Contrato_Obtener(
                codEmpresa,
                codPlan,
                codContrato,
                movimiento);
        }

        [Authorize]
        [HttpGet("FND_frmFNDRecepcionFondosTags_Pendientes_Obtener")]
        public ErrorDto<List<FndRecepcionFondosTagsPendienteResponse>>
            FND_frmFNDRecepcionFondosTags_Pendientes_Obtener(
                int codEmpresa,
                string movimiento)
        {
            return _BL.FND_frmFNDRecepcionFondosTags_Pendientes_Obtener(
                codEmpresa,
                movimiento);
        }

        [Authorize]
        [HttpPost("FND_frmFNDRecepcionFondosTags_Movimiento_Aplicar")]
        public ErrorDto<FndRecepcionFondosTagsAplicarResponse>
            FND_frmFNDRecepcionFondosTags_Movimiento_Aplicar(
                int codEmpresa,
                FndRecepcionFondosTagsAplicarRequest request)
        {
            request.usuario = User.Identity?.Name?.Trim() ?? string.Empty;
            return _BL.FND_frmFNDRecepcionFondosTags_Movimiento_Aplicar(
                codEmpresa,
                request);
        }

        [Authorize]
        [HttpGet("FND_frmFNDRecepcionFondosTags_Historial_Obtener")]
        public ErrorDto<List<FndRecepcionFondosTagsHistorialResponse>>
            FND_frmFNDRecepcionFondosTags_Historial_Obtener(
                int codEmpresa,
                [FromQuery] FndRecepcionFondosTagsHistorialRequest request)
        {
            return _BL.FND_frmFNDRecepcionFondosTags_Historial_Obtener(
                codEmpresa,
                request);
        }
    }
}
