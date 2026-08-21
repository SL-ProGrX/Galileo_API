using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.ControlTramites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifTagsGruposController : ControllerBase
    {
        private readonly FrmSifTagsGruposBL _bl;

        public FrmSifTagsGruposController(IConfiguration config)
        {
            _bl = new FrmSifTagsGruposBL(config);
        }

        [HttpGet("SIF_Grupos_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<SifGruposData>> SIF_Grupos_Lista_Obtener(int CodEmpresa)
        {
            return _bl.SIF_Grupos_Lista_Obtener(CodEmpresa);
        }

        [HttpGet("SIF_Grupos_Dropdown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> SIF_Grupos_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.SIF_Grupos_Dropdown_Obtener(CodEmpresa);
        }

        [HttpPost("SIF_Grupos_Guardar")]
        [Authorize]
        public ErrorDto SIF_Grupos_Guardar(int CodEmpresa, string Usuario, [FromBody] SifGruposGuardarRequest param)
        {
            return _bl.SIF_Grupos_Guardar(CodEmpresa, Usuario, param);
        }

        [HttpGet("SIF_Grupos_Miembros_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<SifGruposMiembroData>> SIF_Grupos_Miembros_Lista_Obtener(int CodEmpresa, string codGrupo)
        {
            return _bl.SIF_Grupos_Miembros_Lista_Obtener(CodEmpresa, codGrupo);
        }

        [HttpPost("SIF_Grupos_Miembro_Asignar")]
        [Authorize]
        public ErrorDto SIF_Grupos_Miembro_Asignar(int CodEmpresa, [FromBody] SifGruposMiembroAsignarRequest param)
        {
            return _bl.SIF_Grupos_Miembro_Asignar(CodEmpresa, param);
        }

        [HttpGet("SIF_Grupos_Tags_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<SifGruposTagData>> SIF_Grupos_Tags_Lista_Obtener(int CodEmpresa, string codGrupo)
        {
            return _bl.SIF_Grupos_Tags_Lista_Obtener(CodEmpresa, codGrupo);
        }

        [HttpPost("SIF_Grupos_Tag_Asignar")]
        [Authorize]
        public ErrorDto SIF_Grupos_Tag_Asignar(int CodEmpresa, [FromBody] SifGruposTagAsignarRequest param)
        {
            return _bl.SIF_Grupos_Tag_Asignar(CodEmpresa, param);
        }
    }
}