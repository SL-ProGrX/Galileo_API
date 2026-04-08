using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasSgtRebajosInternosBL
    {
        private readonly FrmCxCCuentasSgtRebajosInternosDB Db;

        public FrmCxCCuentasSgtRebajosInternosBL(IConfiguration config)
        {
            Db = new FrmCxCCuentasSgtRebajosInternosDB(config);
        }

        public ErrorDto<CxCCuentasSgtRebajosInternosPantallaDto> CxC_Cuentas_SGT_Rebajos_Operacion_Obtener(
            int CodEmpresa,
            int Operacion)
            => Db.CxC_Cuentas_SGT_Rebajos_Operacion_Obtener(CodEmpresa, Operacion);

        public ErrorDto<List<CxCCuentaRebajoInternoDto>> CxC_Cuentas_SGT_Rebajos_Terceros_Obtener(
            int CodEmpresa,
            string Cedula)
            => Db.CxC_Cuentas_SGT_Rebajos_Terceros_Obtener(CodEmpresa, Cedula);

        public ErrorDto<decimal> CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener(
            int CodEmpresa,
            int Operacion)
            => Db.CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener(CodEmpresa, Operacion);

        public ErrorDto<bool> CxC_Cuentas_SGT_Rebajos_Existe_Obtener(
            int CodEmpresa,
            int Operacion,
            int Operacion_Aplicada)
            => Db.CxC_Cuentas_SGT_Rebajos_Existe_Obtener(CodEmpresa, Operacion, Operacion_Aplicada);

        public ErrorDto CxC_Cuentas_SGT_Rebajos_Guardar(
            int CodEmpresa,
            string Usuario,
            int Contabilidad,
            CxCCuentasSgtRebajosInternosGuardarDto req)
            => Db.CxC_Cuentas_SGT_Rebajos_Guardar(CodEmpresa, Usuario, Contabilidad, req);

        public ErrorDto CxC_Cuentas_SGT_Rebajos_Eliminar(
            int CodEmpresa,
            CxCCuentasSgtRebajosInternosEliminarDto req)
            => Db.CxC_Cuentas_SGT_Rebajos_Eliminar(CodEmpresa, req);
    }
}
