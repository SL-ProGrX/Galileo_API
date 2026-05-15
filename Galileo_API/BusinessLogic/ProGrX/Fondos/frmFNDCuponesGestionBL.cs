using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.DataBaseTier.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndCuponesGestionBl
    {
        private readonly FrmFndCuponesGestionDb _db;

        public FrmFndCuponesGestionBl(IConfiguration config)
        {
            _db = new FrmFndCuponesGestionDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FndCuponesGestion_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            return _db.FndCuponesGestion_Bancos_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FndCuponesGestion_Conceptos_Obtener(int CodEmpresa)
        {
            return _db.FndCuponesGestion_Conceptos_Obtener(CodEmpresa);
        }

        public ErrorDto<FndCuponesGestionPlanExisteResult> FndCuponesGestion_PlanExiste(int CodEmpresa)
        {
            return _db.FndCuponesGestion_PlanExiste(CodEmpresa);
        }

        public ErrorDto<List<FndCuponesGestionVencimientoResult>> FndCuponesGestion_ConsultaVencimiento(FndCuponesGestionVencimientoParams param)
        {
            return _db.FndCuponesGestion_ConsultaVencimiento(param);
        }

        public ErrorDto<FndCuponesGestionLiquidaResult> FndCuponesGestion_Liquida(FndCuponesGestionLiquidaParams param)
        {
            return _db.FndCuponesGestion_Liquida(param);
        }
    }
}