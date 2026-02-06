using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplFndContratosAplicacionBl
    {
        private readonly FrmCoAplFndContratosAplicacionDb _db;

        public FrmCoAplFndContratosAplicacionBl(IConfiguration config) => _db = new FrmCoAplFndContratosAplicacionDb(config);

        public ErrorDto<List<CoAplFndContrAplInformacionData>> CO_AplFndContrApl_Informacion_Obtener(int codEmpresa)
        {
            return _db.CO_AplFndContrApl_Informacion_Obtener(codEmpresa);
        }

        public ErrorDto<CoAplExcProcesadosResult> CO_AplFndContrApl_Aplicar(int CodEmpresa, ExcedenteAplicarRequest request)
        {
            return _db.CO_AplFndContrApl_Aplicar(CodEmpresa, request);
        }
    }
}
