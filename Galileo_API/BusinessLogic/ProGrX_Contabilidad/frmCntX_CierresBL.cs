using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXCierresBl
    {
        private readonly FrmCntXCierresDb _db;

        public FrmCntXCierresBl(IConfiguration config) => _db = new FrmCntXCierresDb(config);

        public ErrorDto<List<CntXCierreData>> CntXCierres_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXCierres_Obtener(codEmpresa, codConta);
        }

        public ErrorDto CntXCierres_Guardar(int codEmpresa, int codConta, string usuario, CntXCierreData request)
        {
            return _db.CntXCierres_Guardar(codEmpresa, codConta, usuario, request);
        }

        public ErrorDto CntXCierres_Eliminar(int codEmpresa, int codConta, string usuario, string idCierre)
        {
            return _db.CntXCierres_Eliminar(codEmpresa, codConta, usuario, idCierre);
        }
    }
}
