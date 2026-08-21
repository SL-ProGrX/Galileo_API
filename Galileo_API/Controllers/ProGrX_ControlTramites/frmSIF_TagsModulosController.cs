using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.ControlTramites;
using Galileo_API.BusinessLogic.ProGrX.ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.ControlTramites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSIFTagsModulosController : ControllerBase
    {
        private readonly FrmSifTagsModulosBl _bl;

        public FrmSIFTagsModulosController(IConfiguration config)
        {
            _bl = new FrmSifTagsModulosBl(config);
        }

        [Authorize]
        [HttpGet("SIF_TagsModulos_Procesos_Lista_Obtener")]
        public ErrorDto<List<SifTagsModulosProcesoData>> SIF_TagsModulos_Procesos_Lista_Obtener(int CodEmpresa)
        {
            return _bl.SIF_TagsModulos_Procesos_Lista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SIF_TagsModulos_Procesos_Lista_Export")]
        public ErrorDto<List<SifTagsModulosProcesoData>> SIF_TagsModulos_Procesos_Lista_Export(int CodEmpresa)
        {
            return _bl.SIF_TagsModulos_Procesos_Lista_Export(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SIF_TagsModulos_Procesos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> SIF_TagsModulos_Procesos_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.SIF_TagsModulos_Procesos_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("SIF_TagsModulos_Proceso_Guardar")]
        public ErrorDto SIF_TagsModulos_Proceso_Guardar(int CodEmpresa, string? usuario, [FromBody] SifTagsModulosProcesoGuardarRequest? request)
        {
            return _bl.SIF_TagsModulos_Proceso_Guardar(CodEmpresa, usuario, request);
        }

        [Authorize]
        [HttpGet("SIF_TagsModulos_Etiquetas_Lista_Obtener")]
        public ErrorDto<List<SifTagsModulosEtiquetaData>> SIF_TagsModulos_Etiquetas_Lista_Obtener(int CodEmpresa, string? codModulo)
        {
            return _bl.SIF_TagsModulos_Etiquetas_Lista_Obtener(CodEmpresa, codModulo);
        }

        [Authorize]
        [HttpPost("SIF_TagsModulos_Etiqueta_Guardar")]
        public ErrorDto SIF_TagsModulos_Etiqueta_Guardar(int CodEmpresa, [FromBody] SifTagsModulosEtiquetaGuardarRequest? request)
        {
            return _bl.SIF_TagsModulos_Etiqueta_Guardar(CodEmpresa, request);
        }
    }
}