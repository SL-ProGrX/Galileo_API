using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.ControlTramites;
using Galileo_API.BusinessLogic.ProGrX.ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.ControlTramites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfSeguimientoRevisionesTagController : ControllerBase
    {
        private readonly FrmAfSeguimientoRevisionesTagBL _bl;

        public FrmAfSeguimientoRevisionesTagController(IConfiguration config)
        {
            _bl = new FrmAfSeguimientoRevisionesTagBL(config);
        }

        [Authorize]
        [HttpGet("AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Obtener")]
        public ErrorDto<AfSeguimientoRevisionesTagAfiliacionesListaResult> AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _bl.AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Export")]
        public ErrorDto<AfSeguimientoRevisionesTagAfiliacionesListaResult> AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Export(int CodEmpresa, string parametros)
        {
            return _bl.AF_SeguimientoRevisionesTag_Afiliaciones_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("AF_SeguimientoRevisionesTag_Detalle_Obtener")]
        public ErrorDto<AfSeguimientoRevisionesTagDetalleData> AF_SeguimientoRevisionesTag_Detalle_Obtener(int CodEmpresa, string? cedula, long? consecutivo)
        {
            return _bl.AF_SeguimientoRevisionesTag_Detalle_Obtener(CodEmpresa, cedula, consecutivo);
        }

        [Authorize]
        [HttpGet("AF_SeguimientoRevisionesTag_Seguimiento_Lista_Obtener")]
        public ErrorDto<List<AfSeguimientoRevisionesTagSeguimientoData>> AF_SeguimientoRevisionesTag_Seguimiento_Lista_Obtener(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _bl.AF_SeguimientoRevisionesTag_Seguimiento_Lista_Obtener(CodEmpresa, cedula, consecutivo);
        }

        [Authorize]
        [HttpGet("AF_SeguimientoRevisionesTag_Seguimiento_Lista_Export")]
        public ErrorDto<List<AfSeguimientoRevisionesTagSeguimientoData>> AF_SeguimientoRevisionesTag_Seguimiento_Lista_Export(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _bl.AF_SeguimientoRevisionesTag_Seguimiento_Lista_Export(CodEmpresa, cedula, consecutivo);
        }

        [Authorize]
        [HttpGet("AF_SeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener")]
        public ErrorDto<List<AfSeguimientoRevisionesTagEtiquetaData>> AF_SeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener(int CodEmpresa, string? usuario)
        {
            return _bl.AF_SeguimientoRevisionesTag_Etiquetas_Dropdown_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("AF_SeguimientoRevisionesTag_Revisiones_Lista_Obtener")]
        public ErrorDto<List<AfSeguimientoRevisionesTagRevisionData>> AF_SeguimientoRevisionesTag_Revisiones_Lista_Obtener(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _bl.AF_SeguimientoRevisionesTag_Revisiones_Lista_Obtener(CodEmpresa, cedula, consecutivo);
        }

        [Authorize]
        [HttpGet("AF_SeguimientoRevisionesTag_Revisiones_Lista_Export")]
        public ErrorDto<List<AfSeguimientoRevisionesTagRevisionData>> AF_SeguimientoRevisionesTag_Revisiones_Lista_Export(int CodEmpresa, string? cedula, long consecutivo)
        {
            return _bl.AF_SeguimientoRevisionesTag_Revisiones_Lista_Export(CodEmpresa, cedula, consecutivo);
        }

        [Authorize]
        [HttpPost("AF_SeguimientoRevisionesTag_Aplicar")]
        public ErrorDto AF_SeguimientoRevisionesTag_Aplicar(int CodEmpresa, string? usuario, [FromBody] AfSeguimientoRevisionesTagAplicarRequest? request)
        {
            return _bl.AF_SeguimientoRevisionesTag_Aplicar(CodEmpresa, usuario, request);
        }
        [Authorize]
        [HttpPost("AF_SeguimientoRevisionesTag_Seleccion_Actualizar")]
        public ErrorDto<long?> AF_SeguimientoRevisionesTag_Seleccion_Actualizar(int CodEmpresa,string? usuario,[FromBody] AfSeguimientoRevisionesTagSeleccionRequest? request)
        {
            return _bl.AF_SeguimientoRevisionesTag_Seleccion_Actualizar(CodEmpresa,usuario,request);
        }
    }
}