using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplExcProcesaBl
    {
        private readonly FrmCoAplExcProcesaDb _db;

        public FrmCoAplExcProcesaBl(IConfiguration config) => _db = new FrmCoAplExcProcesaDb(config);

        public ErrorDto<List<CoAplExcProcInformacionData>> CO_AplExcProc_Informacion_Obtener(int codEmpresa)
        {
            return _db.CO_AplExcProc_Informacion_Obtener(codEmpresa);
        }

        public ErrorDto<CoAplExcProcesadosResult> CO_AplExc_Procesa_Aplicar(int CodEmpresa, ExcedenteAplicarRequest request)
        {
            return _db.CO_AplExc_Procesa_Aplicar(CodEmpresa, request);
        }
    }
}
