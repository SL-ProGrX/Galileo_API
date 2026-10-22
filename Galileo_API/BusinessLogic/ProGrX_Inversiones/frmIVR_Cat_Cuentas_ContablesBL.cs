using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrCatCuentasContablesBl
    {
        private readonly FrmIvrCatCuentasContablesDb _db;

        public FrmIvrCatCuentasContablesBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmIvrCatCuentasContablesDb(config);
        }

        public ErrorDto<IvrCatCuentasContablesInicialData?>
            IVR_Cat_Cuentas_Contables_Inicializar(
                int codEmpresa,
                IvrCatCuentasContablesInicializarRequest? request)
        {
            return _db.IVR_Cat_Cuentas_Contables_Inicializar(
                codEmpresa,
                request);
        }

        public ErrorDto<IvrCatCuentasContablesData?>
            IVR_Cat_Cuentas_Contables_Guardar(
                int codEmpresa,
                IvrCatCuentasContablesGuardarRequest? request)
        {
            return _db.IVR_Cat_Cuentas_Contables_Guardar(
                codEmpresa,
                request);
        }
    }
}