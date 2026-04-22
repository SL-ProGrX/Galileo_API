
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoProcesosMasivoModels;


namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoCobroJudicialMasivoBL
    {
        private readonly FrmCoProcesosMasivoDB _db;

        public FrmCoCobroJudicialMasivoBL(IConfiguration config)
        {
            _db = new FrmCoProcesosMasivoDB(config);
        }

        public ErrorDto<CoProcesosMasivoCargaResponse> Co_CobroJudicialMasivo_CargarOperaciones(int codEmpresa, List<string> operaciones, string usuario, string modulo)
                 => _db.Co_ProcesosMasivo_CargarArchivo(codEmpresa, operaciones, usuario, modulo);
       
        public ErrorDto<bool> Co_CobroJudicialMasivo_Procesar(int codEmpresa, string nota, string usuario, string modulo)
              => _db.Co_ProcesosMasivo_Procesar(codEmpresa, nota, usuario, modulo); 

    }
}
