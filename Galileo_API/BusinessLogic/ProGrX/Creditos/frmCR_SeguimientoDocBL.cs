using Galileo.DataBaseTier.ProGrX.Credito;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.BusinessLogic.ProGrX.Credito
{
    public class FrmCRSeguimientoDocBL
    {
        private readonly FrmCRSeguimientoDocDB _db;

        public FrmCRSeguimientoDocBL(IConfiguration config)
        {
            _db = new FrmCRSeguimientoDocDB(config);
        }

        public ErrorDto CR_SeguimientoDoc_Aplicar(int CodEmpresa, FrmCRSeguimientoDocData documento)
        {
            return _db.CR_SeguimientoDoc_Aplicar(CodEmpresa, documento);
        }
    }
}