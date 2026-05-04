using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaAcredoresBl
    {
        private readonly FrmPreaAcredoresDb _db;

        public FrmPreaAcredoresBl(IConfiguration config)
            => _db = new FrmPreaAcredoresDb(config);

        public ErrorDto<List<CrdPreaAcredoresData>> CrPreaAcredores_ObtenerLista(int codEmpresa)
        {
            return _db.CrPreaAcredores_ObtenerLista(codEmpresa);
        }

        public ErrorDto CrPreaAcredores_Guardar(int codEmpresa, string usuario, CrdPreaAcredoresData request)
        {
            return _db.CrPreaAcredores_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CrPreaAcredores_Borrar(int codEmpresa, string usuario, string codAcredor)
        {
            return _db.CrPreaAcredores_Borrar(codEmpresa, usuario, codAcredor);
        }
    }
}