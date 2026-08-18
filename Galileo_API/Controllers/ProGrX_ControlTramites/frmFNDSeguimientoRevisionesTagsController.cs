using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmFndSeguimientoRevisionesTagsController : ControllerBase
    {
        private readonly FrmFndSeguimientoRevisionesTagsBl _bl;

        public FrmFndSeguimientoRevisionesTagsController(IConfiguration config)
        {
            _bl = new FrmFndSeguimientoRevisionesTagsBl(config);
        }

        [HttpGet("FND_frmFNDSeguimientoRevisionesTags_Fondos_Obtener")]
        public ErrorDto<List<FndSeguimientoRevisionFondoData>>
            FND_frmFNDSeguimientoRevisionesTags_Fondos_Obtener(
                int codEmpresa,
                string? cedula = null)
        {
            return _bl.FND_frmFNDSeguimientoRevisionesTags_Fondos_Obtener(
                codEmpresa,
                cedula);
        }

        [HttpGet("FND_frmFNDSeguimientoRevisionesTags_Detalle_Obtener")]
        public ErrorDto<FndSeguimientoRevisionDetalleData?>
            FND_frmFNDSeguimientoRevisionesTags_Detalle_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl.FND_frmFNDSeguimientoRevisionesTags_Detalle_Obtener(
                codEmpresa,
                request);
        }

        [HttpGet("FND_frmFNDSeguimientoRevisionesTags_Seguimiento_Obtener")]
        public ErrorDto<List<FndSeguimientoRevisionRegistroData>>
            FND_frmFNDSeguimientoRevisionesTags_Seguimiento_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl.FND_frmFNDSeguimientoRevisionesTags_Seguimiento_Obtener(
                codEmpresa,
                request);
        }

        [HttpGet("FND_frmFNDSeguimientoRevisionesTags_Etiquetas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            FND_frmFNDSeguimientoRevisionesTags_Etiquetas_Obtener(
                int codEmpresa,
                string usuario)
        {
            return _bl.FND_frmFNDSeguimientoRevisionesTags_Etiquetas_Obtener(
                codEmpresa,
                usuario);
        }

        [HttpGet("FND_frmFNDSeguimientoRevisionesTags_Aviso_Obtener")]
        public ErrorDto
            FND_frmFNDSeguimientoRevisionesTags_Aviso_Obtener(
                int codEmpresa,
                string tagCodigo)
        {
            return _bl.FND_frmFNDSeguimientoRevisionesTags_Aviso_Obtener(
                codEmpresa,
                tagCodigo);
        }

        [HttpGet("FND_frmFNDSeguimientoRevisionesTags_Omisiones_Obtener")]
        public ErrorDto<List<FndSeguimientoRevisionOmisionData>>
            FND_frmFNDSeguimientoRevisionesTags_Omisiones_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl.FND_frmFNDSeguimientoRevisionesTags_Omisiones_Obtener(
                codEmpresa,
                request);
        }

        [HttpPost("FND_frmFNDSeguimientoRevisionesTags_Omision_Cambiar")]
        public ErrorDto<FndSeguimientoRevisionOmisionCambiarData>
            FND_frmFNDSeguimientoRevisionesTags_Omision_Cambiar(
                int codEmpresa,
                FndSeguimientoRevisionOmisionCambiarRequest request)
        {
            return _bl.FND_frmFNDSeguimientoRevisionesTags_Omision_Cambiar(
                codEmpresa,
                request);
        }

        [HttpPost("FND_frmFNDSeguimientoRevisionesTags_Aplicar")]
        public ErrorDto
            FND_frmFNDSeguimientoRevisionesTags_Aplicar(
                int codEmpresa,
                FndSeguimientoRevisionAplicarRequest request)
        {
            return _bl.FND_frmFNDSeguimientoRevisionesTags_Aplicar(
                codEmpresa,
                request);
        }
    }
}