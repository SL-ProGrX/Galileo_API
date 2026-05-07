using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using static Galileo_API.Models.ProGrX_EstudioCrd.FrmPreaParametrosModels;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaParametrosBL
    {
        private readonly FrmPreaParametrosDB _db;

        public FrmPreaParametrosBL(IConfiguration config)
            => _db = new FrmPreaParametrosDB(config);
        public ErrorDto<List<PreaParametroModel>> PreaParametros_Inicializar(int codEmpresa)
               => _db.PreaParametros_Inicializar(codEmpresa);
        public ErrorDto<List<PreaParametroModel>> PreaParametros_Grid_Obtener(int codEmpresa)
                  => _db.PreaParametros_Grid_Obtener(codEmpresa);
        public ErrorDto<List<PreaParametroHistoricoModel>> PreaParametros_Historico_Obtener(int codEmpresa, string codParametro)
                 => _db.PreaParametros_Historico_Obtener(codEmpresa, codParametro);
        public ErrorDto PreaParametros_Parametro_Actualizar(int codEmpresa, PreaParametroActualizarRequest request)
                 => _db.PreaParametros_Parametro_Actualizar(codEmpresa, request);

    }
}
