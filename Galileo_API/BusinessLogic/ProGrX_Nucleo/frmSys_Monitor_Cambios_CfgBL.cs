using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Nucleo;
using Galileo_API.Models.ProGrX_Nucleo;

namespace Galileo_API.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysMonitorCambiosCfgBL
    {
        private readonly FrmSysMonitorCambiosCfgDB _DB;
        public FrmSysMonitorCambiosCfgBL(IConfiguration config)
        {
            _DB = new FrmSysMonitorCambiosCfgDB(config);
        }

        public ErrorDto<string> Sys_GetNomCortoEmpresa_Obtener(int CodEmpresa)
        {
            return _DB.Sys_GetNomCortoEmpresa_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Sys_MonitorCambiosCfg_Modulos_Obtener(int CodEmpresa)
        {
            return _DB.Sys_MonitorCambiosCfg_Modulos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Sys_MonitorCambiosCfg_Tablas_Obtener(int CodEmpresa, object filtros)
        {
            return _DB.Sys_MonitorCambiosCfg_Tablas_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<object>> Sys_MonitorCambiosCfg_Bitacora_Obtener(int CodEmpresa, string  filtros)
        {
            MonitorCambiosCfgFiltros filtro = DbHelper.DeserializeOrNew<MonitorCambiosCfgFiltros>(filtros);
            return _DB.Sys_MonitorCambiosCfg_Bitacora_Obtener(CodEmpresa, filtro);
        }

    }
}
