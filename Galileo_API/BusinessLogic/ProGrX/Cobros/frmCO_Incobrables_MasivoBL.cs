
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoIncobrablesMasivoModels;


namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoIncobrablesMasivoBL
    {
        private readonly FrmCoIncobrablesMasivoDB _db;

        public FrmCoIncobrablesMasivoBL(IConfiguration config)
        {
            _db = new FrmCoIncobrablesMasivoDB(config);
        }

        public ErrorDto<CoIncobrablesMasivoCargaResponse> Co_IncobrablesMasivo_CargarArchivo(int codEmpresa, List<string> operaciones, string usuario)
                 => _db.Co_IncobrablesMasivo_CargarArchivo(codEmpresa, operaciones, usuario);
       
        public ErrorDto<bool> Co_IncobrablesMasivo_Procesar(int codEmpresa, string nota, string usuario)
              => _db.Co_IncobrablesMasivo_Procesar(codEmpresa, nota, usuario);
           
    }
}
