using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsOpcionesBl
    {
        readonly FrmUsOpcionesDb OpcionesDB;

        public FrmUsOpcionesBl(IConfiguration config)
        {
            OpcionesDB = new FrmUsOpcionesDb(config);
        }

        public List<ModuloDto> Modulo_ObtenerTodos()
        {
            return OpcionesDB.Modulo_ObtenerTodos();
        }

        public List<FormularioDto> Formulario_ObtenerTodos(int modulo)
        {
            return OpcionesDB.Formulario_ObtenerTodos(modulo);
        }

        public List<OpcionDto> Opcion_ObtenerTodos(int modulo, string formulario)
        {
            return OpcionesDB.Opcion_ObtenerTodos(modulo, formulario);
        }

        public ErrorDto Opcion_Eliminar(string codigo, string formulario, int modulo)
        {
            return OpcionesDB.Opcion_Eliminar(codigo, formulario, modulo);
        }

        public ErrorDto Opcion_Guardar(OpcionDto request)
        {
            return OpcionesDB.Opcion_Guardar(request);
        }
    }
}
