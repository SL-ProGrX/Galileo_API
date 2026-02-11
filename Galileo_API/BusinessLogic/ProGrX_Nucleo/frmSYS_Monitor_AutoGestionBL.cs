using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo_API.DataBaseTier.ProGrX_Nucleo;
using static Galileo.Models.ProGrX_Nucleo.FrmSysMonitorAutoGestionModels;

namespace Galileo_API.BusinessLogic.ProGrX_Nucleo
{
    public class frmSYS_Monitor_AutoGestionBL
    {
        private readonly frmSYS_Monitor_AutoGestionDB Db;

        public frmSYS_Monitor_AutoGestionBL(IConfiguration config)
        {
            Db = new frmSYS_Monitor_AutoGestionDB(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Monitor_AutoGestion_Personas_DropDown_Obtener(int CodEmpresa, string? filtro)
        {
            return Db.Sys_Monitor_AutoGestion_Personas_DropDown_Obtener(CodEmpresa, filtro);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Monitor_AutoGestion_Creditos_DropDown_Obtener(int CodEmpresa, string? filtro)
        {
            return Db.Sys_Monitor_AutoGestion_Creditos_DropDown_Obtener(CodEmpresa, filtro);
        }
        public ErrorDto<MonitorAutoGestionLista> Sys_Monitor_AutoGestion_Lista_Obtener(int CodEmpresa, string jfiltros, MonitorAutoGestionBuscarRequest req)
        {
            return Db.Sys_Monitor_AutoGestion_Lista_Obtener(CodEmpresa, jfiltros, req);
        }
        public ErrorDto<MonitorAutoGestionLista> Sys_Monitor_AutoGestion_Lista_Export(int CodEmpresa, string jfiltros, MonitorAutoGestionBuscarRequest req)
        {
            return Db.Sys_Monitor_AutoGestion_Lista_Export(CodEmpresa, jfiltros, req);
        }
        public ErrorDto<MonitorAutoGestionCasoDetalle> Sys_Monitor_AutoGestion_Caso_Obtener(int CodEmpresa, long cod_solicitud)
        {
            return Db.Sys_Monitor_AutoGestion_Caso_Obtener(CodEmpresa, cod_solicitud);
        }
        public ErrorDto<MonitorAutoGestionResumenLista> Sys_Monitor_AutoGestion_Resumen_Obtener(int CodEmpresa, DateTime fechaInicio, DateTime fechaFin)
        {
            return Db.Sys_Monitor_AutoGestion_Resumen_Obtener(CodEmpresa, fechaInicio, fechaFin);
        }
        public ErrorDto<MonitorAutoGestionAdjuntosLista> Sys_Monitor_AutoGestion_Adjuntos_Obtener(int CodEmpresa, long cod_solicitud)
        {
            return Db.Sys_Monitor_AutoGestion_Adjuntos_Obtener(CodEmpresa, cod_solicitud);
        }
        public ErrorDto<(byte[] buffer, string nombre, string tipo)> Sys_Monitor_AutoGestion_Adjunto_Descargar(int CodEmpresa, long archivo_id)
        {
            return Db.Sys_Monitor_AutoGestion_Adjunto_Descargar(CodEmpresa, archivo_id);
        }
        public ErrorDto<MonitorAutoGestionResolucionResponse> Sys_Monitor_AutoGestion_Resolucion_Aplicar(int CodEmpresa, MonitorAutoGestionResolucionRequest dto)
        {
            return Db.Sys_Monitor_AutoGestion_Resolucion_Aplicar(CodEmpresa, dto);
        }
        public ErrorDto Sys_Monitor_AutoGestion_Adjuntos_Fix(int CodEmpresa)
        {
            return Db.Sys_Monitor_AutoGestion_Adjuntos_Fix(CodEmpresa);
        }
    }
}
