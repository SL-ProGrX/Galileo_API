using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysEstadoCivilController : ControllerBase
    {
        private readonly FrmSysEstadoCivilBL _bl;
        public FrmSysEstadoCivilController(IConfiguration config)
        {
            _bl = new FrmSysEstadoCivilBL(config);
        }

        [Authorize]
        [HttpGet("Sys_EstadoCivilLista_Obtener")]
        public ErrorDto<SysEstadoCivilLista> Tes_UbicacionesLista_Obtener(int CodEmpresa)
        {
            return _bl.Sys_EstadoCivilLista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Sys_EstadoCivil_Guardar")]
        public ErrorDto  Sys_EstadoCivil_Guardar(int codEmpresa,SysEstadoCivilData estadoCivil)
        { 
            return _bl.Sys_EstadoCivil_Guardar(codEmpresa, estadoCivil);
        }

        [Authorize]
        [HttpDelete("Sys_EstadoCivil_Eliminar")]
        public ErrorDto Sys_EstadoCivil_Eliminar(int codEmpresa, string usuario, string estadoCivil)
        {
            return _bl.Sys_EstadoCivil_Eliminar(codEmpresa, usuario, estadoCivil);
        }

    }
}