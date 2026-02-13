using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Nucleo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo.Models.ProGrX_Nucleo.FrmSysMonitorAutoGestionModels;

namespace Galileo_API.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysMonitorAutoGestionController : ControllerBase
    {
        private readonly FrmSysMonitorAutoGestionBl BL;

        public FrmSysMonitorAutoGestionController(IConfiguration config)
        {
            BL = new FrmSysMonitorAutoGestionBl(config);
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
        public ErrorDto<MonitorAutoGestionLista> Sys_Monitor_AutoGestion_Lista_Obtener([FromQuery] int CodEmpresa,[FromQuery] string jfiltros,[FromQuery] MonitorAutoGestionBuscarRequest req)
        {
            return BL.Sys_Monitor_AutoGestion_Lista_Obtener(CodEmpresa, jfiltros, req);
        }

        [Authorize]
        [HttpGet("Sys_Monitor_AutoGestion_Lista_Export")]
        public ErrorDto<MonitorAutoGestionLista> Sys_Monitor_AutoGestion_Lista_Export([FromQuery] int CodEmpresa,[FromQuery] string jfiltros,[FromQuery] MonitorAutoGestionBuscarRequest req)
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
        public ErrorDto<MonitorAutoGestionResumenLista> Sys_Monitor_AutoGestion_Resumen_Obtener(int CodEmpresa,string fechaInicio,string fechaFin)
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
        public IActionResult Sys_Monitor_AutoGestion_Adjunto_Descargar(int CodEmpresa, long archivo_id)
        {
            var r = BL.Sys_Monitor_AutoGestion_Adjunto_Descargar(CodEmpresa, archivo_id);
            if (r.Code != 0 || r.Result.buffer == null || r.Result.buffer.Length == 0)
                return BadRequest(r);

            var nombre = (r.Result.nombre ?? "adjunto").Trim();
            var tipo = string.IsNullOrWhiteSpace(r.Result.tipo) ? "application/octet-stream" : r.Result.tipo.Trim();
            return File(r.Result.buffer, tipo, nombre);
        }

        [Authorize]
        [HttpPost("Sys_Monitor_AutoGestion_Resolucion_Aplicar")]
        public ErrorDto<MonitorAutoGestionResolucionResponse> Sys_Monitor_AutoGestion_Resolucion_Aplicar(int CodEmpresa, MonitorAutoGestionResolucionRequest dto)
        {
            return BL.Sys_Monitor_AutoGestion_Resolucion_Aplicar(CodEmpresa, dto);
        }
        [Authorize]
        [HttpPost("Sys_Monitor_AutoGestion_Adjuntos_Fix")]
        public ErrorDto Sys_Monitor_AutoGestion_Adjuntos_Fix(int CodEmpresa)
        {
            return BL.Sys_Monitor_AutoGestion_Adjuntos_Fix(CodEmpresa);
        }
    }
}
