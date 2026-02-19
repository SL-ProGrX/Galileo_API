using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysRaCasosController : ControllerBase
    {
        private readonly FrmSysRaCasosBL _bl;
        public FrmSysRaCasosController(IConfiguration config)
        {
            _bl = new FrmSysRaCasosBL(config);
        }


        [Authorize]
        [HttpGet("SYS_RA_Casos_Buscar")]
        public ErrorDto<List<SysRaCasosData>> SYS_RA_Casos_Buscar(int CodEmpresa, [FromQuery] SysCasosFiltroData filtros)
        {
            return _bl.SYS_RA_Casos_Buscar(CodEmpresa, filtros);
        }
         
        [Authorize]
        [HttpGet("SYS_RA_CasosAutorizaciones_Obtener")]
        public ErrorDto<List<SysCasosAutorizacionesData>> SYS_RA_CasosAutorizaciones_Obtener(int CodEmpresa, int persona_id)
        {
            return _bl.SYS_RA_CasosAutorizaciones_Obtener(CodEmpresa, persona_id);
        }
        
        [Authorize]
        [HttpGet("SYS_RA_CasosAccesos_Obtener")]
        public ErrorDto<List<SysCasosAccesosData>> SYS_RA_CasosAccesos_Obtener(int CodEmpresa, int autorizacionId)
        {
            return _bl.SYS_RA_CasosAccesos_Obtener(CodEmpresa, autorizacionId);
        }
 
    }
}
