using Galileo_API.DataBaseTier.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizaProcPrevistaBL
    {
        private readonly FrmCrPolizaProcPrevistaDB _db;

        public FrmCrPolizaProcPrevistaBL(IConfiguration config)
        {
            _db = new FrmCrPolizaProcPrevistaDB(config);
        }
    }
}
