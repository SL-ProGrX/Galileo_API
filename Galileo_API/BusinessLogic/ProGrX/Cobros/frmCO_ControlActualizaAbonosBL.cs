using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Microsoft.Data.SqlClient;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoControlActualizaAbonosBL
    {
        private readonly FrmCoControlActualizaAbonosDB _db;

        public FrmCoControlActualizaAbonosBL(IConfiguration config)
        {
            _db = new FrmCoControlActualizaAbonosDB(config);
        }

        public ErrorDto Co_ControlActualizaAbonos_Actualizar(int CodEmpresa)
        {
            return _db.Co_ControlActualizaAbonos_Actualizar(CodEmpresa);
        }
    }
}
