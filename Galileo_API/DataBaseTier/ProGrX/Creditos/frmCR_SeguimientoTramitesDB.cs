using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSeguimientoTramitesDB
    {
        private readonly PortalDB _portalDb;
        public FrmCrSeguimientoTramitesDB(IConfiguration? config)
        {
           _portalDb = new PortalDB(config);   
        }
    }
}
