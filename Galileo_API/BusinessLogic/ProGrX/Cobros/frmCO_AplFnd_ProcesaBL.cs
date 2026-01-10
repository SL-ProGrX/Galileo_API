using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplFndProcesaBl
    {
        private readonly FrmCoAplFndProcesaDb _db;

        public FrmCoAplFndProcesaBl(IConfiguration config) => _db = new FrmCoAplFndProcesaDb(config);

        public ErrorDto<List<CoAplFndProcInformacionData>> CO_AplFndProc_Informacion_Obtener(int codEmpresa)
        {
            return _db.CO_AplFndProc_Informacion_Obtener(codEmpresa);
        }

        public ErrorDto<CoAplFndProcesadosResult> CO_AplFnd_Procesa_Aplicar(int CodEmpresa, FondosAplicarRequest request)
        {
            return _db.CO_AplFnd_Procesa_Aplicar(CodEmpresa, request);
        }
    }
}
