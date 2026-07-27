using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mainDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCrSeguimientoTramitesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mainDb = new MProGrxMain(config);
            _securityMainDb = new MSecurityMainDb(config);
        }
    }
}
