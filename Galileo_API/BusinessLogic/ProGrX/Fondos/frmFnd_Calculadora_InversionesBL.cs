using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndCalculadoraInversionesBL
    {
        private readonly FrmFndCalculadoraInversionesDB _Db;

        public FrmFndCalculadoraInversionesBL(IConfiguration config)
        {
            _Db = new FrmFndCalculadoraInversionesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Calculadora_Planes_Obtener(int CodEmpresa, int TipoInv)
        {
            return _Db.Fnd_Calculadora_Planes_Obtener(CodEmpresa, TipoInv);
        }

        public ErrorDto<FndCalculadoraPlanes> Fnd_Calculadora_ConsultaPlan_Obtener(int CodEmpresa, string CodPlan)
        {
            return _Db.Fnd_Calculadora_ConsultaPlan_Obtener(CodEmpresa, CodPlan);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Calculadora_PlazosInv_Obtener(int CodEmpresa, string CodPlan)
        {
            return _Db.Fnd_Calculadora_PlazosInv_Obtener(CodEmpresa, CodPlan);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Calculadora_Cupones_Obtener(int CodEmpresa, int Plazo, string CodPlan)
        {
            return _Db.Fnd_Calculadora_Cupones_Obtener(CodEmpresa, Plazo, CodPlan);
        }

        public ErrorDto<int> Fnd_Calculadora_PlazosDias_Obtener(int CodEmpresa, int Plazo, string CodPlan)
        {
            return _Db.Fnd_Calculadora_PlazosDias_Obtener(CodEmpresa, Plazo, CodPlan);
        }

        public ErrorDto<decimal> Fnd_Calculadora_TasaRef_Obtener(int CodEmpresa, int PlazoDias, string Tipo, string Plan, int Operadora, bool chkCupon, int rpTipo, int PlazoInv, int? CuponId)
        {
            return _Db.Fnd_Calculadora_TasaRef_Obtener(CodEmpresa, PlazoDias, Tipo, Plan, Operadora, chkCupon, rpTipo, PlazoInv, CuponId);
        }

        public ErrorDto<List<FndCalculadoraInversionesFlujoData>> Fnd_Calculadora_Inversiones_Calcular(int CodEmpresa, string FiltrosCalculadora)
        {
            return _Db.Fnd_Calculadora_Inversiones_Calcular(CodEmpresa, FiltrosCalculadora);
        }

        public ErrorDto Fnd_Calculadora_Inversiones_EmailEnviar(int CodEmpresa, int CalculoId, string Usuario)
        {
            return _Db.Fnd_Calculadora_Inversiones_EmailEnviar(CodEmpresa, CalculoId, Usuario);
        }
    }
}