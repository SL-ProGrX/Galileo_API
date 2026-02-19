using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.SYS;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysNacionalidadesController : ControllerBase
    {
        private readonly FrmSysNacionalidadesBL _bl;

        public FrmSysNacionalidadesController(IConfiguration config)
        {
            _bl = new FrmSysNacionalidadesBL(config);
        }

        [Authorize]
        [HttpGet("Sys_NacionalidadesLista_Obtener")]
        public ErrorDto<SysNacionalidadesLista> Sys_NacionalidadesLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Sys_NacionalidadesLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Sys_Nacionalidades_Obtener")]
        public ActionResult<ErrorDto<List<SysNacionalidadesData>>> Sys_Nacionalidades_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Sys_Nacionalidades_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Sys_Nacionalidades_Guardar")]
        public ErrorDto Sys_Nacionalidades_Guardar(int CodEmpresa, string usuario, [FromBody] SysNacionalidadesData nacionalidad)
        {
            return _bl.Sys_Nacionalidades_Guardar(CodEmpresa, usuario, nacionalidad);
        }

        [Authorize]
        [HttpPost("Sys_Nacionalidades_Valida")]
        public ErrorDto Sys_Nacionalidades_Valida(int CodEmpresa, [FromBody] SysNacionalidadesData nacionalidad)
        {
            return _bl.Sys_Nacionalidades_Valida(CodEmpresa, nacionalidad);
        }

        [Authorize]
        [HttpDelete("Sys_Nacionalidades_Eliminar")]
        public ErrorDto Sys_Nacionalidades_Eliminar(int CodEmpresa, string usuario, string codNacionalidad)
        {
            return _bl.Sys_Nacionalidades_Eliminar(CodEmpresa, usuario, codNacionalidad);
        }
       
    }
}