using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.DataBaseTier.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndConciliacionTeledolarSinpeBl
    {
        private readonly FrmFndConciliacionTeledolarSinpeDb _db;

        public FrmFndConciliacionTeledolarSinpeBl(IConfiguration config)
        {
            _db = new FrmFndConciliacionTeledolarSinpeDb(config);
        }

        public ErrorDto<List<FndConciliacionTeledolarSinpeResult>> ConciliacionTeledolarSinpe_Obtener(FndConciliacionTeledolarSinpeParams param)
        {
            return _db.ConciliacionTeledolarSinpe_Obtener(param);
        }
    }
}