using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvTransacReportesBL
    {
        private readonly FrmInvTransacReportesDB _db;

        public FrmInvTransacReportesBL(IConfiguration config)
        {
            _db = new FrmInvTransacReportesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_TransacReportes_Causas_Obtener(int CodEmpresa, string tipo)
        {
            return _db.INV_TransacReportes_Causas_Obtener(CodEmpresa, tipo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_TransacReportes_Usuarios_Obtener(int CodEmpresa)
        {
            return _db.INV_TransacReportes_Usuarios_Obtener(CodEmpresa);
        }
    }
}
