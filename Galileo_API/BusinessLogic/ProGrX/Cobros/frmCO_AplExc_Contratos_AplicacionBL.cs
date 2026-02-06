using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplExcContratosAplicacionBl
    {
        private readonly FrmCoAplExcContratosAplicacionDb _db;

        public FrmCoAplExcContratosAplicacionBl(IConfiguration config) => _db = new FrmCoAplExcContratosAplicacionDb(config);

        public ErrorDto<List<CoAplExcContrAplInformacionData>> CO_AplExcContrApl_Informacion_Obtener(int codEmpresa)
        {
            return _db.CO_AplExcContrApl_Informacion_Obtener(codEmpresa);
        }

        public ErrorDto<CoAplExcContrAplicadosResult> CO_AplExcContrApl_Aplicar(int CodEmpresa, ExcContratosAplicarRequest request)
        {
            return _db.CO_AplExcContrApl_Aplicar(CodEmpresa, request);
        }
    }
}
