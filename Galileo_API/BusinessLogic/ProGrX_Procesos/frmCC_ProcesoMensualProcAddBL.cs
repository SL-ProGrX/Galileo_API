using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Procesos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;

namespace Galileo.BusinessLogic.ProGrX_Procesos
{
    public class FrmCCProcesoMensualProcAddBL
    {
        private readonly FrmCCProcesoMensualProcAddDB _db;

        public FrmCCProcesoMensualProcAddBL(IConfiguration config)
        {
            _db = new FrmCCProcesoMensualProcAddDB(config);
        }

        public ErrorDto<CcPlanillaProcesosComplementariosLista> CC_PlanillaProcesosComplementarios_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CC_PlanillaProcesosComplementarios_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<CcPlanillaProcesosComplementariosData>> CC_PlanillaProcesosComplementarios_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CC_PlanillaProcesosComplementarios_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto CC_PlanillaProcesosComplementarios_Guardar(int CodEmpresa, string usuario, CcPlanillaProcesosComplementariosData data)
        {
            return _db.CC_PlanillaProcesosComplementarios_Guardar(CodEmpresa, usuario, data);
        }

        public ErrorDto CC_PlanillaProcesosComplementarios_Eliminar(int CodEmpresa, string transaccion, int proc_num, string ejecucion_tipo, string usuario)
        {
            return _db.CC_PlanillaProcesosComplementarios_Eliminar(CodEmpresa, transaccion, proc_num, ejecucion_tipo, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaProcesosComplementarios_Transacciones_Obtener(int CodEmpresa)
        {
            return _db.CC_PlanillaProcesosComplementarios_Transacciones_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaProcesosComplementarios_TiposEjecucion_Obtener(int CodEmpresa)
        {
            return _db.CC_PlanillaProcesosComplementarios_TiposEjecucion_Obtener(CodEmpresa);
        }
    }
}