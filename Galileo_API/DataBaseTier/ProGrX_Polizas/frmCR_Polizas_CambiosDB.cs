using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizasCambiosDB
    {
        private readonly PortalDB _portalDB;

        public FrmCrPolizasCambiosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }
    }
}
