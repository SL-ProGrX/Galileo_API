using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/FrmUsUsuarios")]
    [Route("api/frmUS_Usuarios")]
    [ApiController]
    public class FrmUsUsuariosController : ControllerBase
    {
        readonly FrmUsUsuariosBl UsuariosBL;

        public FrmUsUsuariosController(IConfiguration config)
        {
            UsuariosBL = new FrmUsUsuariosBl(config);
        }


        [HttpPost("UsuarioExiste")]
        //[Authorize]
        public int UsuarioExiste(string usuario)
        {
            return UsuariosBL.UsuarioExiste(usuario);
        }


        [HttpGet("UsuariosEmpresaObtener")]
        //[Authorize]
        public List<UsuarioModel> UsuariosEmpresaObtener(int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosBL.UsuariosEmpresaObtener(codEmpresa, AdminView, DirGlobal);
        }


        [HttpGet("UsuarioConsultar")]
        //[Authorize]
        public UsuarioModel UsuarioConsultar(string nombreUsuario, int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosBL.UsuarioConsultar(nombreUsuario, codEmpresa, AdminView, DirGlobal);
        }


        [HttpPost("UsuarioGuardarActualizar")]
        //[Authorize]
        public ErrorDto UsuarioGuardarActualizar(UsuarioModel usuarioModel)
        {
            return UsuariosBL.UsuarioGuardarActualizar(usuarioModel);
        }

        
        [HttpGet("UsuarioClientesConsultar")]
        //[Authorize]
        public List<UsuarioClienteDto> UsuarioClientesConsultar(string nombreUsuario)
        {
            return UsuariosBL.UsuarioClientesConsultar(nombreUsuario);
        }


        [HttpPost("UsuarioClienteAsignar")]
        //[Authorize]
        public ErrorDto UsuarioClienteAsignar(UsuarioClienteAsignaDto usuarioClienteAsignaDto)
        {
            return UsuariosBL.UsuarioClienteAsignar(usuarioClienteAsignaDto);
        }


        [HttpGet("UsuarioCuentaTiposTransaccionObtener")]
        //[Authorize]
        public List<TipoTransaccionBitacora> UsuarioCuentaTiposTransaccionObtener()
        {
            return UsuariosBL.UsuarioCuentaTiposTransaccionObtener();
        }


        [HttpPost("UsuarioBitacoraConsultar")]
        //[Authorize]
        public List<UsuarioCuentaBitacora> UsuarioBitacoraConsultar(UsuarioBitacoraRequest usuarioCuentaBitacoraRequestDto)
        {
            return UsuariosBL.UsuarioBitacoraConsultar(usuarioCuentaBitacoraRequestDto);
        }


        [HttpGet("UsuarioClienteRolesConsultar")]
        //[Authorize]
        public List<UsuarioClienteRolDto> UsuarioClienteRolesConsultar(string nombreUsuario, string codEmpresa)
        {
            return UsuariosBL.UsuarioClienteRolesConsultar(nombreUsuario, codEmpresa);
        }


        [HttpPost("UsuarioClienteRolAsignar")]
        //[Authorize]
        public ErrorDto UsuarioClienteRolAsignar(UsuarioClienteRolAsignaDto usuarioClienteRolAsignaDto)
        {
            return UsuariosBL.UsuarioClienteRolAsignar(usuarioClienteRolAsignaDto);
        }

    }
}