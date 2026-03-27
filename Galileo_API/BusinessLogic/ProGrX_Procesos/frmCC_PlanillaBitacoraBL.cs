using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos
{
    public class FrmCCPlanillaBitacoraBL
    {
        private readonly FrmCCPlanillaBitacoraDB Db;

        public FrmCCPlanillaBitacoraBL(IConfiguration config)
        {
            Db = new FrmCCPlanillaBitacoraDB(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CC_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CC_Instituciones_Dropdown_Obtener(CodEmpresa);
        }
        public ErrorDto<CcPlanillaProcesoScrollDto> CC_PlanillaBitacora_Proceso_Scroll_Obtener(int CodEmpresa,int scrollCode,decimal procesoActual)
        {
            return Db.CC_PlanillaBitacora_Proceso_Scroll_Obtener(CodEmpresa, scrollCode, procesoActual);
        }
        public ErrorDto<CcPlanillaBitacoraListaResult> CC_PlanillaBitacora_Lista_Obtener(int CodEmpresa,decimal proceso,string parametros)
        {
            return Db.CC_PlanillaBitacora_Lista_Obtener(CodEmpresa, proceso, parametros);
        }
        public ErrorDto<CcPlanillaBitacoraListaResult> CC_PlanillaBitacora_Lista_Export(int CodEmpresa,decimal proceso,string parametros)
        {
            return Db.CC_PlanillaBitacora_Lista_Export(CodEmpresa, proceso, parametros);
        }
    }
}