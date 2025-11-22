using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsModulosBl
    {
        readonly FrmUsModulosDb ModulosDB;

        public FrmUsModulosBl(IConfiguration config)
        {
            ModulosDB = new FrmUsModulosDb(config);
        }

        public List<ModuloDto> Modulo_ObtenerTodos()
        {
            return ModulosDB.Modulo_ObtenerTodos();
        }

        public ErrorDto Modulo_Eliminar(int request)
        {

            return ModulosDB.Modulo_Eliminar(request);
        }

        public ErrorDto Modulo_Guardar(ModuloDto request)
        {
            return ModulosDB.Modulo_Guardar(request);
        }
    }
}
