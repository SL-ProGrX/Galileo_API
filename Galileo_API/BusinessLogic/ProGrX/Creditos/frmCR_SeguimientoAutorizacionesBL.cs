using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSeguimientoAutorizacionesBl
    {
        private readonly FrmCrSeguimientoAutorizacionesDb _db;

        public FrmCrSeguimientoAutorizacionesBl(IConfiguration config)
        {
            _db = new FrmCrSeguimientoAutorizacionesDb(config);
        }

        public ErrorDto<CrSeguimientoAutorizacionesDetalleData?> Cr_SeguimientoAutorizaciones_Detalle_Obtener(
            int codEmpresa,
            int operacion)
            => _db.Cr_SeguimientoAutorizaciones_Detalle_Obtener(codEmpresa, operacion);

        public ErrorDto Cr_SeguimientoAutorizaciones_Autorizar(
            int codEmpresa,
            CrSeguimientoAutorizacionesAutorizarRequest request)
            => _db.Cr_SeguimientoAutorizaciones_Autorizar(codEmpresa, request);
    }
}