using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    public class FrmCrConveniosParametrosBl
    {
        private readonly FrmCrConveniosParametrosDb _db;

        public FrmCrConveniosParametrosBl(IConfiguration config)
        {
            _db = new FrmCrConveniosParametrosDb(config);
        }

        public ErrorDto<List<CrConvenioParametroData>> CrConveniosParametros_Obtener(int codEmpresa, int codContabilidad)
            => _db.CrConveniosParametros_Obtener(codEmpresa, codContabilidad);

        public ErrorDto<CrConvenioParametroData> CrConveniosParametros_Actualizar(
            int codEmpresa,
            int codContabilidad,
            CrConvenioParametroActualizarRequest request)
            => _db.CrConveniosParametros_Actualizar(codEmpresa, codContabilidad, request);
    }
}
