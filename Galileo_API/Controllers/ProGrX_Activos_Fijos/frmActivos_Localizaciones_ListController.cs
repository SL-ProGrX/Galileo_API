using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosLocalizacionesListController : ControllerBase
    {
        
        private readonly FrmActivosLocalizacionesListBl _bl;

        public FrmActivosLocalizacionesListController(IConfiguration config)
        {
            _bl = new FrmActivosLocalizacionesListBl(config);
        }

        [Authorize]
        [HttpGet("Activos_LocalizacionesLista_Obtener")]
        public ErrorDto<ActivosLocalizacionesLista> Activos_LocalizacionesLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_LocalizacionesLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Activos_Localizaciones_Obtener")]
        public ErrorDto<List<ActivosLocalizacionesData>> Activos_Localizaciones_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_Localizaciones_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Activos_Localizaciones_Guardar")]
        public ErrorDto Activos_Localizaciones_Guardar(int CodEmpresa, string usuario, ActivosLocalizacionesData localizacion)
        {
            return _bl.Activos_Localizaciones_Guardar(CodEmpresa, usuario, localizacion);
        }

        [Authorize]
        [HttpDelete("Activos_Localizaciones_Eliminar")]
        public ErrorDto Activos_Localizaciones_Eliminar(int CodEmpresa, string cod_localiza, string usuario)
        {
            return _bl.Activos_Localizaciones_Eliminar(CodEmpresa, usuario, cod_localiza);
        }

        [Authorize]
        [HttpGet("Activos_Localizaciones_Valida")]
        public ErrorDto Activos_Localizaciones_Valida(int CodEmpresa, string cod_localiza)
        {
            return _bl.Activos_Localizaciones_Valida(CodEmpresa, cod_localiza);
        }
    }
}