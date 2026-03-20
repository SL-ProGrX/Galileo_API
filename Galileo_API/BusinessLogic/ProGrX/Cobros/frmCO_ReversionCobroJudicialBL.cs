using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoInsolventesModels;
using static Galileo_API.Models.ProGrX.Cobros.FrmCOReversionCobroJudicialModels;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOReversionCobroJudicialBL
    {

        private readonly FrmCOReversionCobroJudicialDB _db;

        public FrmCOReversionCobroJudicialBL(IConfiguration config)
        {
            _db = new FrmCOReversionCobroJudicialDB(config);
        }
        public ErrorDto<CrdReversionCobroJudicialConsultaResponse> Crd_ReversionCobroJudicial_Consultar(int codEmpresa, string usuario, int codContabilidad, int operacion)
              => _db.Crd_ReversionCobroJudicial_Consultar(codEmpresa, usuario, codContabilidad, operacion);

        public ErrorDto<object> Crd_ReversionCobroJudicial_Reversar(int codEmpresa,CrdReversionCobroJudicialReversaRequest request)
             => _db.Crd_ReversionCobroJudicial_Reversar(codEmpresa, request);
    }
}
