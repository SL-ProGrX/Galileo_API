using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXPeriodosReversaBl
    {
        private readonly FrmCntXPeriodosReversaDb _db;

        public FrmCntXPeriodosReversaBl(IConfiguration config)
            => _db = new FrmCntXPeriodosReversaDb(config);

        public ErrorDto<List<CntXPeriodosData>> CntXPeriodos_Cierres_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXPeriodos_Cierres_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<CntXPeriodosLogData>> CntXPeriodos_Bitacora_Obtener(int codEmpresa, string request)
        {
            ReversaPeriodoRequest Jfiltros = JsonConvert.DeserializeObject<ReversaPeriodoRequest>(request) ?? new ReversaPeriodoRequest();
            return _db.CntXPeriodos_Bitacora_Obtener(codEmpresa, Jfiltros);
        }

        public ErrorDto CntXPeriodos_Reversar(int codEmpresa, ReversaPeriodoRequest request)
        {
            return _db.CntXPeriodos_Reversar(codEmpresa, request);
        }
    }
}
