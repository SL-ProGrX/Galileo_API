using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvEtiquetasBL
    {
        private readonly FrmInvEtiquetasDB _db;

        public FrmInvEtiquetasBL(IConfiguration config)
        {
            _db = new FrmInvEtiquetasDB(config);
        }

        public ErrorDto<List<ProductData>> GenerateSato(int CodEmpresa, GenerateSatoRequest request)
        {
            return _db.GenerateSato(CodEmpresa, request);
        }
    }
}