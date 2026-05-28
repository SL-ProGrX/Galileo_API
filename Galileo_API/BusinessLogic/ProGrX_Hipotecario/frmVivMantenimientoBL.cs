using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivMantenimientoBl
    {
        private readonly FrmVivMantenimientoDb _db;

        public FrmVivMantenimientoBl(IConfiguration config)
            => _db = new FrmVivMantenimientoDb(config);

        public static ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_ArbolInicial_Obtener()
        {
            return FrmVivMantenimientoDb.VivMantenimiento_ArbolInicial_Obtener();
        }

        public ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_NodosHijos_Obtener(int codEmpresa, string tag, string key)
        {
            return _db.VivMantenimiento_NodosHijos_Obtener(codEmpresa, tag, key);
        }

        public ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_Lista_Obtener(int codEmpresa, string tag, string key)
        {
            return _db.VivMantenimiento_Lista_Obtener(codEmpresa, tag, key);
        }

        public ErrorDto VivMantenimiento_ZonaContacto_Asignar(int codEmpresa, VivMantenimientoZonaContactoAsignarRequest request)
        {
            return _db.VivMantenimiento_ZonaContacto_Asignar(codEmpresa, request);
        }
    }
}
