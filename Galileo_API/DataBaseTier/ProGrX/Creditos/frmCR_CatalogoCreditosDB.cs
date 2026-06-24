using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCatalogoCreditosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmCrCatalogoCreditosDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoCreditosDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }
    }
}
