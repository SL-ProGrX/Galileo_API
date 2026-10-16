using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    /// <summary>
    /// Acceso a datos de frmCR_ConveniosOrdenAsiento (asiento contable de la orden de liquidación).
    /// </summary>
    public class FrmCrConveniosOrdenAsientoDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrConveniosOrdenAsientoDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Equivalente a exec spConvenios_Orden_Asiento del Form_Load/TimerX_Timer del VB6.
        /// </summary>
        public ErrorDto<List<CrOrdenAsientoLinea>> CrConveniosOrdenAsiento_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            return DbHelper.ExecuteListQuery<CrOrdenAsientoLinea>(
                _portalDb,
                codEmpresa,
                "exec spConvenios_Orden_Asiento @CodConvenio, @CodOrden",
                new { CodConvenio = codConvenio.Trim(), CodOrden = codOrden });
        }
    }
}
