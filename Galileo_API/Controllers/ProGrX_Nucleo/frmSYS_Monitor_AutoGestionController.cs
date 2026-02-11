using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo_API.BusinessLogic.ProGrX_Nucleo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo.Models.ProGrX_Nucleo.FrmSysMonitorAutoGestionModels;

namespace Galileo_API.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmSYS_Monitor_AutoGestionController : ControllerBase
    {
        private readonly frmSYS_Monitor_AutoGestionBL BL;

        public frmSYS_Monitor_AutoGestionController(IConfiguration config)
        {
            BL = new frmSYS_Monitor_AutoGestionBL(config);
        }
        [Authorize]
        [HttpGet("Sys_Monitor_AutoGestion_Personas_DropDown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Monitor_AutoGestion_Personas_DropDown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.Sys_Monitor_AutoGestion_Personas_DropDown_Obtener(CodEmpresa, filtro);
        }
        [Authorize]
        [HttpGet("Sys_Monitor_AutoGestion_Creditos_DropDown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Monitor_AutoGestion_Creditos_DropDown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.Sys_Monitor_AutoGestion_Creditos_DropDown_Obtener(CodEmpresa, filtro);
        }
        [Authorize]
        [HttpGet("Sys_Monitor_AutoGestion_Lista_Obtener")]
        public ErrorDto<MonitorAutoGestionLista> Sys_Monitor_AutoGestion_Lista_Obtener(int CodEmpresa, string jfiltros, MonitorAutoGestionBuscarRequest req)
        {
            return BL.Sys_Monitor_AutoGestion_Lista_Obtener(CodEmpresa, jfiltros, req);
        }

        [Authorize]
        [HttpGet("Sys_Monitor_AutoGestion_Lista_Export")]
        public ErrorDto<MonitorAutoGestionLista> Sys_Monitor_AutoGestion_Lista_Export(int CodEmpresa, string jfiltros, MonitorAutoGestionBuscarRequest req)
        {
            return BL.Sys_Monitor_AutoGestion_Lista_Export(CodEmpresa, jfiltros, req);
        }
        [Authorize]
        [HttpGet("Sys_Monitor_AutoGestion_Caso_Obtener")]
        public ErrorDto<MonitorAutoGestionCasoDetalle> Sys_Monitor_AutoGestion_Caso_Obtener(int CodEmpresa, long cod_solicitud)
        {
            return BL.Sys_Monitor_AutoGestion_Caso_Obtener(CodEmpresa, cod_solicitud);
        }
        [Authorize]
        [HttpGet("Sys_Monitor_AutoGestion_Resumen_Obtener")]
        public ErrorDto<MonitorAutoGestionResumenLista> Sys_Monitor_AutoGestion_Resumen_Obtener(int CodEmpresa, DateTime fechaInicio, DateTime fechaFin)
        {
            return BL.Sys_Monitor_AutoGestion_Resumen_Obtener(CodEmpresa, fechaInicio, fechaFin);
        }
        [Authorize]
        [HttpGet("Sys_Monitor_AutoGestion_Adjuntos_Obtener")]
        public ErrorDto<MonitorAutoGestionAdjuntosLista> Sys_Monitor_AutoGestion_Adjuntos_Obtener(int CodEmpresa, long cod_solicitud)
        {
            return BL.Sys_Monitor_AutoGestion_Adjuntos_Obtener(CodEmpresa, cod_solicitud);
        }
        [Authorize]
        [HttpGet("Sys_Monitor_AutoGestion_Adjunto_Descargar")]
        public ErrorDto<(byte[] buffer, string nombre, string tipo)> Sys_Monitor_AutoGestion_Adjunto_Descargar(int CodEmpresa, long archivo_id)
        {
            return BL.Sys_Monitor_AutoGestion_Adjunto_Descargar(CodEmpresa, archivo_id);
        }
        [Authorize]
        [HttpPost]
        [HttpPost("Sys_Monitor_AutoGestion_Resolucion_Aplicar")]
        public ErrorDto<MonitorAutoGestionResolucionResponse> Sys_Monitor_AutoGestion_Resolucion_Aplicar(int CodEmpresa, MonitorAutoGestionResolucionRequest dto)
        {
            return BL.Sys_Monitor_AutoGestion_Resolucion_Aplicar(CodEmpresa, dto);
        }
        [Authorize]
        [HttpPost]
        [HttpPost("Sys_Monitor_AutoGestion_Adjuntos_Fix")]
        public ErrorDto Sys_Monitor_AutoGestion_Adjuntos_Fix(int CodEmpresa)
        {
            return BL.Sys_Monitor_AutoGestion_Adjuntos_Fix(CodEmpresa);
        }
    }
}
