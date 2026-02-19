using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.BusinessLogic.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmSysRaUsuariosController : ControllerBase
    {

        private readonly FrmSysRaUsuariosBL _bl;
        public FrmSysRaUsuariosController(IConfiguration config)
        {
            _bl = new FrmSysRaUsuariosBL(config);
        }

        [Authorize]
        [HttpGet("Sys_RA_Usuarios_Consulta")]
        public ErrorDto<List<SysUsuariosData>> Sys_RA_Usuarios_Consulta(int CodEmpresa, string filtro = "")
        {
            return _bl.Sys_RA_Usuarios_Consulta(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("Sys_RA_Usuarios_Asigna")]
        public ErrorDto Sys_RA_Usuarios_Asigna(int CodEmpresa, string ra_usuario, string usuario, bool accion)
        {
            return _bl.Sys_RA_Usuarios_Asigna(CodEmpresa, ra_usuario, usuario, accion);
        }
    }
}