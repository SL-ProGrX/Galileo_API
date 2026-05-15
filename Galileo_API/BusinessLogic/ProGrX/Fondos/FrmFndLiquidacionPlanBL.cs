using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndLiquidacionPlanBL
    {
        private readonly FrmFndLiquidacionPlanDB _db;

        public FrmFndLiquidacionPlanBL(IConfiguration? config)
        {
            _db = new FrmFndLiquidacionPlanDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Listas(int CodEmpresa, string usuario, string catalogo)
        {
            return _db.FND_LiquidacionPlan_Listas(CodEmpresa, usuario, catalogo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_buscar(int CodEmpresa, int codOperadora)
        {
            return _db.FND_LiquidacionPlan_buscar(CodEmpresa, codOperadora);
        }

        public ErrorDto<DropDownListaGenericaModel> FND_LiquidacionPlan_Plan_Scroll_Obtener(
           int CodEmpresa,
           int codOperadora,
           string? codPlanActual,
           int scrollCode)
        {
            return _db.FND_LiquidacionPlan_Plan_Scroll_Obtener(CodEmpresa, codOperadora, codPlanActual, scrollCode);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Operadora_Obtener(int CodEmpresa)
        {
            return _db.FND_LiquidacionPlan_Operadora_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FndConsultaPlanRowDto>> FND_LiquidacionPlanContratos_Buscar(
           int CodEmpresa,
           FndLiquidacionPlanFiltrosData filtro)
        {
            return _db.FND_LiquidacionPlanContratos_Buscar(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Catalogo_Buscar(int CodEmpresa)
        {
            return _db.FND_LiquidacionPlan_Catalogo_Buscar(CodEmpresa);
        }

        public ErrorDto<FndLiquidacionPlanLiquidarResult> FND_LiquidacionPlan_Liquidar(
           int codEmpresa,
           FndLiquidacionPlanLiquidarRequest request)
        {
            return _db.FND_LiquidacionPlan_Liquidar(codEmpresa, request); 
        }

        public ErrorDto<int> FND_LiquidacionPlan_ArchivoRef_Cargar(
                int codEmpresa,
                FndArchivoRefCargaRequest request)
        {
            return _db.FND_LiquidacionPlan_ArchivoRef_Cargar(codEmpresa, request);
        }

    }
}