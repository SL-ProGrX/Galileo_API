using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmUsCopiaAccesosController : ControllerBase
    {
        readonly FrmUsCopiaAccesosBl CopiaAccesosBL;

        public FrmUsCopiaAccesosController(IConfiguration config)
        {
            CopiaAccesosBL = new FrmUsCopiaAccesosBl(config);
        }

        [HttpGet("UsuariosEmpresa_Obtener")]
        //[Authorize]
        public List<UsuarioEmpresa> UsuariosEmpresa_Obtener(int codEmpresa)
        {
            return CopiaAccesosBL.UsuariosEmpresa_Obtener(codEmpresa);
        }

        [HttpPost("UsuarioAccesos_Copiar")]
        //[Authorize]
        public ErrorDto UsuarioAccesos_Copiar(UsuarioPermisosCopiar info)
        {
            return CopiaAccesosBL.UsuarioAccesos_Copiar(info);
        }

        [HttpGet("UsuarioEmpresa_Obtener")]
        //[Authorize]
        public UsuarioEmpresa UsuarioEmpresa_Obtener(string nombreUsuario, int codEmpresa)
        {
            return CopiaAccesosBL.UsuarioEmpresa_Obtener(nombreUsuario, codEmpresa);
        }
    }
}
