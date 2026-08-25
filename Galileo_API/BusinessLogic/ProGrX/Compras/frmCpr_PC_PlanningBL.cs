using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprPCPlanningBL
    {
        private readonly FrmCprPCPlanningDB _db;

        public FrmCprPCPlanningBL(IConfiguration config)
        {
            _db = new FrmCprPCPlanningDB(config);
        }

        public ErrorDto<List<CprPlanComprasDto>> CprPlanCompras_Obtener(int CodEmpresa)
        {
            return _db.CprPlanCompras_Obtener(CodEmpresa);
        }

        public ErrorDto<CprPlanDTDto> CprPlanDT_Obtener(int CodEmpresa, int PlanCompras, string CodProducto)
        {
            return _db.CprPlanDT_Obtener(CodEmpresa, PlanCompras, CodProducto);
        }

        public ErrorDto<List<CprPlanDTCortesDto>> CprPlanDTCortes_Obtener(int CodEmpresa, int PlanCompras, string CodProducto)
        {
            return _db.CprPlanDTCortes_Obtener(CodEmpresa, PlanCompras, CodProducto);
        }

        public ErrorDto CprPlanCompras_Insert(int CodEmpresa, CprPlanComprasDto request)
        {
            return _db.CprPlanCompras_Insert(CodEmpresa, request);
        }

        public ErrorDto CprPlanCompras_Update(int CodEmpresa, CprPlanComprasDto request)
        {
            return _db.CprPlanCompras_Update(CodEmpresa, request);
        }

        public ErrorDto CprPlanDT_Upsert(int CodEmpresa, string parametros, List<CprPlanDTCortesDto> cortes)
        {
            return _db.CprPlanDT_Upsert(CodEmpresa, parametros, cortes);
        }

        public ErrorDto<CprResumenPlanLista> CprResumenPlan_Obtener(int CodEmpresa, string parametros)
        {
            return _db.CprResumenPlan_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CprPlanContableLista> CprPlanContable_Obtener(int CodEmpresa, string parametros)
        {
            return _db.CprPlanContable_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CprBitacoraLista> CprBitacora_Obtener(int CodEmpresa, string parametros)
        {
            return _db.CprBitacora_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CprResumenPlanLista> CprResumenPlan_ObtenerxCuenta(int CodEmpresa, string parametros)
        {
            return _db.CprResumenPlan_ObtenerxCuenta(CodEmpresa, parametros);
        }

        public ErrorDto<string> CprPlanCompras_PlanesEstrategicos(int CodEmpresa, int planCompras)
        {
            return _db.CprPlanCompras_PlanesEstrategicos(CodEmpresa, planCompras);
        }

        public ErrorDto<int> CprPlanCompras_AgregarSeleccion(int CodEmpresa, List<CprSeleccionDto> planEst)
        {
            return _db.CprPlanCompras_AgregarSeleccion(CodEmpresa, planEst);
        }
    }
}
