using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrDigitalCatalogosBl
    {
        private readonly FrmCrDigitalCatalogosDb _db;

        public FrmCrDigitalCatalogosBl(IConfiguration config)
        {
            _db = new FrmCrDigitalCatalogosDb(config);
        }

        public ErrorDto<List<CrDigitalCatalogoData>> CrDigitalCatalogos_Obtener(
            int codEmpresa,
            string tipo)
        {
            return _db.CrDigitalCatalogos_Obtener(codEmpresa, tipo);
        }

        public ErrorDto CrDigitalCatalogos_Guardar(
            int codEmpresa,
            string usuario,
            string tipo,
            CrDigitalCatalogoData request)
        {
            return _db.CrDigitalCatalogos_Guardar(
                codEmpresa,
                usuario,
                tipo,
                request);
        }

        public ErrorDto CrDigitalCatalogos_Eliminar(
            int codEmpresa,
            string usuario,
            string tipo,
            short id)
        {
            return _db.CrDigitalCatalogos_Eliminar(
                codEmpresa,
                usuario,
                tipo,
                id);
        }
    }
}
