using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvVentasEnLineaSincBl
    {
        private readonly FrmInvVentasEnLineaSincDb _db;

        public FrmInvVentasEnLineaSincBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmInvVentasEnLineaSincDb(config);
        }

        public ErrorDto<InvVentasEnLineaSincData>
            INV_VentasEnLineaSinc_CatalogoExistencias_Sincronizar(
                int codEmpresa,
                InvVentasEnLineaSincRequest? request)
        {
            return _db
                .INV_VentasEnLineaSinc_CatalogoExistencias_Sincronizar(
                    codEmpresa,
                    request);
        }

        public ErrorDto<InvVentasEnLineaSincData>
            INV_VentasEnLineaSinc_VentasExistencias_Sincronizar(
                int codEmpresa,
                InvVentasEnLineaSincRequest? request)
        {
            return _db
                .INV_VentasEnLineaSinc_VentasExistencias_Sincronizar(
                    codEmpresa,
                    request);
        }
    }
}