using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXProcesosAddBl
    {
        private readonly FrmCntXProcesosAddDb _db;

        public FrmCntXProcesosAddBl(IConfiguration config) => _db = new FrmCntXProcesosAddDb(config);

        public ErrorDto<List<CtnXProcesosAddDto>> CntXProcesosAdd_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXProcesosAdd_Obtener(codEmpresa, codConta);
        }

        public ErrorDto CntXProcesosAdd_Procesar(int codEmpresa, CntXProcesarRequest req)
        {
            return _db.CntXProcesosAdd_Procesar(codEmpresa, req);
        }
    }
}
