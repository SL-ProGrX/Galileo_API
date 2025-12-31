using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndActualizacionSifBl
    {
        private readonly FrmFndActualizacionSifDb _db;

        public FrmFndActualizacionSifBl(IConfiguration config)
        {
            _db = new FrmFndActualizacionSifDb(config);
        }

        public ErrorDto Fnd_ActualizacionSif_Aplicar(int CodEmpresa, string Usuario)
        {
            return _db.Fnd_ActualizacionSif_Aplicar(CodEmpresa, Usuario);
        }
    }
}
