using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_ControlTramites.FrmSifTagsModels;

namespace Galileo_API.Controllers.ProGrX.ControlTramites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifTagsController : ControllerBase
    {
        private readonly FrmSifTagsBL _bl;

        public FrmSifTagsController(IConfiguration config)
        {
            _bl = new FrmSifTagsBL(config);
        }

        [HttpGet("SIF_Tags_Lista_Obtener")]
        [Authorize]
        public ErrorDto<SifTagsListaResult> SIF_Tags_Lista_Obtener(int CodEmpresa, string filtro)
        {
            return _bl.SIF_Tags_Lista_Obtener(CodEmpresa, filtro);
        }

        [HttpPost("SIF_Tags_Guardar")]
        [Authorize]
        public ErrorDto SIF_Tags_Guardar(int CodEmpresa, bool vEdita, string Usuario, [FromBody] SifTagsData param)
        {
            return _bl.SIF_Tags_Guardar(CodEmpresa, vEdita, Usuario, param);
        }

        [HttpGet("SIF_Tags_Dropdown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> SIF_Tags_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.SIF_Tags_Dropdown_Obtener(CodEmpresa);
        }

        [HttpGet("SIF_Tags_Notificacion_Obtener")]
        [Authorize]
        public ErrorDto<SifTagsNotificacionDto> SIF_Tags_Notificacion_Obtener(int CodEmpresa, string tagCodigo)
        {
            return _bl.SIF_Tags_Notificacion_Obtener(CodEmpresa, tagCodigo);
        }

        [HttpPost("SIF_Tags_Notificacion_Guardar")]
        [Authorize]
        public ErrorDto SIF_Tags_Notificacion_Guardar(int CodEmpresa, [FromBody] SifTagsNotificacionDto param)
        {
            return _bl.SIF_Tags_Notificacion_Guardar(CodEmpresa, param);
        }

        [HttpDelete("SIF_Tags_Notificacion_Eliminar")]
        [Authorize]
        public ErrorDto SIF_Tags_Notificacion_Eliminar(int CodEmpresa, string tagCodigo)
        {
            return _bl.SIF_Tags_Notificacion_Eliminar(CodEmpresa, tagCodigo);
        }
    }
}