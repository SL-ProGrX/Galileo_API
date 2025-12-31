using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/FrmUsUsuarios")]
    [Route("api/frmUS_Usuarios")]
    [ApiController]
    [Authorize]
    public class FrmUsUsuariosController : ControllerBase
    {
        readonly FrmUsUsuariosBl UsuariosBL;

        public FrmUsUsuariosController(IConfiguration config)
        {
            UsuariosBL = new FrmUsUsuariosBl(config);
        }


        [HttpPost("UsuarioExiste")]
        public int UsuarioExiste(string usuario)
        {
            return UsuariosBL.UsuarioExiste(usuario);
        }


        [HttpGet("UsuariosEmpresaObtener")]
        public List<UsuarioModel> UsuariosEmpresaObtener(int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosBL.UsuariosEmpresaObtener(codEmpresa, AdminView, DirGlobal);
        }


        [HttpGet("UsuarioConsultar")]
        public UsuarioModel UsuarioConsultar(string nombreUsuario, int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosBL.UsuarioConsultar(nombreUsuario, codEmpresa, AdminView, DirGlobal);
        }


        [HttpPost("UsuarioGuardarActualizar")]
        public ErrorDto UsuarioGuardarActualizar(UsuarioModel usuarioModel)
        {
            return UsuariosBL.UsuarioGuardarActualizar(usuarioModel);
        }

        
        [HttpGet("UsuarioClientesConsultar")]
        public List<UsuarioClienteDto> UsuarioClientesConsultar(string nombreUsuario)
        {
            return UsuariosBL.UsuarioClientesConsultar(nombreUsuario);
        }


        [HttpPost("UsuarioClienteAsignar")]
        public ErrorDto UsuarioClienteAsignar(UsuarioClienteAsignaDto usuarioClienteAsignaDto)
        {
            return UsuariosBL.UsuarioClienteAsignar(usuarioClienteAsignaDto);
        }


        [HttpGet("UsuarioCuentaTiposTransaccionObtener")]
        public List<TipoTransaccionBitacora> UsuarioCuentaTiposTransaccionObtener()
        {
            return UsuariosBL.UsuarioCuentaTiposTransaccionObtener();
        }


        [HttpPost("UsuarioBitacoraConsultar")]
        public List<UsuarioCuentaBitacora> UsuarioBitacoraConsultar(UsuarioBitacoraRequest usuarioCuentaBitacoraRequestDto)
        {
            return UsuariosBL.UsuarioBitacoraConsultar(usuarioCuentaBitacoraRequestDto);
        }


        [HttpGet("UsuarioClienteRolesConsultar")]
        public List<UsuarioClienteRolDto> UsuarioClienteRolesConsultar(string nombreUsuario, string codEmpresa)
        {
            return UsuariosBL.UsuarioClienteRolesConsultar(nombreUsuario, codEmpresa);
        }


        [HttpPost("UsuarioClienteRolAsignar")]
        public ErrorDto UsuarioClienteRolAsignar(UsuarioClienteRolAsignaDto usuarioClienteRolAsignaDto)
        {
            return UsuariosBL.UsuarioClienteRolAsignar(usuarioClienteRolAsignaDto);
        }

    }
}