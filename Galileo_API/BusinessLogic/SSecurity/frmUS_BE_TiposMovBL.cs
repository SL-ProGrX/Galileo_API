using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsBeTiposMovBl
    {
        readonly FrmUsBeTiposMovDb BE_TiposMovDB;

        public FrmUsBeTiposMovBl(IConfiguration config)
        {
            BE_TiposMovDB = new FrmUsBeTiposMovDb(config);
        }

        public List<MovimientoBE> MovimientoBE_ObtenerTodos(int modulo)
        {
            return BE_TiposMovDB.MovimientoBE_ObtenerTodos(modulo);
        }

        public ErrorDto MovimientoBE_Guardar(MovimientoBE request)
        {
            return BE_TiposMovDB.MovimientoBE_Guardar(request);
        }

        public ErrorDto MovimientoBE_Eliminar(string movimiento, int modulo)
        {
            return BE_TiposMovDB.MovimientoBE_Eliminar(movimiento, modulo);
        }

    }
}
