using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrAnulaAbonosNewBl
    {
        private readonly FrmCrAnulaAbonosNewDb _db;

        public FrmCrAnulaAbonosNewBl(IConfiguration config)
            => _db = new FrmCrAnulaAbonosNewDb(config);

        public ErrorDto<CrAnulaAbonosNewConsultaData> CrAnulaAbonosNew_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            return _db.CrAnulaAbonosNew_Operacion_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<string> CrAnulaAbonosNew_CuentaRecomendada_Obtener(
            int codEmpresa,
            CrAnulaAbonosNewCuentaRecomendadaRequest request)
        {
            return _db.CrAnulaAbonosNew_CuentaRecomendada_Obtener(codEmpresa, request);
        }

        public ErrorDto<CrAnulaAbonosNewAplicarResultadoData> CrAnulaAbonosNew_Aplicar(
            int codEmpresa,
            CrAnulaAbonosNewAplicarRequest request)
        {
            return _db.CrAnulaAbonosNew_Aplicar(codEmpresa, request);
        }
    }
}