using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCPlanPagosBl
    {
        private readonly FrmCxCPlanPagosDb _db;

        public FrmCxCPlanPagosBl(IConfiguration config) => _db = new FrmCxCPlanPagosDb(config);

        public ErrorDto<CxCPlanPagosOperacionData> CxCPlanPagos_Operacion_Obtener(int codEmpresa, int operacionId)
        {
            return _db.CxCPlanPagos_Operacion_Obtener(codEmpresa, operacionId);
        }

        public ErrorDto<List<CxCPlanPagosMovimientoData>> CxCPlanPagos_Movimientos_Obtener(int codEmpresa, int operacionId)
        {
            return _db.CxCPlanPagos_Movimientos_Obtener(codEmpresa, operacionId);
        }

        public ErrorDto<CxCPlanPagosOperacionResumenData> CxCPlanPagos_ResumenOperacion_Obtener(int codEmpresa, int operacionId)
        {
            return _db.CxCPlanPagos_ResumenOperacion_Obtener(codEmpresa, operacionId);
        }
    }
}
