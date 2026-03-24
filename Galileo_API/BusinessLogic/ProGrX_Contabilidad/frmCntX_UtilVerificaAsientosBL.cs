using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXUtilVerificaAsientosBl
    {
        private readonly FrmCntXUtilVerificaAsientosDb _db;

        public FrmCntXUtilVerificaAsientosBl(IConfiguration config) 
            => _db = new FrmCntXUtilVerificaAsientosDb(config);

        public ErrorDto CntXAsientos_Verificar(int codEmpresa, CntXAsientosVerificarRequest request)
        {
            return _db.CntXAsientos_Verificar(codEmpresa, request);
        }
    }
}
