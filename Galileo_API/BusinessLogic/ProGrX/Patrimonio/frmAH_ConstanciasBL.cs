using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAHConstanciasBL
    {
        private readonly FrmAHConstanciasDB _db;

        public FrmAHConstanciasBL(IConfiguration config)
        {
            _db = new FrmAHConstanciasDB(config);
        }

        public ErrorDto<FrmAhConstanciasConsultaResponse?> Patrimonio_frmAH_Constancias_Consulta_Obtener(
            int codEmpresa,
            string cedula,
            string usuario)
            => _db.Patrimonio_frmAH_Constancias_Consulta_Obtener(codEmpresa, cedula, usuario);
    }
}
