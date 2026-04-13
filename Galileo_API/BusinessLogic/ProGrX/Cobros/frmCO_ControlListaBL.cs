using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOControlListaBL
    {
        private readonly FrmCOControlListaDB _db;

        public FrmCOControlListaBL(IConfiguration config)
        {
            _db = new FrmCOControlListaDB(config);
        }

        public ErrorDto<CoControlListaBuscarResponse> CoControlLista_Buscar(
            int codEmpresa,
            CoControlListaBuscarRequest request)
        {
            return _db.CoControlLista_Buscar(codEmpresa, request);
        }
    }
}
