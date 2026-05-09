using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaTablaImpRentaBl
    {
        private readonly FrmPreaTablaImpRentaDb _db;

        public FrmPreaTablaImpRentaBl(IConfiguration config)
            => _db = new FrmPreaTablaImpRentaDb(config);

        public ErrorDto<List<CrdPreaTablaImpRentaData>> CrPreaTablaImpRenta_Obtener(int codEmpresa)
        {
            return _db.CrPreaTablaImpRenta_Obtener(codEmpresa);
        }

        public ErrorDto CrPreaTablaImpRenta_Guardar(int codEmpresa, string usuario, CrdPreaTablaImpRentaData request)
        {
            return _db.CrPreaTablaImpRenta_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CrPreaTablaImpRenta_Eliminar(int codEmpresa, int idx, string usuario)
        {
            return _db.CrPreaTablaImpRenta_Eliminar(codEmpresa, idx, usuario);
        }
    }
}
