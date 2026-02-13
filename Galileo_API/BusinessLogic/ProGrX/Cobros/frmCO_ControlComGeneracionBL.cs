using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoControlComGeneracionBL
    {
        private readonly FrmCoControlComGeneracionDB _db;

        public FrmCoControlComGeneracionBL(IConfiguration config)
        {
            _db = new FrmCoControlComGeneracionDB(config);
        }

        public ErrorDto Co_ControlComGeneracion_Actualizar(int CodEmpresa)
        {
            return _db.Co_ControlComGeneracion_Actualizar(CodEmpresa);
        }
    }
}
