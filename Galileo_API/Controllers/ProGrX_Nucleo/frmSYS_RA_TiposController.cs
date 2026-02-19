using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmSysRaTiposController : ControllerBase
    {
        private readonly FrmSysRaTiposBL _bl;
        public FrmSysRaTiposController(IConfiguration config)
        {
            _bl = new FrmSysRaTiposBL(config);
        }

        [Authorize]
        [HttpGet("Sys_RaTiposLista_Obtener")]
        public ErrorDto<SysRaTiposLista> Sys_RaTiposLista_Obtener(int CodEmpresa, string filtro)
        {
            return _bl.Sys_RaTiposLista_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("Sys_RaTipos_Guardar")]
        public ErrorDto Sys_RaTipos_Guardar(int CodEmpresa, string usuario, SysRaTiposData tipo)
        {
            return _bl.Sys_RaTipos_Guardar(CodEmpresa, usuario, tipo);
        }

        [Authorize]
        [HttpDelete("Sys_RaTipos_Eliminar")]
        public ErrorDto Sys_RaTipos_Eliminar(int CodEmpresa, string usuario, string tipo_id)
        {
            return _bl.Sys_RaTipos_Eliminar(CodEmpresa, usuario, tipo_id);
        }
 
    }
}