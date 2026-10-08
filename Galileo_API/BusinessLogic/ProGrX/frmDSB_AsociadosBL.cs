using Galileo.DataBaseTier.ProGrX;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;

namespace Galileo.BusinessLogic.ProGrX
{
    public class FrmDsbAsociadosBL
    {
        private readonly FrmDsbAsociadosDB _db;

        public FrmDsbAsociadosBL(IConfiguration config)
        {
            _db = new FrmDsbAsociadosDB(config);
        }

        public ErrorDto<DashboardAsociadosData> Asociado_Obtener(
            int codEmpresa,
            string cedula,
            string usuario)
            => _db.Asociado_Obtener(codEmpresa, cedula, usuario);
    }
}
