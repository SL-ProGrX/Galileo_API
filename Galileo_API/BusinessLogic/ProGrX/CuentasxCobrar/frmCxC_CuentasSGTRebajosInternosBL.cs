using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasSGTRebajosInternosBL
    {
        private readonly FrmCxCCuentasSGTRebajosInternosDB Db;

        public FrmCxCCuentasSGTRebajosInternosBL(IConfiguration config)
        {
            Db = new FrmCxCCuentasSGTRebajosInternosDB(config);
        }

        public ErrorDto<CxCCuentasSGTRebajosInternosPantallaDto> CxC_Cuentas_SGT_Rebajos_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            return Db.CxC_Cuentas_SGT_Rebajos_Operacion_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<decimal> CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener(
    int codEmpresa,
    int operacion)
        {
            return Db.CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener(codEmpresa, operacion);
        }
    }
}
