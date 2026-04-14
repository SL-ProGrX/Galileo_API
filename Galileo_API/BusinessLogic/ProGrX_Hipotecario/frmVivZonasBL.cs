using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivZonasBl
    {
        private readonly FrmVivZonasDb _db;

        public FrmVivZonasBl(IConfiguration config)
            => _db = new FrmVivZonasDb(config);

        public ErrorDto<List<VivZonaData>> VivZonas_Lista_Obtener(int codEmpresa)
        {
            return _db.VivZonas_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Provincias_Obtener(int codEmpresa)
        {
            return _db.Provincias_Obtener(codEmpresa);
        }

        public ErrorDto<List<VivZonaCantonData>> Cantones_Obtener(
            int codEmpresa, int idZona, string provincia, bool soloAsignadas)
        {
            return _db.Cantones_Obtener(codEmpresa, idZona, provincia, soloAsignadas);
        }

        public ErrorDto VivZonas_Asignar(
            int codEmpresa, int idZona, string provincia, string canton, string usuario, bool isChecked)
        {
            return _db.VivZonas_Asignar(codEmpresa, idZona, provincia, canton, usuario, isChecked);
        }

        public ErrorDto VivZonas_TodosCantones_Asignar(
            int codEmpresa, int idZona, string provincia, string usuario)
        {
            return _db.VivZonas_TodosCantones_Asignar(codEmpresa, idZona, provincia, usuario);
        }

        public ErrorDto VivZonas_Guardar(int codEmpresa, string usuario, VivZonaData request)
        {
            return _db.VivZonas_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto VivZonas_Eliminar(int codEmpresa, int idZona, string usuario)
        {
            return _db.VivZonas_Eliminar(codEmpresa, idZona, usuario);
        }
    }
}
