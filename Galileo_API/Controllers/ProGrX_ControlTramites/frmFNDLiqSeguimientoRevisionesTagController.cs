using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.ControlTramites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndLiqSeguimientoRevisionesTagController : ControllerBase
    {
        private readonly FrmFndLiqSeguimientoRevisionesTagBl _bl;

        public FrmFndLiqSeguimientoRevisionesTagController(IConfiguration config)
        {
            _bl = new FrmFndLiqSeguimientoRevisionesTagBl(config);
        }

        [Authorize]
        [HttpGet("FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Obtener")]
        public ErrorDto<FndLiqSeguimientoRevisionesTagLiquidacionesListaResult> FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Obtener(int CodEmpresa, string parametros, bool soloSinRetencion)
        {
            return _bl.FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Obtener(CodEmpresa, parametros, soloSinRetencion);
        }

        [Authorize]
        [HttpGet("FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Export")]
        public ErrorDto<FndLiqSeguimientoRevisionesTagLiquidacionesListaResult> FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Export(int CodEmpresa, string parametros, bool soloSinRetencion)
        {
            return _bl.FND_LiqSeguimientoRevisionesTag_Liquidaciones_Lista_Export(CodEmpresa, parametros, soloSinRetencion);
        }

        [Authorize]
        [HttpGet("FND_LiqSeguimientoRevisionesTag_Nombre_Obtener")]
        public ErrorDto<FndLiqSeguimientoRevisionesTagLiquidacionData> FND_LiqSeguimientoRevisionesTag_Nombre_Obtener(int CodEmpresa, string? cedula)
        {
            return _bl.FND_LiqSeguimientoRevisionesTag_Nombre_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Obtener")]
        public ErrorDto<List<FndLiqSeguimientoRevisionesTagSeguimientoData>> FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Obtener(int CodEmpresa, long consecutivo)
        {
            return _bl.FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Obtener(CodEmpresa, consecutivo);
        }

        [Authorize]
        [HttpGet("FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Export")]
        public ErrorDto<List<FndLiqSeguimientoRevisionesTagSeguimientoData>> FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Export(int CodEmpresa, long consecutivo)
        {
            return _bl.FND_LiqSeguimientoRevisionesTag_Seguimiento_Lista_Export(CodEmpresa, consecutivo);
        }

        [Authorize]
        [HttpGet("FND_LiqSeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener")]
        public ErrorDto<List<FndLiqSeguimientoRevisionesTagEtiquetaData>> FND_LiqSeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener(int CodEmpresa, string? usuario)
        {
            return _bl.FND_LiqSeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Obtener")]
        public ErrorDto<List<FndLiqSeguimientoRevisionesTagRevisionData>> FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Obtener(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _bl.FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Obtener(CodEmpresa, cedula, consecutivo);
        }

        [Authorize]
        [HttpGet("FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Export")]
        public ErrorDto<List<FndLiqSeguimientoRevisionesTagRevisionData>> FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Export(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _bl.FND_LiqSeguimientoRevisionesTag_Revisiones_Lista_Export(CodEmpresa, cedula, consecutivo);
        }

        [Authorize]
        [HttpPost("FND_LiqSeguimientoRevisionesTag_Seleccion_Actualizar")]
        public ErrorDto<long?> FND_LiqSeguimientoRevisionesTag_Seleccion_Actualizar(int CodEmpresa, string? usuario, [FromBody] FndLiqSeguimientoRevisionesTagSeleccionRequest? request)
        {
            return _bl.FND_LiqSeguimientoRevisionesTag_Seleccion_Actualizar(CodEmpresa, usuario, request);
        }

        [Authorize]
        [HttpPost("FND_LiqSeguimientoRevisionesTag_Aplicar")]
        public ErrorDto FND_LiqSeguimientoRevisionesTag_Aplicar(int CodEmpresa, string? usuario, [FromBody] FndLiqSeguimientoRevisionesTagAplicarRequest? request)
        {
            return _bl.FND_LiqSeguimientoRevisionesTag_Aplicar(CodEmpresa, usuario, request);
        }
    }
}