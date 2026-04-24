
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoProcesosMasivoModels;


namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoCobroJudicialReversaMasivoBL
    {
        private readonly FrmCoProcesosMasivoDB _db;

        public FrmCoCobroJudicialReversaMasivoBL(IConfiguration config)
        {
            _db = new FrmCoProcesosMasivoDB(config);
        }

        public ErrorDto<CoProcesosMasivoCargaResponse> Co_CobroJudicialRevMasivo_CargarOperaciones(int codEmpresa, List<string> operaciones, string usuario, string modulo)
                 => _db.Co_ProcesosMasivo_CargarArchivo(codEmpresa, operaciones, usuario, modulo);
       
        public ErrorDto<bool> Co_CobroJudicialRevMasivo_Procesar(int codEmpresa, string nota, string usuario, string modulo)
              => _db.Co_ProcesosMasivo_Procesar(codEmpresa, nota, usuario, modulo); 

    }
}
