using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.DataBaseTier.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndFondosAplCreditosBl
    {
        private readonly FrmFndFondosAplCreditosDb _db;

        public FrmFndFondosAplCreditosBl(IConfiguration config)
        {
            _db = new FrmFndFondosAplCreditosDb(config);
        }

        public ErrorDto<List<FndFondosAplCreditosPlanModel>> FondosAplCreditos_Planes_Obtener(int codOperadora, int codEmpresa, string orderBy)
        {
            return _db.FondosAplCreditos_Planes_Obtener(codOperadora, codEmpresa, orderBy);
        }

        public ErrorDto<List<FndFondosAplCreditosListaResult>> FondosAplCreditos_Lista(FndFondosAplCreditosListaParams param)
        {
            return _db.FondosAplCreditos_Lista(param);
        }

        public ErrorDto<FndFondosAplCreditosAplicacionGeneralResult> FondosAplCreditos_AplicacionGeneral(FndFondosAplCreditosAplicacionGeneralParams param)
        {
            return _db.FondosAplCreditos_AplicacionGeneral(param);
        }

        public ErrorDto<FndFondosAplCreditosAplicacionResult> FondosAplCreditos_Aplicacion(FndFondosAplCreditosAplicacionParams param, int codEmpresa)
        {
            return _db.FondosAplCreditos_Aplicacion(param, codEmpresa);
        }

        public ErrorDto<List<FndFondosAplCreditosResumenResult>> FondosAplCreditos_Resumen_Obtener(int codEmpresa)
        {
            return _db.FondosAplCreditos_Resumen_Obtener(codEmpresa);
        }
    }
}