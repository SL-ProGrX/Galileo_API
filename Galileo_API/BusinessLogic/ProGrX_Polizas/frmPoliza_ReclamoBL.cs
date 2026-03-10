using Galileo_API.DataBaseTier.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizaReclamoBL
    {
        private readonly FrmPolizaReclamoDB _db;
    
        public FrmPolizaReclamoBL(IConfiguration config)
        {
           _db = new FrmPolizaReclamoDB(config);
        }
    }
}
