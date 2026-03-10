using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizaReclamoDB
    {
        private readonly PortalDB _portalDb;

        public FrmPolizaReclamoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }
    }
}
