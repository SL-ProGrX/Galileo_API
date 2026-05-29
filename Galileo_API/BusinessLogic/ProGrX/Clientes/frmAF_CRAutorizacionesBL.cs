using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfCrAutorizacionesBL
    {
        private readonly FrmAFCrAutorizacionesDB _db;

        public FrmAfCrAutorizacionesBL(IConfiguration config)
        {
            _db = new FrmAFCrAutorizacionesDB(config);
        }

        public ErrorDto<List<AfCrAutorizacion>> AF_CRAutorizaciones_Obtener(int CodEmpresa, AfCrAutorizacionFiltros filtros)
        {
            return _db.AF_CRAutorizaciones_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_CRAutorizaciones_Autorizar(int CodEmpresa, int CodRenuncia, string Observaciones, int pAutoriza, string Usuario)
        {
            return _db.AF_CRAutorizaciones_Autorizar(CodEmpresa, CodRenuncia, Observaciones, pAutoriza, Usuario);
        }
    }
}