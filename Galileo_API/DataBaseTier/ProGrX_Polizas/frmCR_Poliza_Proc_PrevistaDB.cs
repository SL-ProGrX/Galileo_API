using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizaProcPrevistaDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrPolizaProcPrevistaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }
    }
}
