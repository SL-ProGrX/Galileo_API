using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysRaAutorizacionesController : ControllerBase
    {
        private readonly FrmSysRaAutorizacionesBL _bl;
        public FrmSysRaAutorizacionesController(IConfiguration config)
        {
            _bl = new FrmSysRaAutorizacionesBL(config);
        }

        [Authorize]
        [HttpGet("SYS_RA_AutorizacionesUsuariosAutorizados_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> SYS_RA_AutorizacionesUsuariosAutorizados_Obtener(int CodEmpresa)
        {
            return _bl.SYS_RA_AutorizacionesUsuariosAutorizados_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SYS_RA_AutorizacionesCasos_Obtener")]
        public ErrorDto<List<SysAutorizacionesData>> SYS_RA_AutorizacionesCasos_Obtener(int CodEmpresa)
        {
            return _bl.SYS_RA_AutorizacionesCasos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SYS_RA_AutorizacionesCasosDatos_Obtener")]
        public ErrorDto<SysAutorizacionesData> SYS_RA_AutorizacionesCasosDatos_Obtener(int CodEmpresa, int persona_id)
        {
            return _bl.SYS_RA_AutorizacionesCasosDatos_Obtener(CodEmpresa, persona_id);
        }

        [Authorize]
        [HttpPost("SYS_RA_Autorizaciones_Autorizar")]
        public ErrorDto SYS_RA_Autorizaciones_Autorizar(int CodEmpresa, string usuario, SysAutorizacionesData datos, string clave)
        {
            return _bl.SYS_RA_Autorizaciones_Autorizar(CodEmpresa, usuario, datos, clave);
        }

    }
}