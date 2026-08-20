namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    using Galileo.Models.ERROR;
    using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
    using Galileo_API.Models.ProGrX_ControlTramites;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAfLiquidacionRevisionController : ControllerBase
    {
        private readonly FrmAfLiquidacionRevisionBL _bl;

        public FrmAfLiquidacionRevisionController(IConfiguration config)
        {
            _bl = new FrmAfLiquidacionRevisionBL(config);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionRevision_Obtener")]
        public ErrorDto<List<AfLiquidacionRevisionListaModel>> AF_LiquidacionRevision_Obtener(
            int CodEmpresa,
            string? Cedula)
        {
            return _bl.AF_LiquidacionRevision_Obtener(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionRevision_Detalle_Obtener")]
        public ErrorDto<AfLiquidacionRevisionDetalleModel?> AF_LiquidacionRevision_Detalle_Obtener(
            int CodEmpresa,
            int Consec)
        {
            return _bl.AF_LiquidacionRevision_Detalle_Obtener(CodEmpresa, Consec);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionRevision_Operaciones_Obtener")]
        public ErrorDto<List<AfLiquidacionRevisionOperacionModel>> AF_LiquidacionRevision_Operaciones_Obtener(
            int CodEmpresa,
            int Consec)
        {
            return _bl.AF_LiquidacionRevision_Operaciones_Obtener(CodEmpresa, Consec);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionRevision_Seguimiento_Obtener")]
        public ErrorDto<List<AfLiquidacionRevisionSeguimientoModel>> AF_LiquidacionRevision_Seguimiento_Obtener(
            int CodEmpresa,
            string Cedula,
            string Documento)
        {
            return _bl.AF_LiquidacionRevision_Seguimiento_Obtener(CodEmpresa, Cedula, Documento);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionRevision_Etiquetas_Obtener")]
        public ErrorDto<List<AfLiquidacionRevisionEtiquetaModel>> AF_LiquidacionRevision_Etiquetas_Obtener(
            int CodEmpresa,
            string Usuario)
        {
            return _bl.AF_LiquidacionRevision_Etiquetas_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionRevision_Omisiones_Obtener")]
        public ErrorDto<List<AfLiquidacionRevisionOmisionModel>> AF_LiquidacionRevision_Omisiones_Obtener(
            int CodEmpresa,
            string Cedula,
            string Documento)
        {
            return _bl.AF_LiquidacionRevision_Omisiones_Obtener(CodEmpresa, Cedula, Documento);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionRevision_Aviso_Obtener")]
        public ErrorDto<AfLiquidacionRevisionAvisoModel?> AF_LiquidacionRevision_Aviso_Obtener(
            int CodEmpresa,
            string TagCodigo)
        {
            return _bl.AF_LiquidacionRevision_Aviso_Obtener(CodEmpresa, TagCodigo);
        }

        [Authorize]
        [HttpPost("AF_LiquidacionRevision_Omision_Insertar")]
        public ErrorDto<AfLiquidacionRevisionOmisionInsertarModel?> AF_LiquidacionRevision_Omision_Insertar(
            int CodEmpresa,
            AfLiquidacionRevisionOmisionInsertarRequest request)
        {
            return _bl.AF_LiquidacionRevision_Omision_Insertar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_LiquidacionRevision_Omision_Eliminar")]
        public ErrorDto AF_LiquidacionRevision_Omision_Eliminar(
            int CodEmpresa,
            AfLiquidacionRevisionOmisionEliminarRequest request)
        {
            return _bl.AF_LiquidacionRevision_Omision_Eliminar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_LiquidacionRevision_Omisiones_Aplicar")]
        public ErrorDto AF_LiquidacionRevision_Omisiones_Aplicar(
            int CodEmpresa,
            AfLiquidacionRevisionOmisionesAplicarRequest request)
        {
            return _bl.AF_LiquidacionRevision_Omisiones_Aplicar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_LiquidacionRevision_Aplicar")]
        public ErrorDto AF_LiquidacionRevision_Aplicar(
            int CodEmpresa,
            AfLiquidacionRevisionAplicarRequest request)
        {
            return _bl.AF_LiquidacionRevision_Aplicar(CodEmpresa, request);
        }
    }
}
