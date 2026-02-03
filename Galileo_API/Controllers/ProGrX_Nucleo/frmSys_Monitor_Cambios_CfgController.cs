using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Nucleo;
using Galileo_API.Models.ProGrX_Nucleo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmSysMonitorCambiosCfgController : ControllerBase
    {
       private readonly FrmSysMonitorCambiosCfgBL _BL;
       public FrmSysMonitorCambiosCfgController(IConfiguration config)
       {
              _BL = new FrmSysMonitorCambiosCfgBL(config);
       }

        [HttpGet("Sys_GetNomCortoEmpresa_Obtener")]
        public ErrorDto<string> Sys_GetNomCortoEmpresa_Obtener(int CodEmpresa)
        {
            return _BL.Sys_GetNomCortoEmpresa_Obtener(CodEmpresa);
        }
        [HttpGet("Sys_MonitorCambiosCfg_Modulos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_MonitorCambiosCfg_Modulos_Obtener(int CodEmpresa)
        {
            return _BL.Sys_MonitorCambiosCfg_Modulos_Obtener(CodEmpresa);
        }
        [HttpGet("Sys_MonitorCambiosCfg_Tablas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_MonitorCambiosCfg_Tablas_Obtener(int CodEmpresa, object filtros)
        {
            return _BL.Sys_MonitorCambiosCfg_Tablas_Obtener(CodEmpresa, filtros);
        }
        [HttpGet("Sys_MonitorCambiosCfg_Bitacora_Obtener")]
        public ErrorDto<List<object>> Sys_MonitorCambiosCfg_Bitacora_Obtener(int CodEmpresa, string filtros)
        {
            return _BL.Sys_MonitorCambiosCfg_Bitacora_Obtener(CodEmpresa, filtros);
        }

    }
}
