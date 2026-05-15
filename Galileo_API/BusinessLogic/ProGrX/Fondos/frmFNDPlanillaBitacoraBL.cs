using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndPlanillaBitacoraBl
    {
        private readonly FrmFndPlanillaBitacoraDb DbFndPlanillaBitacora;

        public FrmFndPlanillaBitacoraBl(IConfiguration config)
        {
            DbFndPlanillaBitacora = new FrmFndPlanillaBitacoraDb(config);
        }

        public ErrorDto<List<FndPrmBitacoraDto>> FND_PlanillaBitacora_Obtener(int CodEmpresa, int CodInstitucion, int Proceso)
        {
            return DbFndPlanillaBitacora.FND_PlanillaBitacora_Obtener(CodEmpresa, CodInstitucion, Proceso);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaBitacora_Instituciones_Obtener(int CodEmpresa)
        {
            return DbFndPlanillaBitacora.FND_PlanillaBitacora_Instituciones_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaBitacora_Operadoras_Obtener(int CodEmpresa)
        {
            return DbFndPlanillaBitacora.FND_PlanillaBitacora_Operadoras_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaBitacora_Planes_Obtener(int CodEmpresa)
        {
            return DbFndPlanillaBitacora.FND_PlanillaBitacora_Planes_Obtener(CodEmpresa);
        }

        public ErrorDto<int> FND_PlanillaBitacora_Proceso_Obtener(int CodEmpresa, int Proceso, int Direccion)
        {
            return DbFndPlanillaBitacora.FND_PlanillaBitacora_Proceso_Obtener(CodEmpresa, Proceso, Direccion);
        }
    }
}