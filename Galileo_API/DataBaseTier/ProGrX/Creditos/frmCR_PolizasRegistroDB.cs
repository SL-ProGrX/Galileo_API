using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrPolizasRegistroDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSeguimientoDB _seguimientoDb;
        private readonly MCobroDb _cobroDb;
        private readonly MProGrxMain _mainDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCrPolizasRegistroDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _seguimientoDb = new MSeguimientoDB(config);
            _cobroDb = new MCobroDb(config);
            _mainDb = new MProGrxMain(config);
            _securityMainDb = new MSecurityMainDb(config);
        }
    }
}