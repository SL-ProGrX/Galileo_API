using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysEducacionBitacoraController : ControllerBase
    {
        private readonly FrmSysEducacionBitacoraBL _bl;

        public FrmSysEducacionBitacoraController(IConfiguration config)
        {
            _bl = new FrmSysEducacionBitacoraBL(config);
        }

        [Authorize]
        [HttpGet("SYS_Educacion_Combo_Obtener")]
        public ActionResult<ErrorDto<List<SysEducacionListData>>> SYS_Educacion_Combo_Obtener(int CodEmpresa, string tipo, string? valor = null)
        {
            return _bl.SYS_Educacion_Combo_Obtener(CodEmpresa, tipo, valor ?? string.Empty);
        }

        [Authorize]
        [HttpGet("SYS_Padron_Obtener")]
        public ActionResult<ErrorDto<SysPadronLista>> SYS_Padron_Obtener(int CodEmpresa, string filtro)
        {
            return _bl.SYS_Padron_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("SYS_Educacion_Obtener")]
        public ActionResult<ErrorDto<List<SysEducacionLogData>>> SYS_Educacion_Obtener(int CodEmpresa, string filtro)
        {
            return _bl.SYS_Educacion_Obtener(CodEmpresa, filtro);
        }
    }
}