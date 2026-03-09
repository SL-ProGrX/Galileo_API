using Galileo.Models;
using Galileo.Models.ERROR;
using PgxAPI.DataBaseTier.ProGrX.Fondos;
using PgxAPI.Models.ProGrX.Fondos;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace PgxAPI.BusinessLogic.ProGrX.Fondos
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

        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Operadora_Obtener(int CodEmpresa)
        {
            return _db.FND_LiquidacionPlan_Operadora_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FndConsultaPlanRowDto>> FND_LiquidacionPlanContartos_Buscar(
           int CodEmpresa,
           FndLiquidacionPlanFiltrosData filtro)
        {
            return _db.FND_LiquidacionPlanContartos_Buscar(CodEmpresa, filtro);
        }

       

    }
}
