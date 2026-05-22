using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito.Galileo_API.Models.ProGrX.Credito;
using System.Data.Common;

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
