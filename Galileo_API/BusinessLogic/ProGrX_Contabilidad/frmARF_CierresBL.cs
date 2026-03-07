using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmArfCierresBl
    {
        private readonly FrmArfCierresDb _db;

        public FrmArfCierresBl(IConfiguration config)
            => _db = new FrmArfCierresDb(config);

        public ErrorDto<ArfCierreData?> ARFCierres_CorteActual_Obtener(int codEmpresa)
        {
            return _db.ARFCierres_CorteActual_Obtener(codEmpresa);
        }

        public ErrorDto ARFCierres_Cerrar(int codEmpresa, ArfCierreData request)
        {
            return _db.ARFCierres_Cerrar(codEmpresa, request);
        }
    }
}
