using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCatalogoGarantiasBl
    {
        private readonly FrmCrCatalogoGarantiasDb _db;

        public FrmCrCatalogoGarantiasBl(IConfiguration config)
            => _db = new FrmCrCatalogoGarantiasDb(config);

        public ErrorDto<List<CrGarantiaTiposData>> CrGarantiaTipos_Obtener(int codEmpresa)
        {
            return _db.CrGarantiaTipos_Obtener(codEmpresa);
        }

        public ErrorDto CrGarantiaTipos_Guardar(int codEmpresa, string usuario, CrGarantiaTiposData request)
        {
            return _db.CrGarantiaTipos_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CrGarantiaTipos_Eliminar(int codEmpresa, string garantia, string usuario)
        {
            return _db.CrGarantiaTipos_Eliminar(codEmpresa, garantia, usuario);
        }
    }
}
