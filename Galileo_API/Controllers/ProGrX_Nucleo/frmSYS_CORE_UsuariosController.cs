using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysCoreUsuariosController : ControllerBase
    {
        private readonly FrmSysCoreUsuariosBL _bl;
        public FrmSysCoreUsuariosController(IConfiguration config)
        {
            _bl = new FrmSysCoreUsuariosBL(config);
        }

        [HttpGet("CoreUsuariosLista_Obtener")]
        [Authorize]
        public ErrorDto<CoreUsuariosLista> CoreUsuariosLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CoreUsuariosLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("CoreUsuariosExiste_Obtener")]
        [Authorize]
        public ErrorDto CoreUsuariosExiste_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.CoreUsuariosExiste_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("CoreUsuarios_Obtener")]
        [Authorize]
        public ErrorDto<CoreUsuariosData> CoreUsuarios_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.CoreUsuarios_Obtener(CodEmpresa, usuario);
        }

        [HttpPut("CoreUsuarios_Importar")]
        [Authorize]
        public ErrorDto CoreUsuarios_Importar(int CodEmpresa)
        {
            return _bl.CoreUsuarios_Importar(CodEmpresa);
        }

        [HttpGet("CoreUsuario_Scroll")]
        [Authorize]
        public ErrorDto<CoreUsuariosData> CoreUsuario_Scroll(int CodEmpresa, int scroll, string? usuario)
        {
            return _bl.CoreUsuario_Scroll(CodEmpresa, scroll, usuario);
        }

        [HttpPost("CoreUsuarios_Guardar")]
        [Authorize]
        public ErrorDto CoreUsuarios_Guardar(int CodEmpresa, CoreUsuariosData usuariosData)
        {
            return _bl.CoreUsuarios_Guardar(CodEmpresa, usuariosData);
        }

        [HttpDelete("CoreUsuarios_Eliminar")]
        [Authorize]
        public ErrorDto CoreUsuarios_Eliminar(int CodEmpresa, string usuario)
        {
            return _bl.CoreUsuarios_Eliminar(CodEmpresa, usuario);
        }

        [HttpGet("CoreUsuariosMiembros_Obtener")]
        [Authorize]
        public ErrorDto<List<CoreMiembrosData>> CoreUsuariosMiembros_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.CoreUsuariosMiembros_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("CoreUsuariosUENs_Roles_Obtener")]
        [Authorize]
        public ErrorDto<List<CoreMiembrosRolData>> CoreUsuariosUENs_Roles_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.CoreUsuariosUENs_Roles_Obtener(CodEmpresa, usuario);
        }

        [HttpPatch("CoreUsuariosMiembro_Actualiza")]
        [Authorize]
        public ErrorDto CoreUsuariosMiembro_Actualiza(string miembro)
        {
            return _bl.CoreUsuariosMiembro_Actualiza(miembro);
        }

        [HttpPatch("CoreUsuariosMiembroRol_Actualiza")]
        [Authorize]
        public ErrorDto CoreUsuariosMiembroRol_Actualiza(string miembroRol)
        {
            return _bl.CoreUsuariosMiembroRol_Actualiza(miembroRol);
        }
    }
}