using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndPlanillaFondosBl
    {
        private readonly FrmFndPlanillaFondosDb DbFndPlanillaFondos;

        public FrmFndPlanillaFondosBl(IConfiguration config)
        {
            DbFndPlanillaFondos = new FrmFndPlanillaFondosDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaFondos_Instituciones_Obtener(int CodEmpresa)
        {
            return DbFndPlanillaFondos.FND_PlanillaFondos_Instituciones_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaFondos_Operadoras_Obtener(int CodEmpresa)
        {
            return DbFndPlanillaFondos.FND_PlanillaFondos_Operadoras_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_PlanillaFondos_Planes_Obtener(int CodEmpresa, int CodOperadora)
        {
            return DbFndPlanillaFondos.FND_PlanillaFondos_Planes_Obtener(CodEmpresa, CodOperadora);
        }

        public ErrorDto<string> FND_PlanillaFondos_Comprobante_Obtener(int CodEmpresa, int CodInstitucion, int Proceso)
        {
            return DbFndPlanillaFondos.FND_PlanillaFondos_Comprobante_Obtener(CodEmpresa, CodInstitucion, Proceso);
        }

        public ErrorDto<DropDownListaGenericaModel> FND_PlanillaFondos_Cuenta_Obtener(int CodEmpresa, string Tipo, int CodInstitucion, int CodOperadora, string CodPlan, int CodConta)
        {
            return DbFndPlanillaFondos.FND_PlanillaFondos_Cuenta_Obtener(CodEmpresa, Tipo, CodInstitucion, CodOperadora, CodPlan, CodConta);
        }

        public ErrorDto<List<int>> FND_PlanillaFondos_Procesos_ObtenerRango(int CodEmpresa, int Proceso)
        {
            return DbFndPlanillaFondos.FND_PlanillaFondos_Procesos_ObtenerRango(CodEmpresa, Proceso);
        }

        public ErrorDto<FndPlanillaFondosData> FND_PlanillaFondos_Deducciones_Cargar(int CodEmpresa, string Request)
        {
            CargarDeduccionesRequest request = JsonConvert.DeserializeObject<CargarDeduccionesRequest>(Request) ?? new CargarDeduccionesRequest();
            return DbFndPlanillaFondos.FND_PlanillaFondos_Deducciones_Cargar(CodEmpresa, request);
        }

        public ErrorDto<object> FND_PlanillaFondos_Procesar(int CodEmpresa, string Request)
        {
            FndPlanillaDirectaProcesaDto request = JsonConvert.DeserializeObject<FndPlanillaDirectaProcesaDto>(Request) ?? new FndPlanillaDirectaProcesaDto();
            return DbFndPlanillaFondos.FND_PlanillaFondos_Procesar(CodEmpresa, request);
        }
    }
}