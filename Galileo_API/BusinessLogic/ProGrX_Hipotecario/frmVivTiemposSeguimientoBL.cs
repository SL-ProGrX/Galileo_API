using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivTiemposSeguimientosBl
    {
        private readonly FrmVivTiemposSeguimientoDb _db;

        public FrmVivTiemposSeguimientosBl(IConfiguration config)
            => _db = new FrmVivTiemposSeguimientoDb(config);

        public ErrorDto<List<VivTiemposSeguimientoData>> VivTiemposSeguimiento_Obtener(int codEmpresa)
        {
            return _db.VivTiemposSeguimiento_Obtener(codEmpresa);
        }

        public ErrorDto VivTiemposSeguimiento_Guardar(int codEmpresa, VivTiemposSeguimientoData request)
        {
            return _db.VivTiemposSeguimiento_Guardar(codEmpresa, request);
        }
    }
}
