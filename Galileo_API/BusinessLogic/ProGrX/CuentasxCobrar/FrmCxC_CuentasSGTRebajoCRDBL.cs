using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasSgtRebajoCrdBL
    {
        private readonly FrmCxCCuentasSgtRebajoCrdDB Db;

        public FrmCxCCuentasSgtRebajoCrdBL(IConfiguration config)
        {
            Db = new FrmCxCCuentasSgtRebajoCrdDB(config);
        }

        /// <summary>
        /// Obtiene la carga inicial de la pantalla de rebajos a créditos.
        /// </summary>
        public ErrorDto<CxCCuentasSgtRebajoCrdPantallaDto> CxC_Cuentas_SGT_Rebajo_CRD_Operacion_Obtener(
                int CodEmpresa,
                int Operacion,
                int Cta_Pendientes)
    => Db.CxC_Cuentas_SGT_Rebajo_CRD_Operacion_Obtener(CodEmpresa, Operacion, Cta_Pendientes);

        /// <summary>
        /// Obtiene los créditos de terceros por cédula.
        /// </summary>
        public ErrorDto<List<CxCCuentaRebajoCrdDto>> CxC_Cuentas_SGT_Rebajo_CRD_Terceros_Obtener(
            int CodEmpresa,
            string Cedula,
            int Cta_Pendientes)
            => Db.CxC_Cuentas_SGT_Rebajo_CRD_Terceros_Obtener(CodEmpresa, Cedula, Cta_Pendientes);

        /// <summary>
        /// Valida si ya existe un rebajo registrado para la solicitud indicada.
        /// </summary>
        public ErrorDto<bool> CxC_Cuentas_SGT_Rebajo_CRD_Existe_Obtener(
            int CodEmpresa,
            int Operacion,
            int Id_Solicitud)
            => Db.CxC_Cuentas_SGT_Rebajo_CRD_Existe_Obtener(CodEmpresa, Operacion, Id_Solicitud);

        /// <summary>
        /// Guarda un rebajo a crédito.
        /// </summary>
        public ErrorDto CxC_Cuentas_SGT_Rebajo_CRD_Guardar(
            int CodEmpresa,
            CxCCuentasSgtRebajoCrdGuardarDto req)
            => Db.CxC_Cuentas_SGT_Rebajo_CRD_Guardar(CodEmpresa, req);

        /// <summary>
        /// Elimina un rebajo a crédito registrado.
        /// </summary>
        public ErrorDto CxC_Cuentas_SGT_Rebajo_CRD_Eliminar(
            int CodEmpresa,
            CxCCuentasSgtRebajoCrdEliminarDto req)
            => Db.CxC_Cuentas_SGT_Rebajo_CRD_Eliminar(CodEmpresa, req);

        /// <summary>
        /// Ejecuta la actualización de créditos para la operación.
        /// </summary>
        public ErrorDto CxC_Cuentas_SGT_Rebajo_CRD_Actualizar(
            int CodEmpresa,
            CxCCuentasSgtRebajoCrdActualizarDto req)
            => Db.CxC_Cuentas_SGT_Rebajo_CRD_Actualizar(CodEmpresa, req);
    }
}
