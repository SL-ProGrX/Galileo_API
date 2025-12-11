using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosTiposActivoController : ControllerBase
    {
        private readonly FrmActivosTiposActivoBL _bl;

        public FrmActivosTiposActivoController(IConfiguration config)
        {
            _bl = new FrmActivosTiposActivoBL(config);
        }

        
        [HttpGet("Activos_TiposActivoLista_Obtener")]
        [Authorize]
        public ErrorDto<ActivosTiposActivosLista> Activos_TiposActivoLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_TiposActivosLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Activos_TiposActivoExiste_Obtener")]
        [Authorize]
        public ErrorDto Activos_TiposActivoExiste_Obtener(int CodEmpresa, string tipo_activo)
        {
            return _bl.Activos_TiposActivosExiste_Obtener(CodEmpresa, tipo_activo);
        }

        [HttpGet("Activos_TiposActivo_Obtener")]
        [Authorize]
        public ErrorDto<ActivosTiposActivosData> Activos_TiposActivo_Obtener(int CodEmpresa, string tipo_activo)
        {
            return _bl.Activos_TiposActivos_Obtener(CodEmpresa, tipo_activo);
        }

        [HttpGet("Activos_TiposActivo_MetodosDepreciacion_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_TiposActivo_Obtener(int CodEmpresa)
        {
            return _bl.Activos_TiposActivos_MetodosDepreciacion_Obtener(CodEmpresa);
        }

        [HttpGet("Activos_TiposActivo_Scroll")]
        [Authorize]
        public ErrorDto<ActivosTiposActivosData> Activos_TiposActivo_Scroll(int CodEmpresa, int scroll, string? tipo_activo)
        {
            return _bl.Activos_TiposActivos_Scroll(CodEmpresa, scroll, tipo_activo);
        }

        [HttpPost("Activos_TiposActivo_Guardar")]
        [Authorize]
        public ErrorDto Activos_TiposActivo_Guardar(int CodEmpresa, ActivosTiposActivosData tiposActivoData)
        {
            return _bl.Activos_TiposActivos_Guardar(CodEmpresa, tiposActivoData);
        }

        [HttpDelete("Activos_TiposActivo_Eliminar")]
        [Authorize]
        public ErrorDto Activos_TiposActivo_Eliminar(int CodEmpresa, string usuario, string tipo_activo)
        {
            return _bl.Activos_TiposActivos_Eliminar(CodEmpresa, usuario, tipo_activo);
        }
        
        [HttpGet("Activos_Activos_TiposActivo_TipoVidaUtil_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_TiposActivos_TipoVidaUtil_Obtener()
        {
            return _bl.Activos_TiposActivos_TipoVidaUtil_Obtener();
        }

        [HttpGet("Activos_TiposActivo_TiposAsientos_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_TiposActivos_TiposAsientos_Obtener(int CodEmpresa, int contabilidad)
        {
            return _bl.Activos_TiposActivos_TiposAsientos_Obtener(CodEmpresa, contabilidad);
        }
    }
}
