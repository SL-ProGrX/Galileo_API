using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSeguimientoRetencionesBl
    {
        private readonly FrmCrSeguimientoRetencionesDb _db;

        public FrmCrSeguimientoRetencionesBl(IConfiguration config)
        {
            _db = new FrmCrSeguimientoRetencionesDb(config);
        }

        public ErrorDto<CrSeguimientoRetencionesPantallaData> CR_SeguimientoRetenciones_Inicializar(
            int codEmpresa,
            CrSeguimientoRetencionesInicializarRequest request)
            => _db.CR_SeguimientoRetenciones_Inicializar(codEmpresa, request);

        public ErrorDto CR_SeguimientoRetenciones_Guardar(
            int codEmpresa,
            CrSeguimientoRetencionesGuardarRequest request)
            => _db.CR_SeguimientoRetenciones_Guardar(codEmpresa, request);

        public ErrorDto CR_SeguimientoRetenciones_Eliminar(
            int codEmpresa,
            CrSeguimientoRetencionesEliminarRequest request)
            => _db.CR_SeguimientoRetenciones_Eliminar(codEmpresa, request);
    }
}