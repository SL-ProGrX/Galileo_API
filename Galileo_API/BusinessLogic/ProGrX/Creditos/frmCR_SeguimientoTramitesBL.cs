using Galileo_API.DataBaseTier.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Credito
{
    public class FrmCrSeguimientoTramitesBL
    {
        private readonly FrmCrSeguimientoTramitesDB _Db;

        public FrmCrSeguimientoTramitesBL(IConfiguration config)
        {
            _Db = new FrmCrSeguimientoTramitesDB(config);
        }
    }
}
