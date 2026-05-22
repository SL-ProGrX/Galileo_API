using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoRevisionesTagDB
    {
        private readonly PortalDB _portalDB;
        private const string validaSolicitud = "Debe indicar una operación válida.";

        public FrmCrSeguimientoRevisionesTagDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }
    }
}
