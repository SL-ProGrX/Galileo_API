using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/FrmUsCopiaAccesos")]
    [Route("api/frmUS_CopiaAccesos")]
    [ApiController]
    [Authorize]
    public class FrmUsCopiaAccesosController : ControllerBase
    {
        readonly FrmUsCopiaAccesosBl CopiaAccesosBL;

        public FrmUsCopiaAccesosController(IConfiguration config)
        {
            CopiaAccesosBL = new FrmUsCopiaAccesosBl(config);
        }

        [HttpGet("UsuariosEmpresa_Obtener")]
        public List<UsuarioEmpresa> UsuariosEmpresa_Obtener(int codEmpresa)
        {
            return CopiaAccesosBL.UsuariosEmpresa_Obtener(codEmpresa);
        }

        [HttpPost("UsuarioAccesos_Copiar")]
        public ErrorDto UsuarioAccesos_Copiar(UsuarioPermisosCopiar info)
        {
            return CopiaAccesosBL.UsuarioAccesos_Copiar(info);
        }

        [HttpGet("UsuarioEmpresa_Obtener")]
        public UsuarioEmpresa UsuarioEmpresa_Obtener(string nombreUsuario, int codEmpresa)
        {
            return CopiaAccesosBL.UsuarioEmpresa_Obtener(nombreUsuario, codEmpresa);
        }
    }
}
