using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Galileo.Models.Auth;
using System.Security.Claims;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmUsUsuariosController : ControllerBase
    {
        readonly FrmUsUsuariosBl UsuariosBL;
        private readonly IConfiguration _config;

        public FrmUsUsuariosController(IConfiguration config)
        {
            _config = config;
            UsuariosBL = new FrmUsUsuariosBl(config);
        }


        [HttpPost("UsuarioExiste")]
        public ErrorDto<int> UsuarioExiste(string usuario)
        {
            return UsuariosBL.UsuarioExiste(usuario);
        }


        [HttpGet("UsuariosEmpresaObtener")]
        public ErrorDto<List<UsuarioModel>> UsuariosEmpresaObtener(int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosBL.UsuariosEmpresaObtener(codEmpresa, AdminView, DirGlobal);
        }


        [HttpGet("UsuariosExplorerObtener")]
        public ErrorDto<List<UsuarioModel>> UsuariosExplorerObtener(int codEmpresa)
        {
            return UsuariosBL.UsuariosExplorerObtener(codEmpresa);
        }

        [HttpGet("ExplorerRootInfoObtener")]
        public ErrorDto<ExplorerRootInfoDto> ExplorerRootInfoObtener(int codEmpresa)
        {
            return UsuariosBL.ExplorerRootInfoObtener(codEmpresa);
        }


        [HttpGet("UsuarioConsultar")]
        public ErrorDto<UsuarioModel?> UsuarioConsultar(string nombreUsuario, int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosBL.UsuarioConsultar(nombreUsuario, codEmpresa, AdminView, DirGlobal);
        }


        [HttpPost("UsuarioGuardarActualizar")]
        public ActionResult<ErrorDto> UsuarioGuardarActualizar(UsuarioModel usuarioModel)
        {
            var actor = User.FindFirst("UserName")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(actor) ||
                !string.Equals(User.FindFirst("app")?.Value, AuthApplications.SSecurity, StringComparison.OrdinalIgnoreCase))
                return Forbid();
            if (usuarioModel.EmpresaId < 0)
                return BadRequest();

            try
            {
                var permissions = new SeguridadPortalBl(_config).sbAdmin_Rols_Load(actor, usuarioModel.EmpresaId);
                if (!permissions.Admin_Portal && !(usuarioModel.EmpresaId > 0 && permissions.Rol_LocalUsers))
                    return Forbid();

                usuarioModel.UsuarioRegistro = actor.Trim();
                return Ok(UsuariosBL.UsuarioGuardarActualizar(usuarioModel,
                    permissions.Admin_Portal || permissions.Rol_AdminView,
                    permissions.Admin_Portal || permissions.Rol_DirGlobal));
            }
            catch (Exception ex)
            {
                return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "No fue posible validar los permisos para guardar el usuario.", detail: ex.GetBaseException().Message);
            }
        }

        
        [HttpGet("UsuarioClientesConsultar")]
        public ErrorDto<List<UsuarioClienteDto>> UsuarioClientesConsultar(string nombreUsuario)
        {
            return UsuariosBL.UsuarioClientesConsultar(nombreUsuario);
        }


        [HttpPost("UsuarioClienteAsignar")]
        public ErrorDto UsuarioClienteAsignar(UsuarioClienteAsignaDto usuarioClienteAsignaDto)
        {
            return UsuariosBL.UsuarioClienteAsignar(usuarioClienteAsignaDto);
        }


        [HttpGet("UsuarioCuentaTiposTransaccionObtener")]
        public ErrorDto<List<TipoTransaccionBitacora>> UsuarioCuentaTiposTransaccionObtener()
        {
            return UsuariosBL.UsuarioCuentaTiposTransaccionObtener();
        }


        [HttpPost("UsuarioBitacoraConsultar")]
        public ErrorDto<List<UsuarioCuentaBitacora>> UsuarioBitacoraConsultar(UsuarioBitacoraRequest usuarioCuentaBitacoraRequestDto)
        {
            return UsuariosBL.UsuarioBitacoraConsultar(usuarioCuentaBitacoraRequestDto);
        }


        [HttpGet("UsuarioClienteRolesConsultar")]
        public ErrorDto<List<UsuarioClienteRolDto>> UsuarioClienteRolesConsultar(string nombreUsuario, int codEmpresa)
        {
            return UsuariosBL.UsuarioClienteRolesConsultar(nombreUsuario, codEmpresa.ToString());
        }

        [HttpGet("UsuarioClienteRolesExplorerObtener")]
        public ErrorDto<List<UsuarioClienteRolDto>> UsuarioClienteRolesExplorerObtener(string nombreUsuario, int codEmpresa)
        {
            return UsuariosBL.UsuarioClienteRolesExplorerObtener(nombreUsuario, codEmpresa);
        }

        [HttpGet("RolMiembrosExplorerObtener")]
        public ErrorDto<List<RolMiembroExplorerDto>> RolMiembrosExplorerObtener(string rolId, int codEmpresa)
        {
            return UsuariosBL.RolMiembrosExplorerObtener(rolId, codEmpresa);
        }


        [HttpPost("UsuarioClienteRolAsignar")]
        public ErrorDto UsuarioClienteRolAsignar(UsuarioClienteRolAsignaDto usuarioClienteRolAsignaDto)
        {
            return UsuariosBL.UsuarioClienteRolAsignar(usuarioClienteRolAsignaDto);
        }

    }
}
