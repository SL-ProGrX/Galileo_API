using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRSeguimientoTagsController : ControllerBase
    {
        private readonly FrmCRSeguimientoTagsBL BL;

        public FrmCRSeguimientoTagsController(IConfiguration config)
        {
            BL = new FrmCRSeguimientoTagsBL(config);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoTags_Usuario_Obtener")]
        public ErrorDto<CrSeguimientoTagsUsuarioDto> CR_SeguimientoTags_Usuario_Obtener(
            int CodEmpresa,
            string usuario)
        {
            return BL.CR_SeguimientoTags_Usuario_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoTags_Etiquetas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_SeguimientoTags_Etiquetas_Dropdown_Obtener(
            int CodEmpresa,
            string usuario)
        {
            return BL.CR_SeguimientoTags_Etiquetas_Dropdown_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoTags_Operacion_Obtener")]
        public ErrorDto<CrSeguimientoTagsOperacionDto> CR_SeguimientoTags_Operacion_Obtener(
            int CodEmpresa,
            long operacion)
        {
            return BL.CR_SeguimientoTags_Operacion_Obtener(CodEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoTags_Lista_Obtener")]
        public ErrorDto<CrSeguimientoTagsLista> CR_SeguimientoTags_Lista_Obtener(
            int CodEmpresa,
            string parametros)
        {
            return BL.CR_SeguimientoTags_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoTags_Lista_Export")]
        public ErrorDto<CrSeguimientoTagsLista> CR_SeguimientoTags_Lista_Export(
            int CodEmpresa,
            string parametros)
        {
            return BL.CR_SeguimientoTags_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoTags_Aplicar")]
        public ErrorDto<CrSeguimientoTagsAplicarResult> CR_SeguimientoTags_Aplicar(
            int CodEmpresa,
            [FromBody] CrSeguimientoTagsAplicarRequest request)
        {
            return BL.CR_SeguimientoTags_Aplicar(CodEmpresa, request);
        }
    }
}