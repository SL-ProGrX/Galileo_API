using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysOrigenRecursosController : ControllerBase
    {
        private readonly FrmSysOrigenRecursosBL _bl;
        public FrmSysOrigenRecursosController(IConfiguration config)
        {
            _bl = new FrmSysOrigenRecursosBL(config);
        }

        [Authorize]
        [HttpGet("Sys_OrigenRecursosLista_Obtener")]
        public ErrorDto<SysOrigenRecursosLista> Tes_UbicacionesLista_Obtener(int CodEmpresa)
        {
            return _bl.Sys_OrigenRecursosLista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Sys_OrigenRecursos_Guardar")]
        public ErrorDto  Sys_OrigenRecursos_Guardar(int codEmpresa,SysOrigenRecursosData OrigenRecursos)
        { 
            return _bl.Sys_OrigenRecursos_Guardar(codEmpresa, OrigenRecursos);
        }

        [Authorize]
        [HttpDelete("Sys_OrigenRecursos_Eliminar")]
        public ErrorDto Sys_OrigenRecursos_Eliminar(int codEmpresa, string usuario, string OrigenRecursos)
        {
            return _bl.Sys_OrigenRecursos_Eliminar(codEmpresa, usuario, OrigenRecursos);
        }

    }
}