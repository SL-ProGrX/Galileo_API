using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFPlanMutualBL
    {
        private readonly FrmAFPlanMutualDB _db;

        public FrmAFPlanMutualBL(IConfiguration config)
        {
            _db = new FrmAFPlanMutualDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_PlanMutualLista_Obtener(int CodEmpresa)
        {
            return _db.AF_PlanMutualLista_Obtener(CodEmpresa);
        }

        public ErrorDto<AfPlanPersonaslLista> AF_PlanMutualPersonas_Obtener(int CodEmpresa, string plan, string estado, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_PlanMutualPersonas_Obtener(CodEmpresa, plan, estado, filtros);
        }

        public ErrorDto<AfPlanMutualLista> AF_PlanMutual_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_PlanMutual_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<AfPlanMutualPersonasData>> AF_PlanMutualPersonas_Exportar(int CodEmpresa, string plan, string estado, int total)
        {
             return _db.AF_PlanMutualPersonas_Exportar(CodEmpresa, plan, estado, total);
        }

        public ErrorDto AF_PlanMutualPersona_Guardar(int CodEmpresa, string plan, string usuario, AfPlanMutualPersonasData persona)
        {
            return _db.AF_PlanMutualPersona_Guardar(CodEmpresa, plan, usuario, persona);
        }

        public ErrorDto AF_PlanMutual_Guardar(int CodEmpresa, string usuario, AfPlanMutualDto plan)
        {
            return _db.AF_PlanMutual_Guardar(CodEmpresa, usuario, plan);
        }

        public ErrorDto AF_PlanMutual_Eliminar(int CodEmpresa, string usuario, string plan)
        {
            return _db.AF_PlanMutual_Eliminar(CodEmpresa, usuario, plan);
        }

        public ErrorDto AF_PlanMutual_Actualizar(int CodEmpresa, string usuario, string plan)
        {
            return _db.AF_PlanMutual_Actualizar(CodEmpresa, usuario, plan);
        }
    }
}
