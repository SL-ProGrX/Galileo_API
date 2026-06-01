using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCausasSeguimientoBl
    {
        private readonly FrmCrCausasSeguimientoDb _db;

        public FrmCrCausasSeguimientoBl(IConfiguration config)
        {
            _db = new FrmCrCausasSeguimientoDb(config);
        }

        public ErrorDto<List<CrCausasSeguimientoData>> CrCausasSeguimiento_Causas_Obtener(
            int codEmpresa, string tipo)
            => _db.CrCausasSeguimiento_Causas_Obtener(codEmpresa, tipo);

        public ErrorDto CrCausasSeguimiento_Causas_Guardar(
            int codEmpresa,
            CrCausasSeguimientoGuardarRequest request)
            => _db.CrCausasSeguimiento_Causas_Guardar(codEmpresa, request);

        public ErrorDto CrCausasSeguimiento_Causas_Eliminar(
            int codEmpresa,
            CrCausasSeguimientoEliminarRequest request)
            => _db.CrCausasSeguimiento_Causas_Eliminar(codEmpresa, request);
    }
}