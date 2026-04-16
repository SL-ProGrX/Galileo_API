using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasRoeReimpresionBL
    {
        private readonly FrmCajasRoeReimpresionDB _db;

        public FrmCajasRoeReimpresionBL(IConfiguration config)
        {
            _db = new FrmCajasRoeReimpresionDB(config);
        }

        public ErrorDto<List<CajasRoeConsultaResult>> CajasRoe_Consulta(int codEmpresa, CajasRoeConsultaParams param)
        {
            return _db.CajasRoe_Consulta(codEmpresa, param);
        }

        public ErrorDto<CajasRoeImprimeValidaResult?> CajasRoe_Imprime_Valida(int codEmpresa, int idRoe)
        {
            return _db.CajasRoe_Imprime_Valida(codEmpresa, idRoe);
        }

        public ErrorDto<CajasRoeImprimeResult?> CajasRoe_Imprime(int codEmpresa, CajasRoeImprimeParams param)
        {
            return _db.CajasRoe_Imprime(codEmpresa, param);
        }
    }
}