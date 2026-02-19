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
    public class FrmSysRaPersonasController : ControllerBase
    {
        private readonly FrmSysRaPersonasBL _bl;
        public FrmSysRaPersonasController(IConfiguration config)
        {
            _bl = new FrmSysRaPersonasBL(config);
        }


        [Authorize]
        [HttpGet("SYS_RA_Personas_Buscar")]
        public ErrorDto<List<SysRaExpedientesData>> SYS_RA_Personas_Buscar(int CodEmpresa, [FromQuery] SysExpedienteFiltroData filtro)
        {
            return _bl.SYS_RA_Personas_Buscar(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("SYS_RA_Personas_Guardar")]
        public ErrorDto SYS_RA_Personas_Guardar(int CodEmpresa, int personaId, SysRaExpedientesData datos, string usuario)
        {
            return _bl.SYS_RA_Personas_Guardar(CodEmpresa, personaId, datos, usuario);
        }

        [Authorize]
        [HttpGet("SYS_Usuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> SYS_Usuarios_Obtener(int CodEmpresa)
        {
            return _bl.SYS_Usuarios_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SYS_RaTipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> SYS_RaTipos_Obtener(int CodEmpresa)
        {
            return _bl.SYS_RaTipos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SYS_RA_CasosPorCedula_Obtener")]
        public ErrorDto<List<SysAutorizacionesData>> SYS_RA_CasosPorCedula_Obtener(int CodEmpresa, string filtro="")
        {
            return _bl.SYS_RA_CasosPorCedula_Obtener(CodEmpresa, filtro);
        }

    }
}