using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysParentescosController : ControllerBase
    {
        private readonly FrmSysParentescosBL _bl;
        public FrmSysParentescosController(IConfiguration config)
        {
            _bl = new FrmSysParentescosBL(config);
        }

        [Authorize]
        [HttpGet("Sys_ParentescosLista_Obtener")]
        public ErrorDto<SysParentescosLista> Sys_ParentescosLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.SYS_ParentescosLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Sys_Parentescos_Obtener")]
        public ErrorDto<List<SysParentescosData>> Sys_Parentescos_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.SYS_Parentescos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Sys_Parentescos_Guardar")]
        public ErrorDto Sys_Parentescos_Guardar(int CodEmpresa, string usuario, SysParentescosData parentesco)
        {
            return _bl.SYS_Parentescos_Guardar(CodEmpresa, usuario, parentesco);
        }

        [Authorize]
        [HttpDelete("Sys_Parentescos_Eliminar")]
        public ErrorDto Sys_Parentescos_Eliminar(int CodEmpresa, string parentesco, string usuario)
        {
            return _bl.SYS_Parentescos_Eliminar(CodEmpresa, parentesco, usuario);
        }

        [Authorize]
        [HttpGet("Sys_Parentescos_Valida")]
        public ErrorDto Sys_Parentescos_Valida(int CodEmpresa, string cod_parentesco)
        {
            return _bl.SYS_Parentescos_Valida(CodEmpresa, cod_parentesco);
        }
    }
}