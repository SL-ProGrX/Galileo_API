
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoCobroJudicialMasivoModels;


namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoCobroJudicialMasivoBL
    {
        private readonly FrmCoCobroJudicialMasivoDB _db;

        public FrmCoCobroJudicialMasivoBL(IConfiguration config)
        {
            _db = new FrmCoCobroJudicialMasivoDB(config);
        }

        public ErrorDto<CoCobroJudicialMasivoCargaResponse> Co_CobroJudicialMasivo_CargarOperaciones(int codEmpresa, List<string> operaciones, string usuario)
                 => _db.Co_CobroJudicialMasivo_CargarOperaciones(codEmpresa, operaciones, usuario);
       
        public ErrorDto<bool> Co_CobroJudicialMasivo_Procesar(int codEmpresa, string nota, string usuario)
              => _db.Co_CobroJudicialMasivo_Procesar(codEmpresa, nota, usuario);


    }
}
