using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
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
        public ErrorDto<int> UsuarioExiste(string usuario)
        {
            return UsuariosBL.UsuarioExiste(usuario);
        }


        [HttpGet("UsuariosEmpresaObtener")]
        public ErrorDto<List<UsuarioModel>> UsuariosEmpresaObtener(int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosBL.UsuariosEmpresaObtener(codEmpresa, AdminView, DirGlobal);
        }


        [HttpGet("UsuarioConsultar")]
        public ErrorDto<UsuarioModel?> UsuarioConsultar(string nombreUsuario, int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosBL.UsuarioConsultar(nombreUsuario, codEmpresa, AdminView, DirGlobal);
        }


        [HttpPost("UsuarioGuardarActualizar")]
        public ErrorDto UsuarioGuardarActualizar(UsuarioModel usuarioModel)
        {
            return UsuariosBL.UsuarioGuardarActualizar(usuarioModel);
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


        [HttpPost("UsuarioClienteRolAsignar")]
        public ErrorDto UsuarioClienteRolAsignar(UsuarioClienteRolAsignaDto usuarioClienteRolAsignaDto)
        {
            return UsuariosBL.UsuarioClienteRolAsignar(usuarioClienteRolAsignaDto);
        }

    }
}
