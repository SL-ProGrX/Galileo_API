
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoProcesosMasivoModels;


namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoIncobrablesMasivoBL
    {
        private readonly FrmCoProcesosMasivoDB _db;

        public FrmCoIncobrablesMasivoBL(IConfiguration config)
        {
            _db = new FrmCoProcesosMasivoDB(config);
        }

        public ErrorDto<CoProcesosMasivoCargaResponse> Co_IncobrablesMasivo_CargarArchivo(int codEmpresa, List<string> operaciones, string usuario, string modulo)
                 => _db.Co_ProcesosMasivo_CargarArchivo(codEmpresa, operaciones, usuario, modulo);
       
        public ErrorDto<bool> Co_IncobrablesMasivo_Procesar(int codEmpresa, string nota, string usuario, string modulo)
              => _db.Co_ProcesosMasivo_Procesar(codEmpresa, nota, usuario, modulo);
           
    }
}
